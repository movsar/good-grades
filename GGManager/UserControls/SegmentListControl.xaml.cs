using GGManager.Commands;
using GGManager.Stores;
using Data.Entities;
using Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Shared;
using System;

namespace GGManager.UserControls
{
    public partial class SegmentListControl : UserControl
    {
        private readonly ContentStore _contentStore = App.AppHost!.Services.GetRequiredService<ContentStore>();
        public ICommand DeleteSelectedSegment { get; }
        private Segment? _draggedSegment;

        public SegmentListControl()
        {
            InitializeComponent();
            DataContext = this;

            // Initialize commands
            DeleteSelectedSegment = new SegmentCommands.DeleteSegment(_contentStore, this);

            // Set events
            _contentStore.ItemDeleted += OnItemDeleted;
            _contentStore.ItemUpdated += OnItemUpdated;
            _contentStore.CurrentDatabaseChanged += _contentStore_CurrentDatabaseChanged;
            _contentStore.SegmentUpdated += _contentStore_SegmentUpdated; ;
        }

        private void _contentStore_SegmentUpdated(Segment segment)
        {
            RedrawSegmentList();
            _contentStore.SelectedSegment = segment;
        }

        private void _contentStore_CurrentDatabaseChanged()
        {
            RedrawSegmentList();
        }

        private void RedrawSegmentList(string? selectedSegmentId = null)
        {
            lvSegments.Items.Clear();
            foreach (var segment in _contentStore.DbContext.Segments.OrderBy(s => s.Order))
            {
                lvSegments.Items.Add(segment);
            }

            if (selectedSegmentId == null)
            {
                _contentStore.SelectedSegment = null;
                return;
            }

            var currentSegment = _contentStore.DbContext.Segments.Where(item => item.Id == selectedSegmentId);
            lvSegments.SelectedItem = currentSegment;
        }

        private void BtnNewSection_Click(object sender, RoutedEventArgs e)
        {
            // Создание нового сегмента с порядковым номером
            int maxOrder = _contentStore.DbContext.Segments.Any()
                ? _contentStore.DbContext.Segments.Max(s => s.Order)
                : 0;

            Segment segment = new Segment()
            {
                Title = Translations.GetValue("NewChapter"),
                Order = maxOrder + 1
            };

            // Добавление нового сегмента в БД и сохранение
            _contentStore.DbContext.Add(segment);
            _contentStore.DbContext.SaveChanges();

            RedrawSegmentList();
            _contentStore.SelectedSegment = segment;
        }

        private void lvSegments_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var segment = ((Segment)lvSegments.SelectedItem);
            if (segment == null) return;

            if (_contentStore.SelectedSegment?.Id == segment.Id)
            {
                return;
            }

            _contentStore.SelectedSegment = segment;
        }

        #region Drag-and-Drop

        private ListViewItem? _lastHighlightedItem;  // Track last highlighted item
        private Point _dragStartPoint;

        // Start drag operation when clicking an item
        private void lvSegments_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is not DependencyObject source) return;

            var listViewItem = FindAncestor<ListViewItem>(source);
            if (listViewItem == null) return;

            // Ensure the item is selected before drag starts
            if (!listViewItem.IsSelected)
            {
                listViewItem.IsSelected = true;
            }

            _dragStartPoint = e.GetPosition(null);
        }

        // Handle drop and swap items
        private void lvSegments_Drop(object sender, DragEventArgs e)
        {
            if (_draggedSegment == null) return;

            if (e.OriginalSource is DependencyObject source)
            {
                var targetItem = FindAncestor<ListViewItem>(source);
                if (targetItem == null || targetItem.Content == _draggedSegment) return;

                var targetSegment = targetItem.Content as Segment;
                if (targetSegment != null)
                {
                    SwapOrders(_draggedSegment, targetSegment);
                    RedrawSegmentList();
                    _contentStore.SelectedSegment = _draggedSegment;
                }
            }

            ResetHighlight();
            _draggedSegment = null;
        }

        // Reset item background when dragging leaves
        private void lvSegments_DragLeave(object sender, DragEventArgs e)
        {
            ResetHighlight();
        }

        private void lvSegments_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            var currentPos = e.GetPosition(null);
            if (Math.Abs(currentPos.X - _dragStartPoint.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(currentPos.Y - _dragStartPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
            {
                // Ignore small movements (not a drag)
                return;
            }

            var listViewItem = FindAncestor<ListViewItem>(e.OriginalSource as DependencyObject);
            if (listViewItem == null || !listViewItem.IsSelected) return;

            _draggedSegment = listViewItem.Content as Segment;
            if (_draggedSegment == null) return;

            DragDrop.DoDragDrop(lvSegments, _draggedSegment, DragDropEffects.Move);
        }
        #endregion

        // Swap the order of two segments in the database
        private void SwapOrders(Segment draggedSegment, Segment targetSegment)
        {
            int tempOrder = draggedSegment.Order;
            draggedSegment.Order = targetSegment.Order;
            targetSegment.Order = tempOrder;
            _contentStore.DbContext.SaveChanges();
        }

        // Reset the highlight of the last item
        private void ResetHighlight()
        {
            if (_lastHighlightedItem != null)
            {
                _lastHighlightedItem.Background = Brushes.Transparent;
                _lastHighlightedItem = null;
            }
        }

        // Find ancestor of a given type in the visual tree
        private static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T)
                    return (T)current;

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }
        #region Segment Event Handlers

        private void OnItemUpdated(IEntityBase entity)
        {
            if (entity is not Segment)
            {
                return;
            }

            RedrawSegmentList(entity.Id);
        }

        private void OnItemDeleted(IEntityBase entity)
        {
            if (entity is not Segment)
            {
                return;
            }

            RedrawSegmentList();
        }

        #endregion


    }
}