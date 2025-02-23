using Data.Entities;
using Data.Interfaces;
using Serilog;
using Shared.Interfaces;
using Shared.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace Shared.Controls.Assignments
{
    public partial class TestingAssignmentControl : UserControl, IAssignmentControl
    {
        private int _currentQuestionIndex;
        private TestingAssignment _assignment;
        private List<TestingQuestionViewControl> _questionViewControls;

        public event Action<IAssignment, bool> AssignmentCompleted;
        public event Action<IAssignment, string, bool> AssignmentItemSubmitted;
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int CorrectAnswers { get; private set; }
        public int IncorrectAnswers { get; private set; }
        public int StepsCount { get; set; }

        public TestingAssignmentControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void LoadNextItem()
        {
            if (_currentQuestionIndex == _questionViewControls.Count - 1)
            {
                return;
            }

            ccQuestion.Content = _questionViewControls[++_currentQuestionIndex];
        }

        private void LoadPreviousItem()
        {
            if (_currentQuestionIndex == 0)
            {
                return;
            }

            ccQuestion.Content = _questionViewControls[--_currentQuestionIndex];
        }

        private void RestartTest()
        {
            _currentQuestionIndex = -1;
            foreach (var control in _questionViewControls)
            {
                control.ResetSelections();
            }

            LoadNextItem();
        }

        public void OnCheckClicked()
        {
            var questionViewControl = _questionViewControls[_currentQuestionIndex];
            var selections = questionViewControl.SelectedOptionIds;

            CorrectAnswers = _questionViewControls.Count(qvc => QuestionService.CheckUserAnswers(qvc.Question, qvc.SelectedOptionIds));
            IncorrectAnswers = _assignment.Questions.Count - CorrectAnswers;

            var success = IncorrectAnswers == 0;

            AssignmentCompleted?.Invoke(_assignment, success);
            ccQuestion.Content = new StatisticsControl(CorrectAnswers, IncorrectAnswers);

            IsEnabled = false;
        }

        public void OnRetryClicked()
        {
            RestartTest();
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
            _currentQuestionIndex = -1;

            if (_assignment == assignment && ccQuestion.Content is not StatisticsControl)
            {
                return;
            }

            _assignment = (TestingAssignment)assignment;
            _questionViewControls = new List<TestingQuestionViewControl>();

            StepsCount = _assignment.Questions.Count;

            foreach (var question in _assignment.Questions)
            {
                var questionViewControl = new TestingQuestionViewControl(question);
                _questionViewControls.Add(questionViewControl);
            }

            LoadNextItem();
        }
    }
}