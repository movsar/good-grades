using GGManager.Models;
using GGManager.Services;
using Data;
using Data.Entities.TaskItems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Shared;
using Image = System.Windows.Controls.Image;
using System.Text.RegularExpressions;
using Shared.Services;
using System.Windows.Media;

namespace GGManager.UserControls
{
    public partial class AssignmentItemEditControl : UserControl
    {
        #region Fields
        private string Hint = Translations.GetValue("SetDescription");
        private FormCompletionInfo _formCompletionInfo;
        private AssignmentType _assignmentType;
        #endregion

        #region Properties and Events
        public AssignmentItem Item { get; }
        public bool IsValid { get; private set; } = true;

        public event Action<AssignmentItem> Discarded;
        public event Action<AssignmentItem> Committed;
        internal string InitialText { get; private set; }
        #endregion

        #region Reactions
        private void OnTextSet(bool isSet)
        {
            _formCompletionInfo.Update(nameof(Item.Text), isSet);
        }
        private void OnImageSet(bool isSet = true)
        {
            var data = Storage.ReadDbAsset(Item.ImagePath);

            BitmapImage logo = new BitmapImage();
            logo.BeginInit();
            logo.StreamSource = new MemoryStream(data!);
            logo.EndInit();

            var imgControl = new Image();
            imgControl.VerticalAlignment = VerticalAlignment.Stretch;
            imgControl.Source = logo;
            btnChooseImage.Content = imgControl;

            _formCompletionInfo.Update(nameof(Item.ImagePath), isSet);
        }
        #endregion

        #region Initialization

        private void SharedUiInitialization(AssignmentType assignmentType, bool isExistingItem)
        {
            InitializeComponent();
            DataContext = this;

            _assignmentType = assignmentType;

            var propertiesToWatch = new List<string>
            {
                nameof(Item.Text)
            };

            // Decide what controls to make available
            switch (_assignmentType)
            {
                case AssignmentType.Filling:
                    Hint = Translations.GetValue("FillingQuestion");
                    break;
                case AssignmentType.Matching:
                    Hint = Translations.GetValue("SetImageDescription");

                    btnChooseImage.Visibility = Visibility.Visible;

                    propertiesToWatch.Add(nameof(Item.ImagePath));

                    break;
                case AssignmentType.Test:
                case AssignmentType.Selecting:
                    chkIsChecked.Visibility = Visibility.Visible;
                    chkIsChecked.IsChecked = Item.IsChecked;
                    break;
                default:
                    break;
            }

            txtItemText.Text = Hint;

            _formCompletionInfo = new FormCompletionInfo(propertiesToWatch, isExistingItem);
        }
        public AssignmentItemEditControl(AssignmentType taskType)
        {
            Item = new AssignmentItem();

            InitialText = Item.Text;
            // Prepare UI for a new Assignment Item
            SharedUiInitialization(taskType, false);
            btnCommit.Visibility = Visibility.Visible;
            btnDiscard.Visibility = Visibility.Collapsed;
        }
        public AssignmentItemEditControl(AssignmentType taskType, AssignmentItem item)
        {
            Item = item;
            InitialText = item.Text;
            // Prepare UI for an existing Assignment Item
            SharedUiInitialization(taskType, true);
            btnDiscard.Visibility = Visibility.Visible;
            btnCommit.Visibility = Visibility.Collapsed;

            txtItemText.Text = Item.Text;
            OnTextSet(true);

            if (Item.ImagePath != null)
            {
                OnImageSet(true);
            }
        }
        #endregion

