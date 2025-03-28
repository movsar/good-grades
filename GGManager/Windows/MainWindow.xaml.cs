using GGManager.Services;
using GGManager.Stores;
using GGManager.UserControls;
using GGManager.Windows;
using Data.Entities;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Data.Services;
using System.Diagnostics;
using Shared;
using Shared.Services;
using System;
using Serilog;

namespace GGManager
{
    public partial class MainWindow : Window
    {
        private readonly ContentStore _contentStore;
        private readonly SettingsService _settingsService;
        private readonly UpdateService _updateService;

        public MainWindow(ContentStore contentStore, SettingsService settingsService)
        {          
            InitializeComponent();
            DataContext = this;
            Log.Information("MainWindow initialized");

            //инициализация и подписка на события
            _settingsService = settingsService;
            _updateService = new UpdateService(_settingsService);
            _contentStore = contentStore;
            _contentStore.SelectedSegmentChanged += SelectedSegmentChanged;
            _contentStore.CurrentDatabaseChanged += OnDatabaseOpened;

            //открытие последней открытой базы данных при запуске
            var lastOpenedDatabasePath = _settingsService.GetValue("lastOpenedDatabasePath");
            if (!string.IsNullOrEmpty(lastOpenedDatabasePath) && File.Exists(lastOpenedDatabasePath))
            {
                Log.Debug("Found last opened database path: {path}", lastOpenedDatabasePath);
                _contentStore.OpenDatabase(lastOpenedDatabasePath);
            }

            //версия приложения в загаловке
            string? _appVersion = Assembly.GetExecutingAssembly().GetName()?.Version?.ToString();
            Title += " " + _appVersion;
        }

        private void SelectedSegmentChanged(Segment segment)
        {
            Log.Debug("Segment changed to {segmentId}", segment?.Id);
            if (segment != null)
            {
                lblChooseSegment.Visibility = Visibility.Hidden;

                ucSegmentControlParent.Children.Clear();
                ucSegmentControlParent.Children.Add(new SegmentControl());
            }
            else
            {
                ucSegmentControlParent.Children.Clear();
                lblChooseSegment.Visibility = Visibility.Visible;
            }
        }

        private void SetTitle(string? title = null)
        {
            string? _appVersion = Assembly.GetExecutingAssembly().GetName()?.Version?.ToString();
            Title = $"Good Grades | {title ?? _contentStore.DbContext.DbMetas.First().Title}";
        }

        #region Database Operations

        private void mnuOpenDatabase_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("User initiated database open");
            string filePath = FileService.SelectDatabaseFilePath();
            if (string.IsNullOrEmpty(filePath)) 
            {
                Log.Debug("database isn't opened");
                return; 
            }

            _contentStore.OpenDatabase(filePath);
        }

        private void mnuCreateDatabase_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("User initiated database creation");
            string filePath = FileService.SelectNewDatabaseFilePath();
            if (string.IsNullOrEmpty(filePath)) 
            {
                Log.Debug("database isn't created");
                return;
            }
            _contentStore.CreateDatabase(filePath);
            Task.Delay(200);
            var dbInfo = new DbInfoWindow();
            dbInfo.ShowDialog();
        }

        private void mnuDatabaseInfo_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("Info about Database opened successfully");
            var dbInfoWindow = new DbInfoWindow();
            dbInfoWindow.ShowDialog();
        }

        private void mnuAbout_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("About window opened");
            var aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        private void OnDatabaseOpened()
        {
            Log.Information("Database opened");
            if (!_contentStore.DbContext.DbMetas.Any())
            {
                Log.Debug("No DbMeta found, creating new one");
                _contentStore.SetDbMeta();
                var dbInfo = new DbInfoWindow();
                dbInfo.ShowDialog();
            }

            _contentStore.SelectedSegment = _contentStore.DbContext.Segments.FirstOrDefault();
            Log.Debug($"Selected segment: {_contentStore.SelectedSegment?.Id}");

            lblChooseDb.Visibility = Visibility.Collapsed;
            lblChooseSegment.Visibility = Visibility.Visible;
            ucSegmentList.Visibility = Visibility.Visible;
            mnuDatabaseInfo.IsEnabled = true;

            SetTitle();
        }

        private void mnuImportDatabase_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("Attempt to import Database");
            string filePath = FileService.SelectDatabaseFilePath();
            if (!File.Exists(filePath))
            {
                return;
            }
            _contentStore.ImportDatabase(filePath);
            Log.Debug($"Imported database {filePath}");
        }
        #endregion

        private async void mnuCheckUpdates_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("User initiated update check");
            var updateService = new UpdateService(_settingsService);
            IsEnabled = false;
            await _updateService.UpdateMyApp("manager");
            IsEnabled = true;
        }

        private void mnuSetLanguageChechen_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("set Chechen Language");
            _settingsService.SetValue("uiLanguageCode", "uk");
            Translations.SetToCulture("uk");
            Translations.RestartApp();
        }

        private void mnuSetLanguageRussian_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("set Russian Language");
            _settingsService.SetValue("uiLanguageCode", "ru");
            Translations.SetToCulture("ru");
            Translations.RestartApp();
        }

        private void MnuOpenLogFiles_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("Opening Log Files");
            string appName = Assembly.GetEntryAssembly().GetName().Name;

            string logsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                appName,
                "current",
                "logs"
            );

            if (!Directory.Exists(logsPath))
            {
                Log.Debug("logs does not found, creating directory");
                Directory.CreateDirectory(logsPath);
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = logsPath,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            _contentStore?.Dispose();
            base.OnClosed(e);
        }

    }
}
