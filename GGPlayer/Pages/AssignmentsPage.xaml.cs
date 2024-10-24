﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Data.Entities;
using Data.Interfaces;
using Shared.Interfaces;
using Shared.Viewers;

namespace GGPlayer.Pages
{
    public partial class AssignmentsPage : Page
    {
        private List<int> completedAssignments = new List<int>();
        private List<IAssignment> Assignments { get; } = new List<IAssignment>();
        const int MaxAssignments = 30;
        const int ButtonSize = 100;
        const int ButtonSpacing = 20;

        public AssignmentsPage(List<IAssignment> assignments)
        {
            InitializeComponent();
            // Ограничиваем список заданий до 30
            Assignments.AddRange(assignments.Take(MaxAssignments));
            GenerateAssignmentButtons();
        }

        private void GenerateAssignmentButtons()
        {
            // Очистка предыдущего содержимого
            ScrollViewerContainer.Content = null;

            WrapPanel wrapPanel = new WrapPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = ScrollViewerContainer.ActualWidth
            };

            int count = 1;
            foreach (var assignment in Assignments)
            {
                // Создаем Grid для кнопки и фона
                var buttonGrid = new Grid
                {
                    Width = 160,
                    Height = 160,
                    Margin = new Thickness(ButtonSpacing)
                };



                // Фоновое изображение
                var backgroundImage = new Image
                {
                    Source = new BitmapImage(new Uri($"/Images/TaskButtons/Task{count}Btn.png", UriKind.Relative)),
                    Stretch = Stretch.Fill,
                    Width = 160,
                    Height = 160,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Cursor = Cursors.Hand,
                    Tag = assignment
                };

                // Эффект тени, изначально прозрачный
                DropShadowEffect shadowEffect = new DropShadowEffect
                {
                    Color = Colors.Transparent,
                    ShadowDepth = 0,
                    BlurRadius = 0
                };
                backgroundImage.Effect = shadowEffect;

                // Устанавливаем событие клика на изображение
                backgroundImage.MouseDown += AssignmentButton_Click;

                // Добавляем изображение в Grid
                buttonGrid.Children.Add(backgroundImage);

                // Помещаем Grid в WrapPanel
                wrapPanel.Children.Add(buttonGrid);
                count++;
            }

            ScrollViewerContainer.Content = wrapPanel;
        }

        // Метод для изменения цвета границы
        private void ChangeBorderColor(IAssignment assignment, bool isCompleted)
        {
            var image = FindAssignmentImage(assignment);
            if (image != null)
            {
                var effect = image.Effect as DropShadowEffect;

                if (effect == null)
                {
                    effect = new DropShadowEffect
                    {
                        ShadowDepth = 0,
                        BlurRadius = 20, 
                        Opacity = 0.8 
                    };
                    image.Effect = effect;
                }

                if (isCompleted)
                {
                    effect.Color = Colors.LimeGreen; 
                    effect.BlurRadius = 30;
                }
                else
                {
                    effect.Color = Colors.Transparent;
                    effect.BlurRadius = 0;
                }
            }
        }

        // Метод для поиска изображения задания
        private Image FindAssignmentImage(IAssignment assignment)
        {
            foreach (var child in ((WrapPanel)ScrollViewerContainer.Content).Children)
            {
                var grid = child as Grid;
                if (grid != null && grid.Children.Count > 0)
                {
                    var image = grid.Children[0] as Image;
                    // Проверяем, что image не null и Tag тоже не null
                    if (image != null && image.Tag != null && image.Tag.Equals(assignment))
                    {
                        return image;
                    }
                }
            }
            return null;
        }

        private void AssignmentButton_Click(object sender, MouseButtonEventArgs e)
        {
            var clickedImage = (Image)sender;
            var taskIndex = int.Parse(clickedImage.Source.ToString().Split('/').Last().Replace("Task", "").Replace("Btn.png", "")) - 1;
            var assignment = Assignments[taskIndex];

            Window viewer = null!;

            // В зависимости от типа задания открываем соответствующее окно
            switch (assignment)
            {
                case MatchingAssignment:
                    viewer = new MatchingViewer((MatchingAssignment)assignment);
                    break;
                case TestingAssignment:
                    viewer = new TestingViewer((TestingAssignment)assignment);
                    break;
                case FillingAssignment:
                    viewer = new FillingViewer((FillingAssignment)assignment);
                    break;
                case SelectingAssignment:
                    viewer = new SelectingViewer((SelectingAssignment)assignment);
                    break;
                case BuildingAssignment:
                    viewer = new BuildingViewer((BuildingAssignment)assignment);
                    break;
            }

            // Когда окно задания закрывается, проверяем состояние выполнения
            viewer.Closed += (s, args) =>
            {
                if (completedAssignments.Count == Assignments.Count)
                {
                    MessageBox.Show("ХӀокху декъера дерриг тӀедахкарш кхочушдинаахь!");
                }
            };

            // Подписываемся на событие изменения состояния выполнения
            ((IAssignmentViewer)viewer).CompletionStateChanged += AssignmentsPage_CompletionStateChanged;

            viewer.ShowDialog();
        }

        private void AssignmentsPage_CompletionStateChanged(IAssignment assignment, bool completionState)
        {
            if (!completionState)
            {
                return;
            }

            // Находим индекс задания в списке
            var assignmentIndex = Assignments.IndexOf(assignment);
            if (assignmentIndex == -1)
            {
                return;
            }

            // Добавляем задание в список выполненных, если оно еще не добавлено
            if (!completedAssignments.Contains(assignmentIndex))
            {
                completedAssignments.Add(assignmentIndex);
            }

            // Находим изображение задания и перекрашиваем его границу в зеленый
            var wrapPanel = ScrollViewerContainer.Content as WrapPanel;
            var image = FindAssignmentImage(assignment);

            if (image != null)
            {
                // Изменяем цвет границы
                ChangeBorderColor(assignment, true);
            }
          
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Подстраивание под изменение окна
            ScrollViewerContainer.Content = null;
            GenerateAssignmentButtons();
        }
    
        
    }
}