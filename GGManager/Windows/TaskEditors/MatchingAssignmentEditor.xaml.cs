using GGManager.Interfaces;
using GGManager.Stores;
using GGManager.UserControls;
using Data;
using Data.Entities;
using Data.Entities.TaskItems;
using Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Windows;
using Serilog;

namespace GGManager.Windows.Editors
{
    public partial class MatchingAssignmentEditor : Window, IAssignmentEditor
    {
        private MatchingAssignment _assignment;
        public IAssignment Assignment => _assignment;
        private ContentStore ContentStore => App.AppHost!.Services.GetRequiredService<ContentStore>();

        public MatchingAssignmentEditor(MatchingAssignment? matchingTaskEntity = null)
        {
            InitializeComponent();
            DataContext = this;
            Log.Information("Matching assignment editor opened");
            _assignment = matchingTaskEntity ?? new MatchingAssignment()
            {
                Title = txtTitle.Text
            };
            txtTitle.Text = _assignment.Title;

            RedrawItems();
        }
       
        public void RedrawItems()
        {
            spItems.Children.Clear();

            //добавление существующих элементов в интерфейс
            foreach (var item in _assignment.Items)
            {
                var existingItemControl = new AssignmentItemEditControl(AssignmentType.Matching, item);
                existingItemControl.Discarded += OnAssignmentItemDiscarded;

                spItems.Children.Add(existingItemControl);
            }

            //создание пустого поля для добавления нового элемента
            var newItemControl = new AssignmentItemEditControl(AssignmentType.Matching);
            newItemControl.Committed += OnAssignmentItemCommitted;

            spItems.Children.Add(newItemControl);
        }

        private void OnAssignmentItemCommitted(AssignmentItem item)
        {
            //добавление 
            _assignment.Items.Add(item);
            RedrawItems();
        }
        private void OnAssignmentItemDiscarded(AssignmentItem item)
        {
            //удаление
            _assignment.Items.Remove(item);
            RedrawItems();
        }

        private void SaveAndClose()
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Введите заголовок");
                return;
            }

            if (_assignment.Items.Count == 0)
            {
                MessageBox.Show("Нужно добавить элементы");
                return;
            }

            // Update assignment data
            _assignment.Title = txtTitle.Text;
            IAssignmentEditor.SetAssignmentItems(_assignment.Items, spItems);

            var existingAssignment = ContentStore.SelectedSegment!.MatchingAssignments.FirstOrDefault(a => a.Id == _assignment.Id);
            if (existingAssignment == null)
            {
                ContentStore.SelectedSegment!.MatchingAssignments.Add(_assignment);
            }

            // Save and notify the changes
            ContentStore.DbContext.ChangeTracker.DetectChanges();
            ContentStore.DbContext.SaveChanges();

            if (existingAssignment == null)
            {
                ContentStore.RaiseItemAddedEvent(_assignment);
            }
            else
            {
                ContentStore.RaiseItemUpdatedEvent(_assignment);
            }

            Close();
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveAndClose();
        }

    }
}
