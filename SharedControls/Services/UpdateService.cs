using Serilog;
using Shared.Controls;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Velopack;

namespace Shared.Services
{
    public static class UpdateService
    {
        public static async Task UpdateMyApp(string releasesUrl)
        {
            try
            {
                var mgr = new UpdateManager(releasesUrl + "/RELEASES");

                // Check for new version
                var newVersion = await mgr.CheckForUpdatesAsync();
                if (newVersion == null)
                {
                    return;
                }

                Log.Information($"Before Checking for updates");
                Log.Debug($"future: {JsonSerializer.Serialize(newVersion)}");
                Log.Debug($"current: {JsonSerializer.Serialize(mgr.CurrentVersion)}");

                if (newVersion.TargetFullRelease.Version.Major == mgr.CurrentVersion!.Major
                    && newVersion.TargetFullRelease.Version.Minor == mgr.CurrentVersion!.Minor)
                {
                    OkDialog.Show(Translations.GetValue("LastVersionInstalled"), "Good Grades");
                    return;
                }

                if (YesNoDialog.Show(string.Format(Translations.GetValue("AvailableNewVersion"), newVersion)) == MessageBoxResult.Yes)
                {
                    // Download new version
                    await mgr.DownloadUpdatesAsync(newVersion);

                    // Install new version and restart app
                    mgr.ApplyUpdatesAndRestart(newVersion);
                }
            }
            catch (Exception ex)
            {
                ExceptionService.HandleError(ex, "Ошибка при попытаке обновления приложения");
            }
        }
    }
}
