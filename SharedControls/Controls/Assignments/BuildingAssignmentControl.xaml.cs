using Data.Entities;
using Data.Interfaces;
using Serilog;
using Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Shared.Controls.Assignments
{
    public partial class BuildingAssignmentControl : UserControl, IAssignmentControl
    {

        private BuildingAssignment _assignment; 
        private int _currentItemIndex;

        public event Action<IAssignment, bool> AssignmentCompleted;
        public event Action<IAssignment, string, bool> AssignmentItemSubmitted;

        public string TaskTitle { get; }
        public int StepsCount { get; } = 1;
        private List<BuildingItemViewControl> viewControls = new List<BuildingItemViewControl>();
        public BuildingAssignmentControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void LoadNextItem()
        {
            if (_currentItemIndex == viewControls.Count - 1)
            {
                return;
            }

            spItems.Children.Clear();
            spItems.Children.Add(viewControls[++_currentItemIndex]);
        }

        private void LoadPreviousItem()
        {
            if (_currentItemIndex == 0)
            {
                return;
            }

            spItems.Children.Clear();
            spItems.Children.Add(viewControls[--_currentItemIndex]);
        }

        public void OnCheckClicked()
        {
            if (spItems.Children.Count == 0)
            {
                return;
            }

            // Get submission information
            var buildingItemViewControl = (BuildingItemViewControl)spItems.Children[0];
            var arrangedPhrase = GetUserArrangedPhrase(buildingItemViewControl);

            // Check whether the submission was correct
            var isItemSubmissionCorrect = (arrangedPhrase == buildingItemViewControl.Tag.ToString());
            AssignmentItemSubmitted?.Invoke(_assignment, _assignment.Items[_currentItemIndex].Id, isItemSubmissionCorrect);

            if (isItemSubmissionCorrect && _currentItemIndex == _assignment.Items.Count - 1)
            {
                AssignmentCompleted?.Invoke(_assignment, true);
            }

            // Block UI until the Next is clicked
            IsEnabled = false;
        }

        private string GetUserArrangedPhrase(BuildingItemViewControl buildingItemViewControl)
        {
            var words = new List<string>();
            foreach (var child in buildingItemViewControl.spItemDropZone.Children)
            {
                // Проверяем, является ли элемент кнопкой
                if (child is Button btnWord)
                {
                    words.Add(btnWord.Content.ToString());
                }
                // Если это Grid, например, то извлекаем текст из TextBlock внутри Grid
                else if (child is Grid gridWord)
                {
                    var textBlock = gridWord.Children.OfType<TextBlock>().FirstOrDefault();
                    if (textBlock != null)
                    {
                        words.Add(textBlock.Text);
                    }
                }
            }
            return string.Join(" ", words);
        }

        public void OnRetryClicked()
        {
            _currentItemIndex--;
            LoadNextItem();
            IsEnabled = true;
        }

        public void OnNextClicked()
        {
            LoadNextItem();
            IsEnabled = true;
        }

        public void OnPreviousClicked()
        {
            LoadPreviousItem();
            IsEnabled = true;
        }

        public void Initialize(IAssignment assignment)
        {
            IsEnabled = true;
            _currentItemIndex = -1;

            if (_assignment == assignment)
            {
                return;
            }
            _assignment = (BuildingAssignment)assignment;

            viewControls.Clear();
            foreach (var item in _assignment.Items)
            {
                var buildingItemViewControl = new BuildingItemViewControl(item) { Tag = item.Text };
                viewControls.Add(buildingItemViewControl);
            }

            LoadNextItem();
        }
    }
}