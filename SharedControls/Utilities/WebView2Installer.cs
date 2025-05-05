using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;

public static class WebView2Installer
{
    // URL для скачивания WebView2
    private const string x86Url = "https://movsar.dev/ggassets/Microsoft.WebView2.FixedVersionRuntime.135.0.3179.98.x86.cab";
    private const string x64Url = "https://movsar.dev/ggassets/Microsoft.WebView2.FixedVersionRuntime.135.0.3179.98.x64.cab";


    public static void InstallWebView2IfNeeded()
    {
        if (IsWebView2Installed()) return;

        try
        {
            DownloadAndInstallWebView2();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка установки WebView2: {ex.Message}");
        }
    }

    private static bool IsWebView2Installed()
    {
        try
        {
            // Проверка через реестр
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}"))
            {
                return key?.GetValue("pv") != null;
            }
        }
        catch
        {
            return false;
        }
    }

    private static void DownloadAndInstallWebView2()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "WebView2Install");
        string cabPath = Path.Combine(tempDir, "webview2.cab");
        string extractDir = Path.Combine(tempDir, "extracted");

        try
        {
            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(extractDir);

            // Определяем архитектуру
            string downloadUrl = Environment.Is64BitOperatingSystem ? x64Url : x86Url;

            // Скачиваем CAB
            using (var client = new WebClient())
            {
                client.DownloadFile(downloadUrl, cabPath);
            }

            // Извлекаем CAB
            var expandProcess = Process.Start(new ProcessStartInfo
            {
                FileName = "expand.exe",
                Arguments = $"-F:* {cabPath} {extractDir}",
                UseShellExecute = false,
                CreateNoWindow = true
            });
            expandProcess.WaitForExit();

            // Устанавливаем MSI
            foreach (var file in Directory.GetFiles(extractDir, "*.msi"))
            {
                var installProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = "msiexec.exe",
                    Arguments = $"/i \"{file}\" /qn",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                installProcess.WaitForExit();
            }
        }
        finally
        {
            // Очистка временных файлов
            try { Directory.Delete(tempDir, true); } catch { }
        }
    }
}