﻿using Content_Manager.Models;
using Content_Manager.Services;
using Content_Manager.Stores;
using Content_Manager.Windows;
using Data.Interfaces;
using Data.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Content_Manager.UserControls {
    public partial class ReadingMaterialControl : UserControl {
        ContentStore ContentStore => App.AppHost!.Services.GetRequiredService<ContentStore>();
        StylingService StylingService => App.AppHost!.Services.GetRequiredService<StylingService>();

        private const string TitleHintText = "Введите название материала";
        private FormCompletionInfo _formCompletionInfo;
        public ReadingMaterial Material { get; set; }

        #region Reactions
        private void OnFormStatusChanged(bool isReady) {
            if (isReady) {
                btnSave.Visibility = Visibility.Visible;
                btnPreview.Visibility = Visibility.Visible;
            } else {
                btnSave.Visibility = Visibility.Collapsed;
                btnPreview.Visibility = Visibility.Collapsed;
            }
        }
        private void OnTitleSet(bool isSet) {
            _formCompletionInfo.Update(nameof(Material.Title), isSet);
        }
        private void OnContentSet(bool isSet = true) {
            btnUploadFromFile.Background = StylingService.StagedBrush;

            _formCompletionInfo.Update(nameof(Material.Content), isSet);
        }
        #endregion

        #region Initialization
        private void SetUiForNewMaterial() {
            btnPreview.Visibility = Visibility.Collapsed;
            btnDelete.Visibility = Visibility.Collapsed;
            btnSave.Visibility = Visibility.Collapsed;

            btnPreview.Background = StylingService.StagedBrush;
        }
        private void SetUiForExistingMaterial() {
            btnUploadFromFile.Background = StylingService.ReadyBrush;
            btnPreview.Background = StylingService.ReadyBrush;
            btnDelete.Visibility = Visibility.Visible;
        }

        private void SharedInitialization(bool isExistingMaterial = false) {
            DataContext = this;
            
            var propertiesToWatch = new string[] { nameof(Material.Title), nameof(Material.Content) };
            _formCompletionInfo = new FormCompletionInfo(propertiesToWatch, isExistingMaterial);
            _formCompletionInfo.StatusChanged += OnFormStatusChanged;
        }
        public ReadingMaterialControl() {
            SharedInitialization();
            InitializeComponent();
            SetUiForNewMaterial();

            Material = new ReadingMaterial() { Title = TitleHintText };
        }

        public ReadingMaterialControl(ReadingMaterial material) {
            InitializeComponent();
            SetUiForExistingMaterial();
            SharedInitialization(true);

            Material = material;
        }
        #endregion

        #region TitleHandlers
        private void txtTitle_GotFocus(object sender, RoutedEventArgs e) {
            if (Material.Title == TitleHintText) {
                Material.Title = "";
            }
        }

        private void txtTitle_LostFocus(object sender, RoutedEventArgs e) {
            if (string.IsNullOrEmpty(Material.Title)) {
                Material.Title = TitleHintText;
            }
        }

        private void txtTitle_TextChanged(object sender, TextChangedEventArgs e) {
            if (string.IsNullOrEmpty(txtTitle.Text) || txtTitle.Text.Equals(TitleHintText)) {
                OnTitleSet(false);
            } else {
                OnTitleSet(true);
            }
        }
        #endregion

        #region Buttons
        private void btnSave_Click(object sender, RoutedEventArgs e) {
            //MessageBox.Show("Укажите все необходимые данные для материала");

            if (string.IsNullOrEmpty(Material.Id)) {
                ContentStore.SelectedSegment?.ReadingMaterials.Add(Material);
            }

            ContentStore.UpdateItem<Segment>(ContentStore!.SelectedSegment!);
        }

        private void btnPreview_Click(object sender, RoutedEventArgs e) {
            var rtbPreviewWindow = new RtbPreviewWindow(Material);
            rtbPreviewWindow.ShowDialog();
        }

        private void btnUploadFromFile_Click(object sender, RoutedEventArgs e) {
            // Read the rtf file
            string filePath = FileService.SelectFilePath("RTF документы (.rtf)|*.rtf");
            if (string.IsNullOrEmpty(filePath)) return;

            // Read, load contents to the object and add to collection
            var contents = File.ReadAllText(filePath);
            Material.Content = contents;

            OnContentSet(true);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            ContentStore.SelectedSegment!.ReadingMaterials.Remove(Material);
            ContentStore.UpdateItem<Segment>(ContentStore.SelectedSegment);
            ContentStore.SelectedSegment = ContentStore.SelectedSegment;
        }
        #endregion

    }
}
