using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Data.Interfaces;
using GGPlayer.Pages.Assignments;
using GGPlayer.Services;
using Serilog;
using Shared.Controls;
using Shared.Interfaces;

namespace GGPlayer.Pages
{
    public partial class AssignmentsPage : Page
    {
        private List<int> _completedAssignments = new List<int>();
        private List<IAssignment> _assignments;

        private readonly ShellNavigationService _navigationService;
        private readonly AssignmentViewerPage _assignmentViewerPage;

        private bool _canClick = true;
        private readonly TimeSpan _clickDelay = TimeSpan.FromMilliseconds(100);

        public AssignmentsPage(ShellNavigationService navigationService, AssignmentViewerPage assignmentViewerPage)
        {
            InitializeComponent();
            Log.Information("Assignment page was initialized");
            _navigationService = navigationService;
            _assignmentViewerPage = assignmentViewerPage;

            _assignmentViewerPage.AssignmentCompleted += OnAssignmentCompleted;
        }

        public void Initialize(List<IAssignment> assignments)
        {
            // This prevents re-rendering if it's the same segment, maiinly to keep the button completion effects
            if (_assignments != null && _assignments.FirstOrDefault()?.Id == assignments.FirstOrDefault()?.Id)
            {
                return;
            }
            _assignments = assignments;

            GenerateAssignmentButtons();
        }

        private void GenerateAssignmentButtons()
        {
            // Очистка предыдущего содержимого
            wrapPanel.Children.Clear();

            int count = 1;
            foreach (var assignment in _assignments)
            {
                var label = new Label()
                {
                    Content = count,
                    Style = (Style)FindResource("AssignmentButtonStyle"),
                    Tag = assignment.Id
                };

                // Устанавливаем событие клика на изображение
                label.MouseDown += AssignmentButton_Click;

                // Помещаем Grid в WrapPanel
                wrapPanel.Children.Add(label);
                count++;
            }
        }

        // Метод для изменения цвета границы
        private void ChangeBorderColor(IAssignment assignment, bool isCompleted)
        {
            var button = FindAssignmentButton(assignment);
            if (button == null)
            {
                return;
            }

            var effect = new DropShadowEffect();
            if (isCompleted)
            {
                effect.Color = Colors.LimeGreen;
                effect.BlurRadius = 30;
                effect.ShadowDepth = 5;
            }
            else
            {
                effect.Color = Colors.Transparent;
                effect.BlurRadius = 0;
                effect.ShadowDepth = 0;
            }

            button.Effect = effect;
        }

        // Метод для поиска изображения задания
        private Label? FindAssignmentButton(IAssignment assignment)
        {
            foreach (var child in wrapPanel.Children)
            {
                var button = child as Label;
                if (button != null && button.Tag != null && button.Tag.Equals(assignment.Id))
                {
                    return button;
                }
            }
            return null;
        }
        private async void AssignmentButton_Click(object sender, MouseButtonEventArgs e)
        {
            if (!this.IsEnabled)
            {
                return;
            }
            this.IsEnabled = false;

            var clickedButtonLabel = (Label)sender;
            var assignmentIndex = int.Parse(clickedButtonLabel.Content.ToString()!) - 1;
            
            var assignment = _assignments[assignmentIndex];
            NavigateToAssignment(assignment);

            // Wait for x ms before letting user to click again
            await Task.Delay(_clickDelay);
            this.IsEnabled = true;
        }

        private void NavigateToAssignment(IAssignment assignment)
        {
            _assignmentViewerPage.LoadAssignment(assignment);
            _navigationService.NavigateTo(_assignmentViewerPage);
        }

        private void OnAssignmentCompleted(IAssignment assignment, bool success)
        {
            // Находим индекс задания в списке
            var assignmentIndex = _assignments.IndexOf(assignment);
            if (assignmentIndex == -1)
            {
                return;
            }

            // Добавляем задание в список выполненных, если оно еще не добавлено
            if (!_completedAssignments.Contains(assignmentIndex))
            {
                _completedAssignments.Add(assignmentIndex);
            }

            // Находим изображение задания и перекрашиваем его границу в зеленый
            SetAssignmentButtonState(assignment, true);

            if (_completedAssignments.Count == _assignments.Count)
            {
                return;
            }

            var nextTaskIndex = assignmentIndex + 1;
            if (nextTaskIndex < _assignments.Count)
            {
                // Загрузка следующего задания
                var nextAssignment = _assignments[nextTaskIndex];
                NavigateToAssignment(nextAssignment);
            }
            else
            {
                // После последнего возвращаемся на экран выбора заданий
                _navigationService.NavigateTo(this);
            }
        }

        private void SetAssignmentButtonState(IAssignment assignment, bool successfullyCompleted)
        {
            var button = FindAssignmentButton(assignment);
            if (button == null)
            {
                return;
            }

            // Изменяем цвет границы
            ChangeBorderColor(assignment, successfullyCompleted);
        }


    }
}