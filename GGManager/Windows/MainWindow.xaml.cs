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
                _contentStore.OpenDatabase(lastOpenedDatabasePath);
            }

            //версия приложения в загаловке
            string? _appVersion = Assembly.GetExecutingAssembly().GetName()?.Version?.ToString();
            Title += " " + _appVersion;
        }

        private void SelectedSegmentChanged(Segment segment)
        {
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
            string filePath = FileService.SelectDatabaseFilePath();
            if (string.IsNullOrEmpty(filePath)) return;

            _contentStore.OpenDatabase(filePath);
        }

        private void mnuCreateDatabase_Click(object sender, RoutedEventArgs e)
        {
            string filePath = FileService.SelectNewDatabaseFilePath();
            if (string.IsNullOrEmpty(filePath)) return;
            _contentStore.CreateDatabase(filePath);
            Task.Delay(200);
            var dbInfo = new DbInfoWindow();
            dbInfo.ShowDialog();
        }

        private void mnuDatabaseInfo_Click(object sender, RoutedEventArgs e)
        {
            var dbInfoWindow = new DbInfoWindow();
            dbInfoWindow.ShowDialog();
        }

        private void mnuAbout_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        private void OnDatabaseOpened()
        {
            if (!_contentStore.DbContext.DbMetas.Any())
            {
                _contentStore.SetDbMeta();
                var dbInfo = new DbInfoWindow();
                dbInfo.ShowDialog();
            }

            _contentStore.SelectedSegment = _contentStore.DbContext.Segments.FirstOrDefault();

            lblChooseDb.Visibility = Visibility.Collapsed;
            lblChooseSegment.Visibility = Visibility.Visible;
            ucSegmentList.Visibility = Visibility.Visible;
            mnuDatabaseInfo.IsEnabled = true;

            SetTitle();
        }

        private void mnuImportDatabase_Click(object sender, RoutedEventArgs e)
        {
            string filePath = FileService.SelectDatabaseFilePath();
            if (!File.Exists(filePath))
            {
                return;
            }
            _contentStore.ImportDatabase(filePath);
        }
        #endregion

        private async void mnuCheckUpdates_Click(object sender, RoutedEventArgs e)
        {
            var updateService = new UpdateService(_settingsService);
            IsEnabled = false;
            await _updateService.UpdateMyApp("manager");
            IsEnabled = true;
        }

        private void mnuSetLanguageChechen_Click(object sender, RoutedEventArgs e)
        {
            _settingsService.SetValue("uiLanguageCode", "uk");
            Translations.SetToCulture("uk");
            Translations.RestartApp();
        }

        private void mnuSetLanguageRussian_Click(object sender, RoutedEventArgs e)
        {
            _settingsService.SetValue("uiLanguageCode", "ru");
            Translations.SetToCulture("ru");
            Translations.RestartApp();
        }

        private void MnuOpenLogFiles_Click(object sender, RoutedEventArgs e)
        {
           
            string appName = Assembly.GetEntryAssembly().GetName().Name;

            string logsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                appName,
                "current",
                "logs"
            );

            if (!Directory.Exists(logsPath))
            {
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
