﻿using GGManager.Interfaces;
using GGManager.Models;
using GGManager.Services;
using GGManager.Stores;
using GGManager.Windows.Editors;
using Data;
using Data.Entities;
using Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Shared;
using Shared.Controls.Assignments;
using Serilog.Filters;
using Shared.Interfaces;
using Shared.Controls;

namespace GGManager.UserControls
{
    public partial class AssignmentControl : UserControl
    {
        #region Properties and Fields
        private FormCompletionInfo _formCompletionInfo;
        private AssignmentType _taskType;
        ContentStore ContentStore => App.AppHost!.Services.GetRequiredService<ContentStore>();
        StylingService StylingService => App.AppHost!.Services.GetRequiredService<StylingService>();

        public bool IsContentSet { get; set; }

        private readonly IAssignment _assignment;

        #endregion

        #region Initialization
        private void SetUiForNewMaterial()
        {
            btnPreview.Visibility = Visibility.Collapsed;
            btnDelete.Visibility = Visibility.Collapsed;
        }
        private void SetUiForExistingMaterial()
        {
            btnSetData.Background = IsContentSet ? StylingService.ReadyBrush : StylingService.StagedBrush;
            btnDelete.Visibility = Visibility.Visible;
            cmbTaskType.IsEnabled = false;
        }

        private void SharedInitialization(bool isExistingMaterial = false)
        {
            InitializeComponent();
            DataContext = this;

            var propertiesToWatch = new List<string>(){
                nameof(IsContentSet)
            };

            _formCompletionInfo = new FormCompletionInfo(propertiesToWatch, isExistingMaterial);
            _formCompletionInfo.StatusChanged += OnFormStatusChanged;
        }

        public AssignmentControl()
        {
            SharedInitialization();

            AddTaskTypes();
            SetUiForNewMaterial();
        }

        public AssignmentControl(IAssignment taskMaterial)
        {
            _assignment = taskMaterial;
            IsContentSet = GetCurrentTaskItems().Count() > 0;

            SharedInitialization(true);

            AddTaskTypes();
            SetSelectedTaskType();

            SetUiForExistingMaterial();
        }
        #endregion
        private void OnFormStatusChanged(bool isReady)
        {
            if (isReady)
            {
                btnPreview.Visibility = Visibility.Visible;
            }
            else
            {
                btnPreview.Visibility = Visibility.Collapsed;
            }
        }

        private void btnSetData_Click(object sender, RoutedEventArgs e)
        {
            IAssignment taskAssignment;
            //редакторы заданий в зависимости от типа задания
            IAssignmentEditor taskEditor = _taskType switch
            {
                AssignmentType.Matching => new MatchingAssignmentEditor(_assignment as MatchingAssignment),
                AssignmentType.Filling => new FillingAssignmentEditor(_assignment as FillingAssignment),
                AssignmentType.Selecting => new SelectionAssignmentEditor(_assignment as SelectingAssignment),
                AssignmentType.Building => new BuildingAssignmentEditor(_assignment as BuildingAssignment),
                AssignmentType.Test => new TestingAssignmentEditor(_assignment as TestingAssignment),
                _ => throw new NotImplementedException()
            };

            taskEditor.ShowDialog();
            taskAssignment = taskEditor.Assignment;

            if (_assignment != null)
            {
                // Data update may or may not have been done
                ContentStore.RaiseItemUpdatedEvent(taskAssignment);
            }
            else if (taskAssignment.IsContentSet)
            {
                // Content has been specified for a new task
                ContentStore.RaiseItemAddedEvent(taskAssignment);
            }
        }

