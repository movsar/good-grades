﻿using Data.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Shared.Controls;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Velopack;

namespace Shared.Services
{
    public class UpdateService
    {
        private readonly SettingsService _settingsService;

        public UpdateService(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task UpdateMyApp(string module)
        {
            try
            {
                Log.Information($"Before Checking for updates");
                var latestRelease = await GitHubService.GetLatestRelease(module);
                var latestVersion = Regex.Match(latestRelease.Name!, @"\d+\.\d+\.\d+").Value.Trim();
                var setupAsset = latestRelease.Assets
                        .Where(a => a.Name!.ToLower().Contains("setup"))
                        .FirstOrDefault();

                var currentVersion = Assembly.GetEntryAssembly()?.GetName().Version;
                if (currentVersion == null)
                {
                    throw new Exception("Couldn't fetch current version information");
                }
                var currentVersionAsString = $"{currentVersion.Major}.{currentVersion.Minor}.{currentVersion.Build}".Trim();
                if (currentVersionAsString.Equals(latestVersion))
                {
                    OkDialog.Show(Translations.GetValue("LastVersionInstalled"));
                    return;
                }

                if (YesNoDialog.Show(string.Format(Translations.GetValue("AvailableNewVersion"), latestVersion)) != MessageBoxResult.Yes)
                {
                    return;
                }

                // Download new version
                var filePath = await GitHubService.DownloadUpdate(setupAsset);
                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    // Install new version and restart app
                    Process.Start(filePath);
                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/movsar/good-grades/releases",
                    UseShellExecute = true,
                });
            }
        }

        public async Task AutoUpdate(string module)
        {
            try
            {
                var skipVersion = _settingsService.GetValue("SkipVersion");

                var latestRelease = await GitHubService.GetLatestRelease(module);
                var latestVersion = Regex.Match(latestRelease.Name!, @"\d+\.\d+\.\d+").Value.Trim();

                if (skipVersion == latestVersion)
                {
                    Debug.WriteLine("Пользователь пропустил эту версию.");
                    return;
                }

               if( SkipYesNoDialog.Show(string.Format(Translations.GetValue("AvailableNewVersion"), latestVersion)) == MessageBoxResult.No){
                    return;
                }

                else if (SkipYesNoDialog.Show(string.Format(Translations.GetValue("AvailableNewVersion"), latestVersion)) == MessageBoxResult.Cancel)
                {
                    _settingsService.SetValue("SkipVersion", latestVersion);
                    return;
                }

                // Запуск обновления
                await UpdateMyApp(module);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка автообновления: {ex.Message}");
            }
        }
    }
}

