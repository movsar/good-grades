﻿using Content_Manager.Models;
using Content_Manager.Services;
using Content_Manager.Stores;
using Data.Entities;
using Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Shared.Viewers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Content_Manager.UserControls
{
    public partial class ReadingAssignmentControl : UserControl
    {
        #region Properties and Fields
        private FormCompletionInfo _formCompletionInfo;
        private const string TitleHintText = "Введите название материала";

        ContentStore _contentStore = App.AppHost!.Services.GetRequiredService<ContentStore>();
        StylingService _stylingService = App.AppHost!.Services.GetRequiredService<StylingService>();

        public string RmTitle
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("RmTitle", typeof(string), typeof(ReadingAssignmentControl), new PropertyMetadata(""));

        public string RmText { get; set; }
        public byte[] RmImage { get; set; }
        private string RmId { get; }

        #endregion

        #region Reactions
        private void OnFormStatusChanged(bool isReady)
        {
            if (isReady)
            {
                btnSave.Visibility = Visibility.Visible;
                btnPreview.Visibility = Visibility.Visible;
            }
            else
            {
                btnSave.Visibility = Visibility.Collapsed;
                btnPreview.Visibility = Visibility.Collapsed;
            }
        }
        private void OnTitleSet(bool isSet)
        {
            _formCompletionInfo.Update(nameof(RmTitle), isSet);
        }
        private void OnContentSet(bool isSet = true)
        {
            btnUploadFromFile.Background = _stylingService.StagedBrush;

            _formCompletionInfo.Update(nameof(RmText), isSet);
        }
        private void OnImageSet(bool isSet = true)
        {
            btnChooseImage.Background = _stylingService.StagedBrush;

            _formCompletionInfo.Update(nameof(RmImage), isSet);
        }
        #endregion

        #region Initialization
        private void SetUiForNewMaterial()
        {
            btnPreview.Visibility = Visibility.Collapsed;
            btnDelete.Visibility = Visibility.Collapsed;
            btnSave.Visibility = Visibility.Collapsed;
        }
        private void SetUiForExistingMaterial()
        {
            btnUploadFromFile.Background = _stylingService.ReadyBrush;

            if (RmImage != null)
            {
                btnChooseImage.Background = _stylingService.ReadyBrush;
            }

            btnPreview.Background = _stylingService.ReadyBrush;
            btnDelete.Visibility = Visibility.Visible;
        }

        private void SharedInitialization(bool isExistingMaterial = false)
        {
            InitializeComponent();
            DataContext = this;

            var propertiesToWatch = new List<string>();
            propertiesToWatch.Add(nameof(RmTitle));
            propertiesToWatch.Add(nameof(RmText));

            _formCompletionInfo = new FormCompletionInfo(propertiesToWatch, isExistingMaterial);
            _formCompletionInfo.StatusChanged += OnFormStatusChanged;
        }

        public ReadingAssignmentControl()
        {
            SharedInitialization();
            SetUiForNewMaterial();

            RmTitle = TitleHintText;
        }

        public ReadingAssignmentControl(ReadingAssignmnet material)
        {
            SharedInitialization(true);

            RmId = material.Id!;
            RmTitle = material.Title;
            RmText = material.Text;
            RmImage = material.Image;

            SetUiForExistingMaterial();
        }
        #endregion

        #region TitleHandlers
        private void txtTitle_GotFocus(object sender, RoutedEventArgs e)
        {
            if (RmTitle == TitleHintText)
            {
                RmTitle = "";
            }
        }

        private void txtTitle_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(RmTitle))
            {
                RmTitle = TitleHintText;
            }
        }

        private void txtTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtTitle.Text) || txtTitle.Text.Equals(TitleHintText))
            {
                OnTitleSet(false);
            }
            else
            {
                OnTitleSet(true);
            }
        }
        #endregion

        #region ButtonHandlers
        private void btnUploadFromFile_Click(object sender, RoutedEventArgs e)
        {
            // Read the rtf file
            string filePath = FileService.SelectTextFilePath();
            if (string.IsNullOrEmpty(filePath)) return;

            // Read, load contents to the object and add to collection
            var contents = File.ReadAllText(filePath);
            RmText = contents;

            OnContentSet(true);
        }
        private void btnPreview_Click(object sender, RoutedEventArgs e)
        {
            var rtbPreviewWindow = new MaterialPresenter(RmTitle, RmText, RmImage);
            rtbPreviewWindow.ShowDialog();
        }

        private void btnChooseImage_Click(object sender, RoutedEventArgs e)
        {
            string filePath = FileService.SelectImageFilePath();
            if (string.IsNullOrEmpty(filePath)) return;

            // Read, load contents to the object and add to collection
            var content = File.ReadAllBytes(filePath);
            if (content.Length == 0) return;

            RmImage = content;
            OnImageSet(true);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Укажите все необходимые данные для материала");

            if (string.IsNullOrEmpty(RmId))
            {
                var rm = new ReadingAssignmnet()
                {
                    Title = RmTitle,
                    Text = RmText,
                    Image = RmImage
                };
                _contentStore.Database.Write(() => _contentStore.SelectedSegment?.ReadingMaterials.Add(rm));

                _contentStore.RaiseItemAddedEvent(rm);
            }
            else
            {
                var rm = _contentStore.Database.All<ReadingAssignmnet>().First(rm => rm.Id == RmId);
                _contentStore.Database.Write(() =>
                {
                    rm.Title = RmTitle;
                    rm.Text = RmText;
                    rm.Image = RmImage;
                });

                _contentStore.RaiseItemUpdatedEvent(rm);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var rm = _contentStore.Database.Find<ReadingAssignmnet>(RmId);
            _contentStore.Database.Write(() => _contentStore.Database.Remove(rm));
            _contentStore.RaiseItemDeletedEvent(rm);
        }
        #endregion

    }
}