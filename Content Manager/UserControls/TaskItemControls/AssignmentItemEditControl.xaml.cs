﻿using Content_Manager.Models;
using Content_Manager.Services;
using Content_Manager.Stores;
using Data;
using Data.Entities.TaskItems;
using Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;
using Shared.Translations;
using Shared.Services;

namespace Content_Manager.UserControls
{
    public partial class AssignmentItemEditControl : UserControl
    {
        public event Action<IEntityBase> Create;
        public event Action<IEntityBase> Update;
        public event Action<string> Delete;
        public event Action<string> SetAsCorrect;

        #region Fields
        private string Hint = Ru.SetDescription;
        private FormCompletionInfo _formCompletionInfo;
        private TaskType _taskType;
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
            DependencyProperty.Register("ItemText", typeof(string), typeof(AssignmentItemEditControl), new PropertyMetadata(""));
        public string ItemId { get; set; }
        public byte[] ItemImage { get; private set; }
        #endregion

        #region Reactions
        private void OnFormStatusChanged(bool isReady)
        {
            if (isReady)
            {
                btnSave.Visibility = Visibility.Visible;
            }
            else
            {
                btnSave.Visibility = Visibility.Collapsed;
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
            logo.StreamSource = new MemoryStream(ItemImage);
            logo.EndInit();

            var imgControl = new Image();
            imgControl.VerticalAlignment = VerticalAlignment.Stretch;
            imgControl.Source = logo;
            btnChooseImage.Content = imgControl;

            _formCompletionInfo.Update(nameof(ItemImage), isSet);
        }
        #endregion

        #region Initialization
        private void SetUiForNewMaterial()
        {
            btnDelete.Visibility = Visibility.Hidden;
            btnSave.Visibility = Visibility.Hidden;
        }
        private void SetUiForExistingMaterial()
        {
            btnDelete.Visibility = Visibility.Visible;
        }
        private void SharedUiInitialization(TaskType taskType, bool isExistingMaterial, bool isSelected)
        {
            InitializeComponent();
            DataContext = this;

            _taskType = taskType;

            var propertiesToWatch = new List<string>
            {
                nameof(ItemText)
            };

            // Decide what controls to make available
            switch (_taskType)
            {
                case TaskType.Filling:
                    Hint = Ru.FillingQuestion;
                    break;
                case TaskType.Matching:
                    Hint = Ru.SetImageDescription;

                    btnChooseImage.Visibility = Visibility.Visible;

                    propertiesToWatch.Add(nameof(ItemImage));

                    break;
                case TaskType.Test:
                case TaskType.Selecting:
                    btnSetAsCorrect.Visibility = Visibility.Visible;

                    if (isSelected)
                    {
                        btnSetAsCorrect.Content = "\uE73A";
                        btnSetAsCorrect.Foreground = Brushes.DarkGreen;
                    }
                    else
                    {
                        btnSetAsCorrect.Content = "\uE739";
                    }

                    if (!isExistingMaterial)
                    {
                        btnSetAsCorrect.IsEnabled = false;
                    }

                    break;
                default:
                    break;
            }

            ItemText = Hint;

            _formCompletionInfo = new FormCompletionInfo(propertiesToWatch, isExistingMaterial);
            _formCompletionInfo.StatusChanged += OnFormStatusChanged;
        }
        public AssignmentItemEditControl(TaskType taskType)
        {
            SharedUiInitialization(taskType, false, false);
            SetUiForNewMaterial();
        }
        public AssignmentItemEditControl(TaskType taskType, AssignmentItem item, bool isSelected = false)
        {
            SharedUiInitialization(taskType, true, isSelected);
            SetUiForExistingMaterial();

            ItemId = item.Id;
            ItemImage = item.Image;
            ItemText = item.Text;

            OnTextSet(true);

            if (item.Image != null)
            {
                OnImageSet(true);
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

            ItemImage = content;

            OnImageSet(true);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Delete?.Invoke(ItemId);
        }
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ValidateInput();
            }
            catch (Exception ex)
            {
                ExceptionService.HandleError(ex, ex.Message);
                return;
            }

            if (string.IsNullOrEmpty(ItemId))
            {
                var item = new AssignmentItem()
                {
                    Text = ItemText,
                    Image = ItemImage
                };

                Create?.Invoke(item);
            }
            else
            {
                var item = ContentStore.DbContext.Find<AssignmentItem>(ItemId);
                item.Image = ItemImage;
                item.Text = ItemText;

                ContentStore.DbContext.SaveChanges();

                Update?.Invoke(item);
            }
        }
        private void ValidateInput()
        {
            switch (_taskType)
            {
                case TaskType.Filling:
                    var gapOpeners = Regex.Matches(ItemText, @"\{");
                    var gapClosers = Regex.Matches(ItemText, @"\}");
                    var gappedWords = Regex.Matches(ItemText, @"\{\W*\w+.*?\}");

                    if (gapOpeners.Count != gapClosers.Count || gapOpeners.Count != gappedWords.Count)
                    {
                        throw new Exception(Ru.ExceptionUncorrectFormate);
                    }

                    if (gappedWords.Count == 0)
                    {
                        throw new Exception(Ru.ExceptionMinWords);
                    }

                    break;
            }
        }
        #endregion

        private void btnSetAsDefault_Click(object sender, RoutedEventArgs e)
        {
            if (ItemId == null)
            {
                return;
            }

            SetAsCorrect?.Invoke(ItemId);
        }
    }
}