        private void btnPreview_Click(object sender, RoutedEventArgs e)
        {
            Window window = new Window()
            {
                Width = 1280,
                Height = 720,
                Title = "Good Grades"
            };

            IAssignmentControl assignmentControl = null!;
            switch (_taskType)
            {
                case AssignmentType.Matching:
                    assignmentControl = new MatchingAssignmentControl();
                    break;

                case AssignmentType.Test:
                    assignmentControl = new TestingAssignmentControl();
                    break;

                case AssignmentType.Filling:
                    assignmentControl = new FillingAssignmentControl();
                    break;

                case AssignmentType.Selecting:
                    assignmentControl = new SelectingAssignmentControl();
                    break;

                case AssignmentType.Building:
                    assignmentControl = new BuildingAssignmentControl();
                    break;
            }

            var viewer = new AssignmentViewerControl();
            viewer.Initialize(_assignment, assignmentControl);
            window.Content = viewer;
            window.Show();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            ContentStore.DbContext.Remove(_assignment);
            ContentStore.DbContext.SaveChanges();

            ContentStore.RaiseItemDeletedEvent(_assignment);
        }

        private void cmbTaskType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cmbItem = e.AddedItems[0] as ComboBoxItem;
            var selectedItemTitle = cmbItem?.Content.ToString();
            if (string.IsNullOrEmpty(selectedItemTitle))
            {
                return;
            }

            btnSetData.IsEnabled = true;
            _taskType = GetSelectedTaskType(selectedItemTitle);
        }

        private void AddTaskTypes()
        {
            //варианты задания
            var fillingTaskType = new ComboBoxItem()
            {
                Content = Translations.GetValue("FillingTaskName")
            };

            var selectingTaskType = new ComboBoxItem()
            {
                Content = Translations.GetValue("SelectingTaskName")
            };

            var testingTaskType = new ComboBoxItem()
            {
                Content = Translations.GetValue("TestTaskName")
            };

            var matchingTaskType = new ComboBoxItem()
            {
                Content = Translations.GetValue("MatchingTaskName")
            };

            var buildingTaskMaterial = new ComboBoxItem()
            {
                Content = Translations.GetValue("BuildingTaskName")
            };

            cmbTaskType.Items.Add(fillingTaskType);
            cmbTaskType.Items.Add(selectingTaskType);
            cmbTaskType.Items.Add(testingTaskType);
            cmbTaskType.Items.Add(matchingTaskType);
            cmbTaskType.Items.Add(buildingTaskMaterial);
        }

        //получение текущих элементов заданий
        private IEnumerable<object> GetCurrentTaskItems()
        {
            IEnumerable<object> items = _assignment switch
            {
                MatchingAssignment mt => mt.Items,
                FillingAssignment ft => ft.Items,
                SelectingAssignment st => st.Question.Options,
                TestingAssignment tt => tt.Questions,
                BuildingAssignment bt => bt.Items,
                _ => throw new NotImplementedException()
            };

            return items;
        }

        private void SetSelectedTaskType()
        {
            //установка выбранного типа задания
            string selectedTaskName = _assignment switch
            {
                FillingAssignment _ => Translations.GetValue("FillingTaskName"),
                SelectingAssignment _ => Translations.GetValue("SelectingTaskName"),
                TestingAssignment _ => Translations.GetValue("TestTaskName"),
                BuildingAssignment _ => Translations.GetValue("BuildingTaskName"),
                MatchingAssignment _ => Translations.GetValue("MatchingTaskName"),
                _ => ""
            };

            cmbTaskType.SelectedItem = cmbTaskType.Items
                                .Cast<ComboBoxItem>()
                                .FirstOrDefault(item => item.Content.ToString() == selectedTaskName);
        }

        private AssignmentType GetSelectedTaskType(string selectedTaskTypeTitle)
        {
            if (selectedTaskTypeTitle.Equals(Translations.GetValue("FillingTaskName")))
            {
                return AssignmentType.Filling;
            }
            else if (selectedTaskTypeTitle.Equals(Translations.GetValue("TestTaskName")))
            {
                return AssignmentType.Test;
            }
            else if (selectedTaskTypeTitle.Equals(Translations.GetValue("BuildingTaskName")))
            {
                return AssignmentType.Building;
            }
            else if (selectedTaskTypeTitle.Equals(Translations.GetValue("SelectingTaskName")))
            {
                return AssignmentType.Selecting;
            }
            else if (selectedTaskTypeTitle.Equals(Translations.GetValue("MatchingTaskName")))
            {
                return AssignmentType.Matching;
            }
            else
            {
                throw new Exception("No matching TaskType has been found");
            }
        }
    }
}