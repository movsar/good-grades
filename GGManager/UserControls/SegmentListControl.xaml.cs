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

        // клик по элементу списка для начала перетаскивания
        private void lvSegments_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is not DependencyObject source) return;

            var listViewItem = FindAncestor<ListViewItem>(source);
            if (listViewItem == null) return;

            _draggedSegment = (Segment?)listViewItem.Content;
            if (_draggedSegment != null)
            {
                DragDrop.DoDragDrop(lvSegments, _draggedSegment, DragDropEffects.Move);
            }
        }

        //событие перетаскивания поверх элементов в списке
        private void lvSegments_DragOver(object sender, DragEventArgs e)
        {
            if (_draggedSegment == null) return;

            e.Effects = DragDropEffects.Move;

            // Подсвечиваем элемент, на который наведена мышь
            if (e.OriginalSource is DependencyObject source)
            {
                var targetItem = FindAncestor<ListViewItem>(source);
                if (targetItem != null)
                {
                    targetItem.Background = Brushes.LightBlue;
                }
            }
        }

        //событие, когда мышь покидает элемент
        private void lvSegments_DragLeave(object sender, DragEventArgs e)
        {
            // Убираем подсветку с элемента, с которого ушла мышь
            if (e.OriginalSource is DependencyObject source)
            {
                var targetItem = FindAncestor<ListViewItem>(source);
                if (targetItem != null)
                {
                    targetItem.Background = Brushes.Transparent;
                }
            }
        }

        //событие завершения перетаскивания
        private void lvSegments_Drop(object sender, DragEventArgs e)
        {
            if (_draggedSegment == null) return;

            if (e.OriginalSource is DependencyObject source)
            {
                var targetItem = FindAncestor<ListViewItem>(source);
                if (targetItem != null)
                {
                    var targetSegment = (Segment?)targetItem.Content;
                    if (targetSegment != null && targetSegment != _draggedSegment)
                    {
                        SwapOrders(_draggedSegment, targetSegment);
                        RedrawSegmentList();
                    }
                }
            }
            _draggedSegment = null; 
        }

        // Меняет порядок двух сегментов в базе данных
        private void SwapOrders(Segment draggedSegment, Segment targetSegment)
        {
            int tempOrder = draggedSegment.Order;
            draggedSegment.Order = targetSegment.Order;
            targetSegment.Order = tempOrder;
            _contentStore.DbContext.SaveChanges();
        }

        // Находит родительский элемент заданного типа
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


        #endregion

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