using Data.Services;
using Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Shared.Services;
using Shared;
using System.IO;
using GGPlayer.Services;
using System.Diagnostics;
using System.Reflection;

namespace GGPlayer.Pages
{
    public partial class StartPage : Page
    {
        private readonly SettingsService _settingsService;
        private readonly Storage _storage;
        private readonly ShellNavigationService _navigationService;
        private readonly UpdateService _updateService;

        public StartPage(SettingsService settingsService, Storage storage, ShellNavigationService navigationService)
        {
            InitializeComponent();
            DataContext = this;


            _storage = storage;
            _settingsService = settingsService;
            _updateService = new UpdateService(_settingsService);
            _navigationService = navigationService;

            try
            {
                LoadDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка инициализации", "Good Grades", MessageBoxButton.OK, MessageBoxImage.Error);
                Serilog.Log.Error(ex, ex.Message);
            }
        }
        public void LoadDatabase(bool restoreLatest = true)
        {
            // Get the database path
            var dbAbsolutePath = _settingsService.GetValue("lastOpenedDatabasePath");
            if (!restoreLatest || string.IsNullOrEmpty(dbAbsolutePath) || !File.Exists(dbAbsolutePath))
            {
                dbAbsolutePath = GetDatabasePath();
            }

            // If the user cancels and closes the window
            if (string.IsNullOrEmpty(dbAbsolutePath))
            {
                return;
            }

            //открытие последней открытой БД
            _settingsService.SetValue("lastOpenedDatabasePath", dbAbsolutePath);
            _storage.InitializeDbContext(dbAbsolutePath);
            btnGo.IsEnabled = true;

            // Set the background image for the class
            var dbMeta = _storage.DbContext.DbMetas.First();
            Title = "Good Grades: " + dbMeta.Title;
            if (!string.IsNullOrWhiteSpace(dbMeta.BackgroundImagePath))
            {
                BitmapImage logo = new BitmapImage();
                logo.BeginInit();
                var data = Storage.ReadDbAsset(dbMeta.BackgroundImagePath);

                logo.StreamSource = new MemoryStream(data);
                logo.EndInit();
                ImageBrush myBrush = new ImageBrush(logo);
                myBrush.Stretch = Stretch.UniformToFill;
                grdMain.Background = myBrush;
            }
        }
        private string GetDatabasePath()
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = Translations.GetValue("DBFiles");
            ofd.Multiselect = false;
            var result = ofd.ShowDialog();
            if (result.HasValue)
            {
                return ofd.FileName;
            }

            MessageBox.Show(Translations.GetValue("DBFileChoose"));
            return GetDatabasePath();
        }
      
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Создание и показ основного окна
            _navigationService.NavigateTo<MainPage>();
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        private void CloseProgram_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void OpenDatabase_Click(object sender, RoutedEventArgs e)
        {
            LoadDatabase(false);
        }

        private void mnuSetLanguageChechen_Click(object sender, RoutedEventArgs e)
        {
            _settingsService.SetValue("uiLanguageCode", "uk");
            Translations.RestartApp();
        }

        private void mnuSetLanguageRussian_Click(object sender, RoutedEventArgs e)
        {
            _settingsService.SetValue("uiLanguageCode", "ru");
            Translations.RestartApp();
        }

        private async void mnuCheckUpdates_Click(object sender, RoutedEventArgs e)
        {
            var updateService = new UpdateService(_settingsService);
            IsEnabled = false;
            await _updateService.UpdateMyApp("player");
            IsEnabled = true;
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

    }
}
