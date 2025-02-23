using Data.Entities.TaskItems;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Shared.Controls
{
    public partial class TestingQuestionViewControl : UserControl
    {
        public Question Question { get; }
        public List<string> SelectedOptionIds => GetUserSelections();

        private List<string> selectedOptionIds = new List<string>();
        private bool wasPreviousAttemptCorrect = true;
        private List<string> GetUserSelections()
        {
            return selectedOptionIds;
        }

        public TestingQuestionViewControl(Question question)
        {
            InitializeComponent();
            DataContext = this;

            Question = question;
            lblQuestion.Text = question.Text;
            spOptions.Children.Clear();
            foreach (var option in Question.Options)
            {
                spOptions.Children.Add(GenerateOptionButton(option));
            }
        }

        private Button GenerateOptionButton(AssignmentItem option)
        {
            var button = new Button()
            {
                Tag = option.Id,
                Background = Brushes.Transparent,
                Content = option.Text,
                Style = (Style)FindResource("QuestionOptionButtonStyle"),
            };

            button.Click += (s, e) => OnOptionSelected(button);

            return button;
        }

        private void OnOptionSelected(Button selectedButton)
        {
            if (!wasPreviousAttemptCorrect)
            {
                ResetSelections();
                wasPreviousAttemptCorrect = true;
            }

            var selectedId = selectedButton.Tag.ToString();

            if (selectedOptionIds.Contains(selectedId!))
            {
                selectedOptionIds.Remove(selectedId!);
                selectedButton.BorderBrush = Brushes.Gray;
            }
            else
            {
                selectedOptionIds.Add(selectedId!);
                selectedButton.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 60, 127, 177));
            }
        }

        public void HighlightCorrectOptions(List<string> correctOptionIds)
        {
            bool allCorrectSelected = correctOptionIds.All(id => SelectedOptionIds.Contains(id));
            bool anyWrongSelected = SelectedOptionIds.Any(id => !correctOptionIds.Contains(id));

            foreach (Button btn in spOptions.Children.OfType<Button>())
            {
                var optionId = btn.Tag.ToString();

                if (SelectedOptionIds.Contains(optionId!))
                {
                    if (anyWrongSelected || !allCorrectSelected)
                    {
                        btn.BorderBrush = Brushes.LightCoral;
                        wasPreviousAttemptCorrect = false;
                    }
                    else if (correctOptionIds.Contains(optionId!) && allCorrectSelected)
                    {
                        btn.BorderBrush = Brushes.LightGreen;
                        wasPreviousAttemptCorrect = true;
                    }
                }
                else
                {
                    btn.ClearValue(Button.BorderBrushProperty);
                }
            }
        }
        public void ResetSelections()
        {
            selectedOptionIds.Clear();
            foreach (Button btn in spOptions.Children.OfType<Button>())
            {
                btn.BorderBrush = Brushes.Gray;
            }
        }
    }
}
