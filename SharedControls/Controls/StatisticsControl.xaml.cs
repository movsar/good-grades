﻿using System;
using System.Windows;
using System.Windows.Controls;

namespace Shared.Controls
{
    public partial class StatisticsControl : UserControl
    {
        public event Action<bool> AssignmentCompleted;
        public StatisticsControl(int correctAnswers, int incorrectAnswers)
        {
            InitializeComponent();

            txtCorrectAnswers.Text = $"Правильных ответов: {correctAnswers}";
            txtIncorrectAnswers.Text = $"Неправильных ответов: {incorrectAnswers}";

            if (incorrectAnswers > 0)
            {
                btnRetry.Visibility = Visibility.Visible;
            }
            else
            {
                btnNext.Visibility = Visibility.Visible;
            }
        }

        private void btnRetry_Click(object sender, RoutedEventArgs e)
        {
            //DialogResult = true;
            //this.Close();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            AssignmentCompleted?.Invoke(true); // Уведомляем о завершении, если все ответы правильные
            //this.DialogResult = true; // Закрываем окно с результатом
            //this.Close();
        }
    }
}