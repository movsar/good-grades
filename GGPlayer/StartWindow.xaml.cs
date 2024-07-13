﻿
using Data;
using Data.Entities;
using Data.Services;
using GGManager;
using GGManager.Stores;
using GGPlayer.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Shared;
using Shared.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace GGPlayer
{
    public partial class StartWindow : Window
    {
        private readonly SettingsService _settingsService;
        private readonly Storage _storage;
        private readonly ContentStore _contentStore;

        public StartWindow()
        {

            InitializeComponent();
            DataContext = this;
            
            // Initialize fields
            _settingsService = App.AppHost!.Services.GetRequiredService<SettingsService>();
            _storage = App.AppHost!.Services.GetRequiredService<Storage>();

            try
            {
                LoadDatabase();

                // Set the background image for the class
                var dbMeta = _storage.DbContext.DbMetas.First();
                if (dbMeta.BackgroundImage?.Length > 0)
                {
                    BitmapImage logo = new BitmapImage();
                    logo.BeginInit();
                    logo.StreamSource = new MemoryStream(dbMeta.BackgroundImage);
                    logo.EndInit();
                    ImageBrush myBrush = new ImageBrush();
                    myBrush.ImageSource = logo;
                    pnlMain.Background = myBrush;
                }
            }
            catch (Exception ex)
            {
                ExceptionService.HandleError(ex, ex.Message);
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

            _settingsService.SetValue("lastOpenedDatabasePath", dbAbsolutePath);
            _storage.SetDatabaseConfig(dbAbsolutePath);
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
            var shellWindow = new ShellWindow();
            shellWindow.Show();

            // Закрытие стартового окна
            this.Close();
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Show();
        }

        private void CloseProgram_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void OpenDatabase_Click(object sender, RoutedEventArgs e)
        {
            LoadDatabase();
        }
    }
}
