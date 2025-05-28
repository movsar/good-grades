using Microsoft.Win32;
using Shared;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

public static class WebView2Installer
{
    // URL для скачивания WebView2
    private const string x86Url = "https://movsar.dev/ggassets/Microsoft.WebView2.FixedVersionRuntime.135.0.3179.98.x86.cab";
    private const string x64Url = "https://movsar.dev/ggassets/Microsoft.WebView2.FixedVersionRuntime.135.0.3179.98.x64.cab";
   

    public static async Task InstallWebView2IfNeeded()
    {
        if (IsWebView2Installed())
            return;

        if (!HasInternetConnection())
        {
            MessageBox.Show("Для установки WebView2 требуется подключение к интернету.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var cts = new CancellationTokenSource();
        DownloadProgressWindow progressWindow = null;

        try
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                progressWindow = new DownloadProgressWindow(cts);
                progressWindow.Show();
            });

            string tempDir = Path.Combine(Path.GetTempPath(), "WebView2Install");
            Directory.CreateDirectory(tempDir);

            string cabPath = Path.Combine(tempDir, "webview2.cab");
            string extractDir = Path.Combine(tempDir, "extracted");
            Directory.CreateDirectory(extractDir);

            string downloadUrl = Environment.Is64BitOperatingSystem ? x64Url : x86Url;

            await DownloadFileAsync(downloadUrl, cabPath, progressWindow, cts.Token);
            await RunProcessAsync("expand.exe", $"-F:* \"{cabPath}\" \"{extractDir}\"", cts.Token);

            // Находим папку с WebView2
            string[] subdirs = Directory.GetDirectories(extractDir);
            if (subdirs.Length == 0)
            {
                MessageBox.Show("Папка WebView2 не найдена после распаковки!");
                return;
            }

            string webView2Source = subdirs[0];
            string webView2Target = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WebView2");

            DirectoryCopy(webView2Source, webView2Target, true);

            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WebView2", "installed.marker"), "OK");
            MessageBox.Show("WebView2 установлен успешно!");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка установки WebView2: {ex.Message}");
        }
        finally
        {
            if (progressWindow != null)
            {
                await Application.Current.Dispatcher.InvokeAsync(() => progressWindow.Close());
            }

            try
            {
                string tempDir = Path.Combine(Path.GetTempPath(), "WebView2Install");
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
            catch { }
        }
    }

    private static bool HasInternetConnection()
    {
        try
        {
            using (var client = new WebClient())
            using (client.OpenRead("http://www.google.com"))
            {
                return true;
            }
        }
        catch
        {
            return false;
        }
    }
    private static bool IsWebView2Installed()
    {
        return IsEvergreenWebView2Installed() || IsFixedVersionRuntimePresent();
    }

    private static bool IsFixedVersionRuntimePresent()
    {

        string exeDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)!;
        string runtimeDir = Path.Combine(exeDir, "WebView2");

        // Если папка WebView2 существует — считаем, что WebView2 установлен
        return Directory.Exists(runtimeDir);

    }

    private static bool IsEvergreenWebView2Installed()
    {
        try
        {
            using var key1 = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}");
            using var key2 = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}");
            return (key1?.GetValue("pv") != null) || (key2?.GetValue("pv") != null);
        }
        catch
        {
            return false;
        }
    }

    private static async Task DownloadFileAsync(string url, string destinationPath, DownloadProgressWindow progressWindow, CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
        var canReportProgress = totalBytes != -1;

        using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);

        var buffer = new byte[81920];
        long totalRead = 0;
        int read;

        while ((read = await contentStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            totalRead += read;

            if (canReportProgress)
            {
                double progress = (totalRead * 100d) / totalBytes;
                await Application.Current.Dispatcher.InvokeAsync(() => progressWindow.UpdateProgress(progress));
            }

            if (cancellationToken.IsCancellationRequested)
                break;
        }
    }

    private static Task RunProcessAsync(string fileName, string arguments, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<object>();

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
            },
            EnableRaisingEvents = true
        };

        process.Exited += (s, e) =>
        {
            if (process.ExitCode == 0)
                tcs.TrySetResult(null);
            else
                tcs.TrySetException(new Exception($"Процесс {fileName} завершился с кодом {process.ExitCode}"));

            process.Dispose();
        };

        cancellationToken.Register(() =>
        {
            try { if (!process.HasExited) process.Kill(); } catch { }
        });

        if (!process.Start())
            throw new InvalidOperationException($"Не удалось запустить процесс {fileName}");

        return tcs.Task;
    }

    private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
    {
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Источник не найден: {sourceDirName}");

        DirectoryInfo[] dirs = dir.GetDirectories();
        Directory.CreateDirectory(destDirName);

        foreach (FileInfo file in dir.GetFiles())
        {
            string targetPath = Path.Combine(destDirName, file.Name);
            file.CopyTo(targetPath, true);
        }

        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                string newDest = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, newDest, copySubDirs);
            }
        }
    }
}
