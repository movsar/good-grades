﻿using Content_Manager.Stores;
using Content_Manager.UserControls.MaterialControls;
using Data.Enums;
using Data.Interfaces;
using Data.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Content_Manager.UserControls.Tabs
{
    public partial class TestingQuizTab : UserControl
    {
        private readonly ContentStore _contentStore = App.AppHost!.Services.GetRequiredService<ContentStore>();
        public TestingQuizTab()
        {
            InitializeComponent();
            DataContext = this;

            foreach (var question in _contentStore.SelectedSegment!.TestingQuiz.Questions)
            {
                var questionControl = new QuestionControl(question);
                questionControl.Add += QuestionControl_Add;
                questionControl.Save += QuestionControl_Save;
                questionControl.Delete += QuestionControl_Delete;

                spItems.Children.Add(questionControl);

            }
            var newQuestion = new QuestionControl();
            spItems.Children.Add(newQuestion);
            newQuestion.Add += QuestionControl_Add;
            newQuestion.Save += QuestionControl_Save;
            newQuestion.Delete += QuestionControl_Delete;
        }


        private void UpdateQuiz()
        {
            _contentStore.UpdateQuiz(QuizTypes.Testing);
        }

        private void QuestionControl_Add(TestingQuestion testingQuestion)
        {
            _contentStore.SelectedSegment?.TestingQuiz.Questions.Add(testingQuestion);

            UpdateQuiz();
        }

        private void QuestionControl_Delete(string questionId)
        {
            var itemToRemove = _contentStore.SelectedSegment?.TestingQuiz.Questions.Where(qi => qi.Id == questionId).First();
            _contentStore.SelectedSegment?.TestingQuiz.Questions.Remove(itemToRemove!);

            UpdateQuiz();
        }

        private void QuestionControl_Save()
        {
            UpdateQuiz();
        }
    }
}
