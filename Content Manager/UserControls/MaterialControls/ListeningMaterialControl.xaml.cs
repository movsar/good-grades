﻿using Content_Manager.Interfaces;
using Content_Manager.Models;
using Content_Manager.Services;
using Content_Manager.Stores;
using Data.Entities;
using Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Shared.Viewers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Content_Manager.UserControls
{
    public partial class ListeningMaterialControl : UserControl, IMaterialControl
    {
        public event Action<IEntityBase> Create;
        public event Action<string?, IEntityBase> Update;
        public event Action<string> Delete;

        #region Fields
        private FormCompletionInfo _formCompletionInfo;
        private const string TitleHintText = "Введите название материала";
        #endregion

        #region Properties
        ContentStore ContentStore => App.AppHost!.Services.GetRequiredService<ContentStore>();
        StylingService StylingService => App.AppHost!.Services.GetRequiredService<StylingService>();

        public string LmTitle
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("LmTitle", typeof(string), typeof(ListeningMaterialControl), new PropertyMetadata(""));

        public string LmText { get; set; }
        public byte[] LmAudio { get; set; }
        public byte[] LmImage { get; set; }
        private string LmId { get; }
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
            _formCompletionInfo.Update(nameof(LmTitle), isSet);
        }
        private void OnTextSet(bool isSet = true)
        {
            btnChooseText.Background = StylingService.StagedBrush;

            _formCompletionInfo.Update(nameof(LmText), isSet);
        }
        private void OnImageSet(bool isSet = true)
        {
            btnChooseImage.Background = StylingService.StagedBrush;

            _formCompletionInfo.Update(nameof(LmImage), isSet);
        }
        private void OnAudioSet(bool isSet = true)
        {
            btnChooseAudio.Background = StylingService.StagedBrush;

            _formCompletionInfo.Update(nameof(LmAudio), isSet);
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
            btnDelete.Visibility = Visibility.Visible;
            btnChooseText.Background = StylingService.ReadyBrush;
            btnChooseAudio.Background = StylingService.ReadyBrush;
            btnChooseImage.Background = StylingService.ReadyBrush;
        }
        private void SharedInitialization(bool isExistingMaterial = false)
        {
            InitializeComponent();
            DataContext = this;

            var propertiesToWatch = new List<string>();
            propertiesToWatch.Add(nameof(LmTitle));
            propertiesToWatch.Add(nameof(LmText));
            propertiesToWatch.Add(nameof(LmAudio));

            _formCompletionInfo = new FormCompletionInfo(propertiesToWatch, isExistingMaterial);
            _formCompletionInfo.StatusChanged += OnFormStatusChanged;
        }
        public ListeningMaterialControl()
        {
            SharedInitialization();
            SetUiForNewMaterial();

            LmTitle = TitleHintText;
        }

        public ListeningMaterialControl(ListeningMaterialEntity material)
        {
            SharedInitialization(true);
            SetUiForExistingMaterial();

            LmId = material.Id;
            LmTitle = material.Title;
            LmText = material.Text;
            LmAudio = material.Audio;
            LmImage = material.Image;
        }
        #endregion

        #region TitleHandlers
        private void txtTitle_GotFocus(object sender, RoutedEventArgs e)
        {
            if (LmTitle == TitleHintText)
            {
                LmTitle = "";
            }
        }

        private void txtTitle_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(LmTitle))
            {
                LmTitle = TitleHintText;
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
        private void btnChooseText_Click(object sender, RoutedEventArgs e)
        {
            string filePath = FileService.SelectTextFilePath();
            if (string.IsNullOrEmpty(filePath)) return;

            // Read, load contents to the object and add to collection
            var content = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
            if (string.IsNullOrEmpty(filePath)) return;

            LmText = content;
            OnTextSet(true);
        }

        private void btnChooseImage_Click(object sender, RoutedEventArgs e)
        {
            string filePath = FileService.SelectImageFilePath();
            if (string.IsNullOrEmpty(filePath)) return;

            // Read, load contents to the object and add to collection
            var content = File.ReadAllBytes(filePath);
            if (content.Length == 0) return;

            LmImage = content;
            OnImageSet(true);
        }

        private void btnChooseAudio_Click(object sender, RoutedEventArgs e)
        {
            // Read the rtf file
            string filePath = FileService.SelectAudioFilePath();
            if (string.IsNullOrEmpty(filePath)) return;

            // Read, load contents to the object and add to collection
            var content = File.ReadAllBytes(filePath);
            if (content.Length == 0) return;

            LmAudio = content;
            OnAudioSet(true);
        }

        private void btnPreview_Click(object sender, RoutedEventArgs e)
        {
            var listeningPreviewWindow = new MaterialPresenter(LmTitle, LmText, LmImage, LmAudio);
            listeningPreviewWindow.ShowDialog();
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Укажите все необходимые данные для материала");

            if (string.IsNullOrEmpty(LmId))
            {
                var lm = new Data.Entities.ListeningMaterialEntity()
                {
                    Title = LmTitle,
                    Text = LmText,
                    Audio = LmAudio,
                    Image = LmImage
                };

                ContentStore.SelectedSegment?.ListeningMaterials.Add(lm);

                //Create?.Invoke(lm);
            }
            else
            {
                var lm = ContentStore.GetListeningMaterialById(LmId);
                lm.Title = LmTitle;
                lm.Text = LmText;
                lm.Image = LmImage;
                lm.Audio = LmAudio;

                Update?.Invoke(LmId, lm);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Delete?.Invoke(LmId);
        }
        #endregion

    }
}
