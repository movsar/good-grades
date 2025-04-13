using GGManager.Services;
using GGManager.Stores;
using Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Windows;
using Serilog;
using System.IO;
using Shared.Services;
using Data.Services;
using Velopack;
using Shared;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace GGManager
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
                        services.AddTransient<FileService>();
                        services.AddSingleton<Storage>();
                        services.AddSingleton<MainWindow>();
                        services.AddSingleton<SettingsService>();
                        services.AddSingleton<UpdateService>();
                        services.AddSingleton<StylingService>();
                        services.AddSingleton<ContentStore>();
                    }).Build();
        }
        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            MessageBox.Show("Произошла непредвиденная ошибка", "Good Grades", MessageBoxButton.OK, MessageBoxImage.Error);
            Log.Error(e.Exception, e.Exception.Message);
            Shutdown();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show("Произошла непредвиденная ошибка", "Good Grades", MessageBoxButton.OK, MessageBoxImage.Error);
                Log.Error(ex, ex.Message);
                Shutdown();
            }
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            const string appMutexName = "GGManager_SingleInstance";

            bool isNewInstance;
            _appMutex = new Mutex(true, appMutexName, out isNewInstance);

            if (!isNewInstance)
            {
                MessageBox.Show("GGManager уже запущен.", "Good Grades", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }

            AppHost.Start();
            base.OnStartup(e);

            var apiService = new ApiService();
            Task.Run(() => apiService.SendLogsAsync());

            var settingsService = AppHost.Services.GetRequiredService<SettingsService>();
            settingsService.ApplyCommandLineArguments(e.Args);
            
            var uiLanguageCode = settingsService.GetValue("uiLanguageCode");
            Translations.SetToCulture(uiLanguageCode ?? "uk");

            var startUpForm = AppHost!.Services.GetRequiredService<MainWindow>();
            startUpForm.Show();
            var updateService = AppHost.Services.GetRequiredService<UpdateService>();
            await updateService.AutoUpdate("manager");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                _appMutex?.ReleaseMutex();
                _appMutex?.Dispose();
                Log.CloseAndFlush();
                AppHost!.StopAsync();
                base.OnExit(e);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during OnExit");
            }
        }
    }
}