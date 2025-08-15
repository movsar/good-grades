using Data.Services;
using Microsoft.Win32;
using Shared;
using Shared.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

public class WebViewService
{
    // URL для скачивания WebView2
    private const string x86Url = "https://movsar.dev/ggassets/Microsoft.WebView2.FixedVersionRuntime.135.0.3179.98.x86.cab";
    private const string x64Url = "https://movsar.dev/ggassets/Microsoft.WebView2.FixedVersionRuntime.135.0.3179.98.x64.cab";

    private readonly SettingsService _settingsService;
    string _tempDir = Path.Combine(Path.GetTempPath(), "WebView2Install");

    public WebViewService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task InstallWebView2IfNeeded()
    {
        if (IsWebView2Installed())
        {
            return;
        }

        MessageBox.Show("Для корректной работы, нужно скачать и установить WebView2", "Good Grades", MessageBoxButton.OK, MessageBoxImage.Information);
        Process.Start(new ProcessStartInfo()
        {
            UseShellExecute = true,
            Verb = "open",
            FileName = "https://developer.microsoft.com/en-us/microsoft-edge/webview2/consumer?form=MA13LH"
        });
        //if (!HasInternetConnection())
        //{
        //    MessageBox.Show("Для установки WebView2 требуется подключение к интернету.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //    return;
        //}
        //MessageBox.Show("Будет произведено скачивание и установка дополнительных компонетов", "Good Grades", MessageBoxButton.OK, MessageBoxImage.Information);

        //try
        //{
        //    string extractDir = Path.Combine(_tempDir, "extracted");
        //    string cabPath = Path.Combine(_tempDir, "webview2.cab");

        //    Directory.CreateDirectory(_tempDir);
        //    Directory.CreateDirectory(extractDir);

        //    string downloadUrl = Environment.Is64BitOperatingSystem ? x64Url : x86Url;

        //    await NetworkService.DownloadUpdate(downloadUrl, cabPath);

        //    await RunProcessAsync("expand.exe", $"-F:* \"{cabPath}\" \"{extractDir}\"");

        //    string webView2Source = Directory.GetDirectories(extractDir)[0];
        //    string webView2Target = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WebView2");

        //    DirectoryCopy(webView2Source, webView2Target, true);
        //    _settingsService.SetValue("IsWebViewInstalled", "true");

        //    Directory.Delete(_tempDir, true);

        //    MessageBox.Show("WebView2 установлен успешно!");
        //}
        //catch (Exception ex)
        //{
        //    MessageBox.Show($"Ошибка установки WebView2: {ex.Message}");
        //}
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
    private bool IsWebView2Installed()
    {
        return IsEvergreenWebView2Installed() || _settingsService.GetValue("IsWebViewInstalled") == "true";
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

    private static Task RunProcessAsync(string fileName, string arguments)
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

        var cancellationToken = new CancellationToken();
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
