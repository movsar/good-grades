using Data;
using Data.Services;
using GGPlayer.Pages;
using GGPlayer.Pages.Assignments;
using GGPlayer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Shared;
using Shared.Controls;
using Shared.Controls.Assignments;
using Shared.Services;
using Shared.Utilities;
using System.IO;
using System.Windows;


namespace GGPlayer
{
    public partial class App : Application
    {
        private static Mutex? _appMutex;

        public static IHost? AppHost { get; private set; }
        public App()
        {
            // Handle unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            AppHost = Host.CreateDefaultBuilder()
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddSingleton<ShellWindow>();

                        services.AddSingleton<Storage>();
                        services.AddSingleton<SettingsService>();
                        services.AddSingleton<UpdateService>();
                        services.AddSingleton<StylingService>();
                        services.AddSingleton<ShellNavigationService>();

                        services.AddSingleton<StartPage>();
                        services.AddSingleton<MaterialViewerPage>();
                        services.AddSingleton<MainPage>();
                        services.AddSingleton<SegmentPage>();
                        services.AddSingleton<AssignmentViewerPage>();
                        services.AddSingleton<AssignmentsPage>();
                        services.AddSingleton<WebViewService>();

                        services.AddSingleton<MatchingAssignmentControl>();
                        services.AddSingleton<SelectingAssignmentControl>();
                        services.AddSingleton<BuildingAssignmentControl>();
                        services.AddSingleton<FillingAssignmentControl>();
                        services.AddSingleton<TestingAssignmentControl>();
                        services.AddSingleton<MaterialViewerControl>();
                        services.AddSingleton<AssignmentViewerControl>();
                    }).Build();
        }
        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // Log the exception
            Log.Error(e.Exception, "An unhandled exception occurred.");
            e.Handled = true;
            MessageBox.Show($"Произошла непредвиденная ошибка {e.Exception.Message}");
            Application.Current.Shutdown();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                // Log the exception
                Log.Error(ex, "An unhandled domain exception occurred.");
                MessageBox.Show($"Произошла непредвиденная ошибка {ex.Message}");
                Application.Current.Shutdown();
            }
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            const string appMutexName = "GGPlayer_SingleInstance";

            bool isNewInstance;
            _appMutex = new Mutex(true, appMutexName, out isNewInstance);

            if (!isNewInstance)
            {
                MessageBox.Show("GGPlayer уже запущен.", "Good Grades", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }

            AppHost!.Start();
            base.OnStartup(e);

            var settingsService = AppHost!.Services.GetRequiredService<SettingsService>();
            var startWindow = AppHost!.Services.GetRequiredService<ShellWindow>();
            var updateService = AppHost!.Services.GetRequiredService<UpdateService>();
            var webViewService = AppHost!.Services.GetRequiredService<WebViewService>();

            settingsService.ApplyCommandLineArguments(e.Args);

            var uiLanguageCode = settingsService.GetValue("uiLanguageCode");
            Translations.SetToCulture(uiLanguageCode ?? "uk");

            startWindow.Show();
            await updateService.AutoUpdate("player");

            try
            {
                await webViewService.InstallWebView2IfNeeded();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка установки WebView2: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            _appMutex?.ReleaseMutex();
            _appMutex?.Dispose();
            Log.CloseAndFlush();
            await AppHost!.StopAsync();
            base.OnExit(e);
        }
    }
}