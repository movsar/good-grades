﻿using Content_Manager.Interfaces;
using Content_Manager.Models;
using Content_Manager.Services;
using Content_Manager.Stores;
using Data.Entities.Materials;
using Data.Entities.Materials.TaskItems;
using Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace Content_Manager.UserControls
{
    public partial class TextAndImageItemControl : UserControl, IMaterialControl
    {
        public event Action<IEntityBase> Create;
        public event Action<IEntityBase> Update;
        public event Action<string> Delete;
        public event Action<string> SetAsCorrect;

        #region Fields
        private const string Hint = "Введите описание";
        private FormCompletionInfo _formCompletionInfo;
        #endregion

        #region Properties
        ContentStore ContentStore => App.AppHost!.Services.GetRequiredService<ContentStore>();
        StylingService StylingService => App.AppHost!.Services.GetRequiredService<StylingService>();
        public string ItemText
        {
            get { return (string)GetValue(ItemTextProperty); }
            set { SetValue(ItemTextProperty, value); }
        }
        public static readonly DependencyProperty ItemTextProperty =
            DependencyProperty.Register("ItemText", typeof(string), typeof(TextAndImageItemControl), new PropertyMetadata(""));

        public TextAndImageItemEntity Item { get; private set; }
        #endregion

        #region Reactions
        private void OnFormStatusChanged(bool isReady)
        {
            if (isReady)
            {
                btnAdd.Visibility = Visibility.Visible;
            }
            else
            {
                btnAdd.Visibility = Visibility.Collapsed;
            }
        }
        private void OnTextSet(bool isSet)
        {
            _formCompletionInfo.Update(nameof(ItemText), isSet);
        }
        private void OnImageSet(bool isSet = true)
        {
            BitmapImage logo = new BitmapImage();
            logo.BeginInit();
            logo.StreamSource = new MemoryStream(Item.Image);
            logo.EndInit();

            var imgControl = new Image();
            imgControl.VerticalAlignment = VerticalAlignment.Stretch;
            imgControl.Source = logo;
            btnChooseImage.Content = imgControl;

            _formCompletionInfo.Update(nameof(Item.Image), isSet);
        }
        #endregion

        #region Initialization
        private void SetUiForNewMaterial()
        {
            btnDelete.Visibility = Visibility.Hidden;
            btnAdd.Visibility = Visibility.Hidden;
        }
        private void SetUiForExistingMaterial()
        {
            btnDelete.Visibility = Visibility.Visible;
        }
        private void SharedInitialization(bool isExistingMaterial, bool isSelected)
        {
            InitializeComponent();
            DataContext = this;

            var propertiesToWatch = new List<string>();
            propertiesToWatch.Add(nameof(ItemText));

            // Decide what controls to make available
            btnChooseImage.Visibility = Visibility.Visible;

            propertiesToWatch.Add(nameof(Item.Image));


            _formCompletionInfo = new FormCompletionInfo(propertiesToWatch, isExistingMaterial);
            _formCompletionInfo.StatusChanged += OnFormStatusChanged;
        }

        public TextAndImageItemControl(TextAndImageItemEntity? item = null)
        {
            SharedInitialization(true, false);
            SetUiForExistingMaterial();

            Item = item ?? new TextAndImageItemEntity();

            if (Item.Image != null)
            {
                OnImageSet(true);
            }

            if (Item.Text != null)
            {
                OnTextSet(true);
            }
        }
        #endregion

        #region TextHandlers
        private void txtItemText_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ItemText == Hint)
            {
                ItemText = "";
            }
        }

        private void txtItemText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ItemText))
            {
                ItemText = Hint;
            }
        }

        private void txtItemText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtItemText.Text) || txtItemText.Text.Equals(Hint))
            {
                OnTextSet(false);
            }
            else
            {
                OnTextSet(true);
            }
        }
        #endregion

        #region ButtonHandlers
        private void btnChooseImage_Click(object sender, RoutedEventArgs e)
        {
            string filePath = FileService.SelectImageFilePath();
            if (string.IsNullOrEmpty(filePath)) return;

            // Read, load contents to the object and add to collection
            var content = File.ReadAllBytes(filePath);
            if (content.Length == 0) return;

            Item.Image = content;

            OnImageSet(true);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Delete?.Invoke(Item.Id);
        }
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ValidateInput();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            if (string.IsNullOrEmpty(Item.Id))
            {
                var item = new TextAndImageItemEntity()
                {
                    Text = ItemText,
                    Image = Item.Image
                };

                Create?.Invoke(item);
            }
            else
            {
                var item = ContentStore.Database.Find<TextAndImageItemEntity>(Item.Id);
                item.Image = Item.Image;
                item.Text = Item.Text;

                Update?.Invoke(item);
            }
        }

        private void ValidateInput()
        {
            //switch (_quizType)
            //{
            //    case QuizTypes.GapFiller:
            //        var gapOpeners = Regex.Matches(ItemText, @"\{");
            //        var gapClosers = Regex.Matches(ItemText, @"\}");
            //        var gappedWords = Regex.Matches(ItemText, @"\{\W*\w+.*?\}");

            //        if (gapOpeners.Count != gapClosers.Count || gapOpeners.Count != gappedWords.Count)
            //        {
            //            throw new Exception("Неправильное форматирование");
            //        }


            //        if (gappedWords.Count == 0)
            //        {
            //            throw new Exception("Необходимо указать хотя бы одно слово для пропуска");
            //        }

            //        break;
            //}
        }
        #endregion

        private void btnSetAsDefault_Click(object sender, RoutedEventArgs e)
        {
            if (Item.Id == null)
            {
                return;
            }

            SetAsCorrect?.Invoke(Item.Id);
        }
    }
}