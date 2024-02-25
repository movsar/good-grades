using Data.Entities;
using Data.Entities.TaskItems;
using Data.Interfaces;
using Shared.Controls;
using Shared.Interfaces;
using Shared.Translations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Shared.Viewers
{
    public partial class BuildingViewer : Window, IAssignmentViewer
    {
        IList<AssignmentItem> _items;

        public event Action<IAssignment, bool> CompletionStateChanged;

        private readonly BuildingTaskAssignment _assignment;

        public string TaskTitle { get; }
        public BuildingViewer(BuildingTaskAssignment taskAssignment)
        {
            InitializeComponent();
            DataContext = this;

            _assignment = taskAssignment;

            TaskTitle = _assignment.Title;
            _items = _assignment.Items;

            foreach (var item in _items)
            {
                var buildingItemViewControl = new BuildingItemViewControl(item.Text);
                spItems.Children.Add(buildingItemViewControl);
            }
        }
        private void btnCheck_Click(object sender, RoutedEventArgs e)
        {
            foreach (BuildingItemViewControl buildingItemViewControl in spItems.Children)
            {
                // Check if the user input matches one of the options
                //if (!IsCorrectlyArranged)
                //{
                //    CompletionStateChanged?.Invoke(_assignment, false);
                //    MessageBox.Show(Ru.Incorrect);
                //    return;
                //}
            }

            // Show a message if all inputs are correct
            CompletionStateChanged?.Invoke(_assignment, true);
            MessageBox.Show(Ru.Correct);
        }
    }
}
