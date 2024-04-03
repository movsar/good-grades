﻿using Content_Manager.Models;
using Content_Manager.Stores;
using Data;
using Data.Entities;
using Data.Entities.TaskItems;
using Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Shared.Translations;
using Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Windows.Input;

namespace Content_Manager.UserControls
{
    public partial class QuestionEditControl : UserControl
    {
        #region Events
        public event Action<IEntityBase> QuestionCreated;
        public event Action<IEntityBase> QuestionUpdated;
        public event Action<string> QuestionDeleted;
        #endregion

        #region Fields
        static string Hint { get; } = Ru.SetDescription;
        private FormCompletionInfo _formCompletionInfo;
        #endregion

        #region Properties
        private readonly ContentStore ContentStore = App.AppHost!.Services.GetRequiredService<ContentStore>();
        StylingService StylingService => App.AppHost!.Services.GetRequiredService<StylingService>();
        private List<AssignmentItem> Options { get; set; } = new List<AssignmentItem>();

        private readonly Question _testingQuestion;

        public string QuestionText
        {
            get { return (string)GetValue(ItemTextProperty); }
            set { SetValue(ItemTextProperty, value); }
        }

        private TestingAssignment _task;
        private readonly string _taskId;
        public static readonly DependencyProperty ItemTextProperty =
            DependencyProperty.Register("QuestionText", typeof(string), typeof(QuestionEditControl), new PropertyMetadata(""));

        public string QuestionId { get; }

        #endregion

        #region Reactions
        private void OnFormStatusChanged(bool isReady)
        {
            if (isReady)
            {
                btnSaveQuestion.Visibility = Visibility.Visible;
            }
            else
            {
                btnSaveQuestion.Visibility = Visibility.Collapsed;
            }
        }
        private void OnTextSet(bool isSet)
        {
            _formCompletionInfo.Update(nameof(QuestionText), isSet);
        }
        #endregion

        #region Initialization
        private void SetUiForNewMaterial()
        {
            btnDeleteQuestion.Visibility = Visibility.Hidden;
            btnSaveQuestion.Visibility = Visibility.Hidden;
        }
        private void SetUiForExistingMaterial()
        {
            btnDeleteQuestion.Visibility = Visibility.Visible;
        }
        private void SharedInitialization(bool isExistingMaterial)
        {
            InitializeComponent();
            DataContext = this;

            var propertiesToWatch = new List<string>
            {
                nameof(QuestionText)
            };

            _formCompletionInfo = new FormCompletionInfo(propertiesToWatch, isExistingMaterial);
            _formCompletionInfo.StatusChanged += OnFormStatusChanged;
        }
        public QuestionEditControl(TestingAssignment task)
        {
            SharedInitialization(false);
            SetUiForNewMaterial();

            QuestionText = Hint;
            _testingQuestion = new Question();
            _task = task;
        }

        public QuestionEditControl(TestingAssignment task, Question testingQuestion)
        {
            SharedInitialization(true);
            SetUiForExistingMaterial();

            _testingQuestion = testingQuestion;
            _task = task;

            QuestionId = testingQuestion.Id!;
            QuestionText = testingQuestion.Text;
            Options = testingQuestion.Options.ToList();

            foreach (var answer in Options)
            {
                var existingItemControl = new AssignmentItemEditControl(TaskType.Test, answer);
                //existingItemControl.Removed += Option_Delete;

                spItems.Children.Add(existingItemControl);
            }

            var newItemControl = new AssignmentItemEditControl(TaskType.Test);

            spItems.Children.Add(newItemControl);

            OnTextSet(true);
        }


        #endregion

        private Question GetQuestionById(string questionId)
        {
            return ContentStore.SelectedSegment!.TestingTasks.SelectMany(t => t.Questions).Where(q => q.Id == questionId).First();
        }

        #region Event Handlers
        private void Question_Option_SetAsCorrect(string itemId)
        {
            _testingQuestion!.CorrectOptionId = itemId;
            ContentStore.DbContext.SaveChanges();

            ContentStore.RaiseItemUpdatedEvent(_testingQuestion);

        }
        private void Option_Create(IEntityBase entity)
        {
            var newAnswer = (AssignmentItem)entity;
            var question = GetQuestionById(QuestionId);

            _testingQuestion.Options.Add(newAnswer);
            ContentStore.DbContext.SaveChanges();

            QuestionUpdated?.Invoke(_testingQuestion);
        }
        private void Option_Save(IEntityBase model)
        {
            QuestionUpdated?.Invoke(_testingQuestion);
        }
        private void Option_Delete(string itemId)
        {
            var item = ContentStore.DbContext.Find<AssignmentItem>(itemId);

            _testingQuestion.Options.Remove(item);
            ContentStore.DbContext.SaveChanges();

            QuestionUpdated?.Invoke(_testingQuestion);
        }
        #endregion

        #region TextHandlers
        private void txtQuestionText_GotFocus(object sender, RoutedEventArgs e)
        {
            if (QuestionText == Hint)
            {
                QuestionText = "";
            }
        }

        private void txtQuestionText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(QuestionText))
            {
                QuestionText = Hint;
            }
        }

        private void txtQuestionText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtQuestion.Text) || txtQuestion.Text.Equals(Hint))
            {
                OnTextSet(false);
            }
            else
            {
                OnTextSet(true);
            }
        }
        #endregion

        #region Button Handlers
        private void btnSaveQuestion_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }
        
        private void Save()
        {

            // If the TestingTaskAssignment is new - add to database
            var taskState = ContentStore.DbContext.Entry(_task).State;
            if (taskState == EntityState.Detached || taskState == EntityState.Added)
            {
                ContentStore.SelectedSegment!.TestingTasks.Add(_task);
            }

            // Set testing question fields
            _testingQuestion.Text = QuestionText;
            _testingQuestion.Options.Clear();
            foreach (var option in Options)
            {
                _testingQuestion.Options.Add(option);
            }

            // If testing question is new - add to database
            var questionState = ContentStore.DbContext.Entry(_testingQuestion).State;
            if (questionState == EntityState.Detached || questionState == EntityState.Added)
            {
                _task.Questions.Add(_testingQuestion);
                QuestionCreated?.Invoke(_testingQuestion);
            }
            else
            {
                QuestionUpdated?.Invoke(_testingQuestion);
            }

            ContentStore.DbContext.SaveChanges();
        }

        private void btnDeleteQuestion_Click(object sender, RoutedEventArgs e)
        {
            QuestionDeleted?.Invoke(QuestionId);
        }
        #endregion

        private void txtQuestion_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                Save();
            }
        }
    }
}
