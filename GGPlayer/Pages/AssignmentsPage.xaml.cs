﻿using Data.Entities;
using Data.Interfaces;
using Shared.Viewers;
using System.Windows;
using System.Windows.Controls;
using Shared.Interfaces;
using System.Windows.Input;
using System.Windows.Media;

namespace GGPlayer.Pages
{
    public partial class AssignmentsPage : Page
    {
        private List<IAssignment> Assignments { get; } = new List<IAssignment>();

        const int ButtonSize = 150;
        const int ButtonSpacing = 50;
        public AssignmentsPage(List<IAssignment> assignments)
        {
            InitializeComponent();
            Assignments.AddRange(assignments);
        }

        
        private void GenerateAssignmentButtons()
        {
            // Clear previous content
            ScrollViewerContainer.Content = null;

            // Determine the number of buttons that can fit in a row based on the container's width
            int buttonsPerRow = (int)(ScrollViewerContainer.ActualWidth / (ButtonSize + ButtonSpacing));

            WrapPanel wrapPanel = new WrapPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = ScrollViewerContainer.ActualWidth
            };

            int count = 1;
            foreach (var assignment in Assignments)
            {
                // Create a new button for the assignment
                Button button = new Button
                {
                    Content = count.ToString(),
                    Width = ButtonSize,
                    Height = ButtonSize,
                    Margin = new Thickness(ButtonSpacing),
                    Style = (Style)FindResource("CircularButtonStyle"),
                    Cursor = Cursors.Hand
                };

                button.Click += AssignmentButton_Click;

                wrapPanel.Children.Add(button);

                count++;
            }

            ScrollViewerContainer.Content = wrapPanel;
        }

        private void AssignmentButton_Click(object sender, RoutedEventArgs e)
        {
            var clickedButton = (Button)e.Source;
            var assignment = Assignments[int.Parse(clickedButton.Content.ToString()!) - 1];

            Window viewer = null!;
            //выбор типа задания
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

            ((IAssignmentViewer)viewer).CompletionStateChanged += AssignmentsPage_CompletionStateChanged;
            viewer.ShowDialog();

            // TODO: Check whether the user had solved anything, if so - set green
        }

        private void AssignmentsPage_CompletionStateChanged(IAssignment assignment, bool completionState)
        {
            if (!completionState)
            {
                return;
            }

            var assignmentIndex = Assignments.IndexOf(assignment);
            if (assignmentIndex == -1)
            {
                return;
            }

            //проверка кнопки на наличие контента
            var wrapPanel = ScrollViewerContainer.Content as WrapPanel;
            var buttonContentToFind = (assignmentIndex + 1).ToString();
            var button = wrapPanel.Children.OfType<Button>()
                              .FirstOrDefault(b => b.Content.ToString() == buttonContentToFind);
            //перекраска кнопки выполненного задания в зеленый
            if (button != null)
            {
                button.Background = new SolidColorBrush(Colors.Green);
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //подстраивание под изменение окна
            var widthDifference = ActualWidth - e.PreviousSize.Width;
            var heightDifference = ActualHeight - e.PreviousSize.Height;

            if (widthDifference < ButtonSize && heightDifference < ButtonSize)
            {
                return;
            }

            ScrollViewerContainer.Content = null;
            GenerateAssignmentButtons();
        }
    }
}
