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
using System;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace GGManager.Windows.Editors
{
    public partial class FillingAssignmentEditor : Window, IAssignmentEditor
    {
        private FillingAssignment _assignment;
        public IAssignment Assignment => _assignment;
        private ContentStore ContentStore => App.AppHost!.Services.GetRequiredService<ContentStore>();

        public FillingAssignmentEditor(FillingAssignment? taskEntity = null)
        {
            InitializeComponent();
            DataContext = this;
            Log.Information("Filling assignment editor opened");
            _assignment = taskEntity ?? new FillingAssignment()
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
                var existingItemControl = new AssignmentItemEditControl(AssignmentType.Filling, item);
                existingItemControl.Discarded += OnAssignmentItemDiscarded;

                spItems.Children.Add(existingItemControl);
            }

            //добавление пустого элемента для новых записей
            var newItemControl = new AssignmentItemEditControl(AssignmentType.Filling);
            newItemControl.Committed += OnAssignmentItemCommitted;

            spItems.Children.Add(newItemControl);
        }

        private void OnAssignmentItemCommitted(AssignmentItem item)
        {
            //добавление элементов в задание
            _assignment.Items.Add(item);
            RedrawItems();
        }
        private void OnAssignmentItemDiscarded(AssignmentItem item)
        {
            //удаление элементов из задания
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

            foreach (var control in spItems.Children.OfType<AssignmentItemEditControl>())
            {
                if (_assignment.Items.Contains(control.Item))
                {
                    control.Validate();

                    if (!control.IsValid)
                    {
                        RefreshControlUI(control);
                        return;
                    }
                }
            }

            // Update assignment data
            _assignment.Title = txtTitle.Text;
            IAssignmentEditor.SetAssignmentItems(_assignment.Items, spItems);

            var existingAssignment = ContentStore.SelectedSegment!.FillingAssignments.FirstOrDefault(a => a.Id == _assignment.Id);
            if (existingAssignment == null)
            {
                ContentStore.SelectedSegment!.FillingAssignments.Add(_assignment);
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

        private void RefreshControlUI(AssignmentItemEditControl control)
        {
            control.txtItemText.Text = control.InitialText;
        }
    }
}
