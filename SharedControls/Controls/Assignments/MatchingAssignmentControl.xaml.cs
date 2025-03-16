using Data;
using Data.Entities;
using Data.Interfaces;
using Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Shared.Controls.Assignments
{
    public partial class MatchingAssignmentControl : UserControl, IAssignmentControl
    {
        public int StepsCount { get; } = 1;

        // A dictionary to hold matching pairs with string identifiers and corresponding images.
        private readonly Dictionary<string, BitmapImage> _matchingPairs = new Dictionary<string, BitmapImage>();
        // The matching task assignment to be completed in this viewer.
        private MatchingAssignment _assignment;

        // An event that signals when the completion state of the assignment changes.
        public event Action<IAssignment, bool> AssignmentCompleted;
        public event Action<IAssignment, string, bool> AssignmentItemSubmitted;

        #region Properties, Fields and Constructors
        // Constructor initializes the MatchingViewer with a specific assignment.
        public MatchingAssignmentControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void Initialize(IAssignment assignment)
        {
            IsEnabled = true;
            if (assignment == _assignment)
            {
                return;
            }

            _assignment = (MatchingAssignment)assignment;

            _matchingPairs.Clear();

            int numberOfPairs = _assignment.Items.Count;

            // Load matching pairs from the assignment into the dictionary.
            foreach (var item in _assignment.Items)
            {
                var data = Storage.ReadDbAsset(item.ImagePath);

                _matchingPairs.Add(item.Text, ConvertByteArrayToBitmapImage(data));
            }

            var imageBorders = new List<Border>();
            var textBorders = new List<Border>();

            for (int pairIndex = 0; pairIndex < numberOfPairs; pairIndex++)
            {
                var text = _matchingPairs.Keys.ToList()[pairIndex];
                var image = _matchingPairs.Values.ToList()[pairIndex];

                var imageBorder = CreateBorderWithChild(new Image
                {
                    Source = image,
                    Name = $"Pair_{pairIndex}",
                    Style = (Style)FindResource("MatchingImageStyle")
                });
                imageBorders.Add(imageBorder);

                var textBorder = CreateBorderWithChild(new TextBlock
                {
                    Text = text,
                    Name = $"Pair_{pairIndex}",
                });
                textBorders.Add(textBorder);
            }

            AddToStackPanel(imageBorders, textBorders);
        }

        private void AddToStackPanel(List<Border> imageBorders, List<Border> textBorders)
        {
            // Shuffle the items to randomize their positions
            var randomIndexesForTexts = Enumerable.Range(0, _matchingPairs.Count).OrderBy(i => Guid.NewGuid()).ToArray();
            var randomIndexesForImages = Enumerable.Range(0, _matchingPairs.Count).OrderBy(i => Guid.NewGuid()).ToArray();

            // Create UI elements for each pair and assign them to random columns.
            spMatchOptions.Children.Clear();

            var randomizer = new Random(DateTime.Now.Millisecond);

            for (var i = 0; i < _matchingPairs.Count; i++)
            {
                var image = imageBorders[randomIndexesForImages[i]];
                image.Margin = new Thickness(0, randomizer.Next(20), 5, randomizer.Next(20));

                var text = textBorders[randomIndexesForTexts[i]];
                text.Margin = new Thickness(0, randomizer.Next(20), 5, 0);

                var sp = new StackPanel();
                sp.Children.Add(image);
                sp.Children.Add(text);

                spMatchOptions.Children.Add(sp);
            }
        }

        #endregion

        //Generic method to find a child element of a specific type in a grid cell.
        private T? FindChildInGrid<T>(Grid grid, int row, int column) where T : FrameworkElement
        {
            foreach (FrameworkElement child in grid.Children)
            {
                if (Grid.GetRow(child) == row && Grid.GetColumn(child) == column && child is T)
                {
                    return (T)child;
                }
            }
            return null;
        }

        #region Drag & Drop

        // Handles the drop event during a drag-and-drop operation to swap elements.
        private void Element_Drop(object sender, DragEventArgs e)
        {
            var targetBorder = sender as Border;
            var sourceBorder = e.Data.GetData(typeof(Border)) as Border;

            // Do nothing if the source or target elements are invalid.
            if (sourceBorder == null || targetBorder == null)
            {
                return;
            }

            // Reset styles for both the source and target borders.
            ResetBorderStyle(targetBorder);
            ResetBorderStyle(sourceBorder);

            // Do nothing if attempting to drop an element onto itself.
            if (sourceBorder == targetBorder)
            {
                return;
            }

            // Swap contents only if both elements are of the same type.
            UIElement sourceContent = sourceBorder.Child;
            UIElement targetContent = targetBorder.Child;
            if (sourceContent.GetType() != targetContent.GetType())
            {
                return;
            }

            // Perform the swap of the children between the source and target borders.
            sourceBorder.Child = null;
            targetBorder.Child = null;
            sourceBorder.Child = targetContent;
            targetBorder.Child = sourceContent;
        }

        // Initiates a drag operation when a mouse button is pressed down on an element.
        private void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border != null)
            {
                DragDrop.DoDragDrop(border, new DataObject(typeof(Border), border), DragDropEffects.Move);
            }
        }

        // Handles the drag enter event to provide visual feedback for a potential drop.
        private void Border_DragEnter(object sender, DragEventArgs e)
        {
            var border = sender as Border;
            var draggedBorder = e.Data.GetData(typeof(Border)) as Border;

            // Change border style based on whether the dragged element can be dropped here.
            if (border != null && draggedBorder != null)
            {
                bool isSameType = (draggedBorder.Child.GetType() == border.Child.GetType());
                border.BorderBrush = isSameType ? Brushes.Green : Brushes.Red;
                border.BorderThickness = new Thickness(4);
            }
        }

        // Resets the border style when the dragged element leaves the area.
        private void Border_DragLeave(object sender, DragEventArgs e)
        {
            var border = sender as Border;
            if (border != null)
            {
                ResetBorderStyle(border);
            }
        }

        #endregion

        // Resets the border style to default settings.
        private void ResetBorderStyle(Border border)
        {
            // Set to a default or neutral color.
            border.BorderBrush = Brushes.LightGray;
            border.BorderThickness = new Thickness(2);
            border.Background = Brushes.White;
        }

        // Creates a styled Border element containing a child UIElement.
        private Border CreateBorderWithChild(UIElement child)
        {
            var border = new Border { Child = child };

            // Reference a predefined style for rounded borders.
            border.SetResourceReference(Border.StyleProperty, "RoundedBorderStyle");

            // Attach event handlers for drag-and-drop and hover effects.
            border.MouseLeftButtonDown += Element_MouseLeftButtonDown;
            border.Drop += Element_Drop;
            border.DragEnter += Border_DragEnter;
            border.DragLeave += Border_DragLeave;
            border.MouseEnter += (s, e) =>
            {
                var b = s as Border;
                // Light Sky Blue
                b.Background = new SolidColorBrush(Color.FromArgb(255, 135, 206, 250));
            };
            border.MouseLeave += (s, e) =>
            {
                var b = s as Border;
                b.Background = Brushes.White;
            };

            return border;
        }

        // Converts a byte array to a BitmapImage, useful for displaying images from binary data.
        public BitmapImage ConvertByteArrayToBitmapImage(byte[] byteArray)
        {
            using (var stream = new MemoryStream(byteArray))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                // Optimizes memory usage.
                bitmap.Freeze();
                return bitmap;
            }
        }

        public void OnCheckClicked()
        {
            IsEnabled = false;

            // Iterate through grid rows, assuming each row contains an image and text block pair.
            for (int column = 0; column < _assignment.Items.Count; column++)
            {
                var pairStackPanel = spMatchOptions.Children[column] as StackPanel;

                var imageBorder = pairStackPanel.Children[0] as Border;
                var textBlockBorder = pairStackPanel.Children[1] as Border;

                // Skip the iteration if either image or text block is missing.
                if (imageBorder == null || textBlockBorder == null) continue;

                var image = imageBorder.Child as Image;
                var textBlock = textBlockBorder.Child as TextBlock;

                // Show a message and signal incomplete assignment if any pair does not match.
                if (image != null && textBlock != null && image.Name != textBlock.Name)
                {
                    AssignmentCompleted?.Invoke(_assignment, false);
                    return;
                }
            }

            // If all pairs match, show a success message and signal assignment completion.
            AssignmentCompleted?.Invoke(_assignment, true);
        }

        public void OnRetryClicked()
        {
            IsEnabled = true;
        }

        public void OnNextClicked()
        {
            IsEnabled = true;
        }

        public void OnPreviousClicked()
        {
            IsEnabled = true;
        }
    }
}
