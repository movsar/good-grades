﻿using Content_Manager.Models;
using Content_Manager.Services;
using Content_Manager.Stores;
using Data.Models;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace Content_Manager.UserControls {
    /// <summary>
    /// Interaction logic for CwQmControl.xaml
    /// </summary>
    public partial class CwQmControl : UserControl {

        #region Fields
        private const string WordsCollectionHintText = "Введите описание материала";
        private FormCompletionInfo _formCompletionInfo;
        #endregion

        #region Properties
        ContentStore ContentStore => App.AppHost!.Services.GetRequiredService<ContentStore>();
        StylingService StylingService => App.AppHost!.Services.GetRequiredService<StylingService>();
        public string CwQmWordsCollection {
            get { return (string)GetValue(CwQmWordsCollectionProperty); }
            set { SetValue(CwQmWordsCollectionProperty, value); }
        }
        public static readonly DependencyProperty CwQmWordsCollectionProperty =
            DependencyProperty.Register("CwQmWordsCollection", typeof(string), typeof(CwQmControl), new PropertyMetadata(""));

        private string? QuizId => ContentStore.SelectedSegment?.CelebrityWodsQuiz.Id;
        public string CwQmId { get; }
        private byte[] CwQmImage { get; set; }

        #endregion

        #region Reactions
        private void OnFormStatusChanged(bool isReady) {
            if (isReady) {
                btnSave.Visibility = Visibility.Visible;
            } else {
                btnSave.Visibility = Visibility.Collapsed;
            }
        }
        private void OnWordsCollectionSet(bool isSet) {
            _formCompletionInfo.Update(nameof(CwQmWordsCollection), isSet);
        }
        private void OnImageSet(bool isSet = true) {
            BitmapImage logo = new BitmapImage();
            logo.BeginInit();
            logo.StreamSource = new MemoryStream(CwQmImage);
            logo.EndInit();

            var imgControl = new Image();
            imgControl.VerticalAlignment = VerticalAlignment.Stretch;
            imgControl.Source = logo;
            btnChooseImage.Content = imgControl;

            _formCompletionInfo.Update(nameof(CwQmImage), isSet);
        }

        #endregion

        #region Initialization
        private void SetUiForNewMaterial() {
            btnDelete.Visibility = Visibility.Collapsed;
            btnSave.Visibility = Visibility.Collapsed;
        }
        private void SetUiForExistingMaterial() {
            btnDelete.Visibility = Visibility.Visible;
        }
        private void SharedInitialization(bool isExistingMaterial = false) {
            InitializeComponent();
            DataContext = this;

            var propertiesToWatch = new string[] { nameof(CwQmImage), nameof(CwQmWordsCollection) };
            _formCompletionInfo = new FormCompletionInfo(propertiesToWatch, isExistingMaterial);
            _formCompletionInfo.StatusChanged += OnFormStatusChanged;
        }
        public CwQmControl() {
            SharedInitialization();
            SetUiForNewMaterial();

            CwQmWordsCollection = WordsCollectionHintText;
        }

        public CwQmControl(string optionId, byte[] image, string wordsCollection) {
            SharedInitialization(true);
            SetUiForExistingMaterial();

            CwQmId = optionId;
            CwQmImage = image;
            CwQmWordsCollection = wordsCollection;
        }
        #endregion

        #region WordsCollectionHandlers
        private void txtWordsCollection_GotFocus(object sender, RoutedEventArgs e) {
            if (CwQmWordsCollection == WordsCollectionHintText) {
                CwQmWordsCollection = "";
            }
        }

        private void txtWordsCollection_LostFocus(object sender, RoutedEventArgs e) {
            if (string.IsNullOrEmpty(CwQmWordsCollection)) {
                CwQmWordsCollection = WordsCollectionHintText;
            }
        }

        private void txtWordsCollection_TextChanged(object sender, TextChangedEventArgs e) {
            if (string.IsNullOrEmpty(txtWordsCollection.Text) || txtWordsCollection.Text.Equals(WordsCollectionHintText)) {
                OnWordsCollectionSet(false);
            } else {
                OnWordsCollectionSet(true);
            }
        }
        #endregion

        #region ButtonHandlers
        private void btnChooseImage_Click(object sender, RoutedEventArgs e) {
            string filePath = FileService.SelectFilePath("Файлы изображений (.png) | *.png; *.jpg; *.jpeg; *.tiff");
            if (string.IsNullOrEmpty(filePath)) return;

            // Read, load contents to the object and add to collection
            var content = File.ReadAllBytes(filePath);
            if (content.Length == 0) return;

            CwQmImage = content;

            OnImageSet(true);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {

        }
        private void btnSave_Click(object sender, RoutedEventArgs e) {
            if (string.IsNullOrEmpty(CwQmId)) {

                ContentStore.SelectedSegment?.CelebrityWodsQuiz.Data.Add()
                    .Add(new ReadingMaterial(RmTitle, RmText));
            } else {
                //var rm = ContentStore.GetReadingMaterialById(RmId);
                //rm.Title = RmTitle;
                //rm.Text = RmText;
            }

            ContentStore.UpdateItem<Segment>(ContentStore!.SelectedSegment!);
        }
        #endregion

    }
}