        //Валидация заданий на сопоставление и заполнение
        public void Validate()
        {
            try
            {
                //проверка на наличие текста и отсутствие пробелов
                if (string.IsNullOrWhiteSpace(Item.Text))
                {
                    throw new Exception("Введите текст");
                }

                switch (_assignmentType)
                {
                    case AssignmentType.Matching:
                        //проверка на заданность изображений в задании сопоставления
                        if (Item.ImagePath == null)
                        {
                            throw new Exception(Translations.GetValue("SetMatchingImage"));
                        }

                        break;
                    case AssignmentType.Filling:
                        //проверка для заданий заполнения на формат текста
                        var gapOpeners = Regex.Matches(Item.Text, @"\{");
                        var gapClosers = Regex.Matches(Item.Text, @"\}");
                        var gappedWords = Regex.Matches(Item.Text, @"\{\W*\w+.*?\}");

                        if (gapOpeners.Count != gapClosers.Count || gapOpeners.Count != gappedWords.Count)
                        {
                            throw new Exception(Translations.GetValue("ExceptionUncorrectFormate"));
                        }
                        //проверка на наличие минимум одного заполненного слова
                        if (gappedWords.Count == 0)
                        {
                            throw new Exception(Translations.GetValue("ExceptionMinWords"));
                        }

                        break;
                }

                IsValid = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                IsValid = false;
            }
        }

        #region Event Handlers
        private void txtItemText_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtItemText.Text == Hint)
            {
                txtItemText.Text = "";
            }
        }

        private void txtItemText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtItemText.Text))
            {
                txtItemText.Text = Hint;
            }
        }
        private void txtItemText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_formCompletionInfo == null)
            {
                return;
            }

            if (sender is TextBox textBox)
            {
                var scrollViewer = GetDescendantByType<ScrollViewer>(textBox);

                if (scrollViewer != null)
                {
                    // Adjust the height of the TextBox dynamically
                    textBox.Height = Math.Max(textBox.MinHeight, scrollViewer.ExtentHeight + textBox.Padding.Top + textBox.Padding.Bottom * 2);
                }
            }

            // Если текст пустой или равен подсказке, вызывается OnTextSet с параметром false
            if (string.IsNullOrEmpty(txtItemText.Text) || txtItemText.Text.Equals(Hint))
            {
                OnTextSet(false);
            }
            else
            {
                Item.Text = TextService.GetChechenString(txtItemText.Text);
                OnTextSet(true);
            }
        }// Helper method to find a descendant of a specific type
        private T GetDescendantByType<T>(DependencyObject element) where T : DependencyObject
        {
            if (element == null)
                return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);

                if (child is T typedChild)
                    return typedChild;

                var descendant = GetDescendantByType<T>(child);
                if (descendant != null)
                    return descendant;
            }

            return null;
        }

        private void btnChooseImage_Click(object sender, RoutedEventArgs e)
        {
            // Открытие диалогового окна для выбора пути к файлу изображения
            string filePath = FileService.SelectImageFilePath();
            if (string.IsNullOrEmpty(filePath)) return;

            var sizeInKilobytes = (new FileInfo(filePath)).Length / 1024;
            if (sizeInKilobytes > 1000)
            {
                MessageBox.Show("Размер изображения не должен превышать 1Mb");
                return;
            }

            Item.ImagePath = filePath;

            OnImageSet(true);
        }

        private void btnDiscard_Click(object sender, RoutedEventArgs e)
        {
            Discarded?.Invoke(Item);
        }

        private void chkIsChecked_Checked(object sender, RoutedEventArgs e)
        {
            Item.IsChecked = true;
        }

        private void chkIsChecked_Unchecked(object sender, RoutedEventArgs e)
        {
            Item.IsChecked = false;
        }

        private void btnCommit_Click(object sender, RoutedEventArgs e)
        {
            Commit();
        }

        private void Commit()
        {
            Item.Text = TextService.GetChechenString(Item.Text);

            Validate();

            if (!IsValid)
            {
                return;
            }

            // Скрываем кнопку Commit и показываем кнопку Discard
            btnCommit.Visibility = Visibility.Collapsed;
            btnDiscard.Visibility = Visibility.Visible;

            Committed?.Invoke(Item);
        }

        private void txtItemText_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //if (e.Key == System.Windows.Input.Key.Enter)
            //{
            //    Commit();
            //}
        }
        #endregion
    }
}
