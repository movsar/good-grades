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
    public partial class SelectionAssignmentEditor : Window, IAssignmentEditor
    {
        private SelectingAssignment _assignment;
        public IAssignment Assignment => _assignment;
        private ContentStore ContentStore => App.AppHost!.Services.GetRequiredService<ContentStore>();

        public SelectionAssignmentEditor(SelectingAssignment? assignment = null)
        {
            InitializeComponent();
            DataContext = this;
            Log.Information("Selection assignment editor opened");

            if (assignment == null)
            {
                _assignment = new SelectingAssignment();
            }
            else
            {
                _assignment = assignment;
                txtTitle.Text = _assignment.Question.Text;
            }

            RedrawAssignmentItems();
        }

        public void RedrawAssignmentItems()
        {
            spItems.Children.Clear();

            //добавление существующих элементов в интерфейс
            foreach (var item in _assignment.Question.Options)
            {
                var existingQuizItemControl = new AssignmentItemEditControl(AssignmentType.Selecting, item);
                existingQuizItemControl.Discarded += OnAssignmentItemDiscarded;
                spItems.Children.Add(existingQuizItemControl);
            }

            //создание пустого поля для добавления нового элемента
            var newItemControl = new AssignmentItemEditControl(AssignmentType.Selecting);
            newItemControl.Committed += OnAssignmentItemCommitted;
            spItems.Children.Add(newItemControl);
        }

        private void SaveAssignment()
        {
            if (_assignment == null || string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Введите заголовок");
                return;
            }

            // Update assignment data
            _assignment.Title = txtTitle.Text;
            _assignment.Question.Text = txtTitle.Text;
            _assignment.Question.Options.Clear();
            IAssignmentEditor.SetAssignmentItems(_assignment.Question.Options, spItems);

            if (_assignment.Question.Options.Count < 2 || _assignment.Question.Options.FirstOrDefault(o => o.IsChecked == true) == null)
            {
                MessageBox.Show("Добавьте хотя бы два варианта ответа и хотя бы один выберите как правильный");
                return;
            }

            // If it's a new task, add it to the selected segment
            var existingAssignment = ContentStore.SelectedSegment!.SelectionAssignments.FirstOrDefault(st => st.Id == _assignment.Id);
            if (existingAssignment == null)
            {
                ContentStore.SelectedSegment!.SelectionAssignments.Add(_assignment);
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

        #region Event Handlers
        private void OnAssignmentItemDiscarded(AssignmentItem item)
        {
            _assignment.Question.Options.Remove(item);
            RedrawAssignmentItems();
        }

        private void OnAssignmentItemCommitted(AssignmentItem item)
        {
            _assignment.Question.Options.Add(item);
            RedrawAssignmentItems();
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveAssignment();
        }
        #endregion
    }
}
