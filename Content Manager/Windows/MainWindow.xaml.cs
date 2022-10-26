﻿using Content_Manager.Services;
using Content_Manager.Stores;
using Content_Manager.UserControls;
using Content_Manager.Windows;
using System.Windows;

namespace Content_Manager
{
    public partial class MainWindow : Window
    {
        private readonly ContentStore _contentStore;
        private SegmentControl _segmentControl;
        public MainWindow(ContentStore contentStore)
        {
            InitializeComponent();
            DataContext = this;
            _contentStore = contentStore;
            _contentStore.ContentStoreInitialized += ContentStoreInitialized;
            _contentStore.SelectedSegmentChanged += SelectedSegmentChanged;
        }

        private void SelectedSegmentChanged(Data.Models.Segment obj)
        {

            if (obj != null)
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

        private void ContentStoreInitialized()
        {
            lblChooseDb.Visibility = Visibility.Collapsed;
            lblChooseSegment.Visibility = Visibility.Visible;
            ucSegmentList.Visibility = Visibility.Visible;
            mnuDatabaseInfo.IsEnabled = true;
        }

        private void mnuOpenDatabase_Click(object sender, RoutedEventArgs e)
        {
            string filePath = FileService.OpenFilePath("Файлы Баз Данных (.sgb) | *.sgb;");
            if (string.IsNullOrEmpty(filePath)) return;

            _contentStore.OpenDatabase(filePath);
        }

        private void mnuCreateDatabase_Click(object sender, RoutedEventArgs e)
        {
            string filePath = FileService.SaveFilePath("Файлы Баз Данных (.sgb) | *.sgb;");
            if (string.IsNullOrEmpty(filePath)) return;

            _contentStore.CreateDatabase(filePath);
        }
        private void mnuDatabaseInfo_Click(object sender, RoutedEventArgs e)
        {
            var dbInfoWindow = new DbInfoWindow();
            dbInfoWindow.ShowDialog();
        }

    }
}
