using GGManager.Models;
using GGManager.Services;
using GGManager.Stores;
using Data.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Shared;
using Shared.Services;
using System.Windows.Input;
using Shared.Controls;
using Plugin.SimpleAudioPlayer;
using System.Windows.Media;

namespace GGManager.UserControls
{
    public partial class MaterialControl : UserControl
    {

        #region Properties
        private FormCompletionInfo _formCompletionInfo;
        private static MaterialControl? _draggedMaterial;
        static string TitleHintText { get; } = Translations.GetValue("SetMaterialTitle");
        ContentStore ContentStore => App.AppHost!.Services.GetRequiredService<ContentStore>();
        StylingService StylingService => App.AppHost!.Services.GetRequiredService<StylingService>();

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("LmTitle", typeof(string), typeof(MaterialControl), new PropertyMetadata(""));

        public byte[]? PdfData { get; set; }
        public byte[]? Audio { get; set; }
        private string Id { get; }
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
            _formCompletionInfo.Update(nameof(Title), isSet);
        }
        private void OnTextSet(bool isSet = true)
        {
            btnChooseText.Background = StylingService.StagedBrush;

            _formCompletionInfo.Update(nameof(PdfData), isSet);
        }
        private void OnAudioSet(bool isSet = true)
        {
            btnChooseAudio.Background = StylingService.StagedBrush;

            _formCompletionInfo.Update(nameof(Audio), isSet);
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
            if (Audio != null)
            {
                btnChooseAudio.Background = StylingService.ReadyBrush;
            }
        }
        private void SharedInitialization(bool isExistingMaterial = false)
        {
            InitializeComponent();
            DataContext = this;

            var propertiesToWatch = new List<string>
            {
                nameof(Title),
                nameof(PdfData),
            };

            _formCompletionInfo = new FormCompletionInfo(propertiesToWatch, isExistingMaterial);
            _formCompletionInfo.StatusChanged += OnFormStatusChanged;
        }
        public MaterialControl()
        {
            SharedInitialization();
            SetUiForNewMaterial();

            Title = TitleHintText;
        }

        public MaterialControl(Material material)
        {
            SharedInitialization(true);

            Id = material.Id;
            Title = material.Title;
            PdfData = material.PdfData;
            Audio = material.Audio;
            SetUiForExistingMaterial();
        }
        #endregion

        #region TitleHandlers
        private void txtTitle_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Title == TitleHintText)
            {
                Title = "";
            }
        }

        private void txtTitle_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Title))
            {
                Title = TitleHintText;
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
        private void txtTitle_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Save();
            }
        }
        #endregion

        #region ButtonHandlers

        private void btnChooseText_Click(object sender, RoutedEventArgs e)
        {
            string filePath = FileService.SelectPdfFilePath();
            if (string.IsNullOrEmpty(filePath)) return;

            var sizeInKilobytes = (new FileInfo(filePath)).Length / 1024;
            if (sizeInKilobytes > 10_000)
            {
                MessageBox.Show("Размер файла не должен превышать 10Mb");
                return;
            }

            // Read, load contents to the object and add to collection            
            var content = File.ReadAllBytes(filePath);
            if (content.Length == 0)
            {
                return;
            }

            PdfData = content;
            OnTextSet(true);
        }

        private void btnChooseAudio_Click(object sender, RoutedEventArgs e)
        {
            // Read the rtf file
            string filePath = FileService.SelectAudioFilePath();
            if (string.IsNullOrEmpty(filePath)) return;

            // Read, load contents to the object and add to collection
            var content = File.ReadAllBytes(filePath);
            if (content.Length == 0) return;

            if (content.Length > 6_000_000)
            {
                MessageBox.Show("Слишком большой файл, должен быть до 5Мб");
                return;
            }

            Audio = content;
            OnAudioSet(true);
        }

        private void btnPreview_Click(object sender, RoutedEventArgs e)
        {
            var window = new Window()
            {
                Title = Title
            };

            var materialControl = new MaterialViewerControl();
            materialControl.Initialize(Title, PdfData, Audio);

            window.Content = materialControl;
            window.Closing += Window_Closing;
            window.ShowDialog();
        }

        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            CrossSimpleAudioPlayer.Current.Stop();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void Save()
        {
            //MessageBox.Show(Ru.NecessaryInfoForContent);

            if (string.IsNullOrEmpty(Id))
            {
                var lm = new Material
                {
                    Title = Title,
                    PdfData = PdfData,
                    Audio = Audio,
                };

                ContentStore.SelectedSegment?.Materials.Add(lm);
                ContentStore.DbContext.SaveChanges();

                ContentStore.RaiseItemAddedEvent(lm);
            }
            else
            {
                var lm = ContentStore.DbContext.Materials.First(lm => lm.Id == Id);
                lm.Title = Title;
                lm.PdfData = PdfData;
                lm.Audio = Audio;

                ContentStore.DbContext.SaveChanges();

                ContentStore.RaiseItemUpdatedEvent(lm);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var lm = ContentStore.DbContext.Find<Material>(Id);
            ContentStore.DbContext.Materials.Remove(lm);
            ContentStore.DbContext.SaveChanges();

            ContentStore.RaiseItemDeletedEvent(lm);
        }
        #endregion

        private void btnDrag_Drop(object sender, DragEventArgs e)
        {
            if (_draggedMaterial == null || this == _draggedMaterial) return;

            var parentPanel = FindParent<StackPanel>();
            if (parentPanel == null) return;

            int draggedIndex = parentPanel.Children.IndexOf(_draggedMaterial);
            int targetIndex = parentPanel.Children.IndexOf(this);

            if (draggedIndex != targetIndex && draggedIndex >= 0 && targetIndex >= 0)
            {
                SwapMaterials(draggedIndex, targetIndex, parentPanel);
                UpdateOrderIndexes(parentPanel);
            }

            ContentStore.DbContext.SaveChanges();
        }

        private void btnDrag_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }

        private void btnDrag_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _draggedMaterial = this;
            DragDrop.DoDragDrop(btnDrag, this, DragDropEffects.Move);
        }

        private void SwapMaterials(int draggedIndex, int targetIndex, StackPanel panel)
        {
            var draggedElement = panel.Children[draggedIndex];
            panel.Children.RemoveAt(draggedIndex);
            panel.Children.Insert(targetIndex, draggedElement);
        }

        private void UpdateOrderIndexes(StackPanel panel)
        {
            var materials = ContentStore.SelectedSegment!.Materials.ToList();

            for (int i = 0; i < panel.Children.Count; i++)
            {
                if (panel.Children[i] is MaterialControl materialControl)
                {
                    var material = materials.FirstOrDefault(m => m.Id == materialControl.Id);
                    if (material != null)
                    {
                        material.Order = i;
                        ContentStore.DbContext.Update(material);
                    }
                }
            }

            ContentStore.DbContext.SaveChanges();
        }

        private T? FindParent<T>() where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(this);
            while (parent != null && parent is not T)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }
    }
}
