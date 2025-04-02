using GGManager.Interfaces;
using GGManager.Stores;
using GGManager.UserControls;
using Data.Entities;
using Data.Entities.TaskItems;
using Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Serilog;

namespace GGManager.Windows.Editors
{
    public partial class TestingAssignmentEditor : Window, IAssignmentEditor
    {
        private TestingAssignment _assignment;
        public IAssignment Assignment => _assignment;
        private ContentStore ContentStore => App.AppHost!.Services.GetRequiredService<ContentStore>();

        public TestingAssignmentEditor(TestingAssignment? assignment = null)
        {
            InitializeComponent();
            DataContext = this;
            Log.Information("Testing assignment editor opened");

            _assignment = assignment ?? new TestingAssignment()
            {
                Title = txtTitle.Text
            };

            txtTitle.Text = _assignment.Title;

            RedrawQuestions();
        }
        //перерисовка интерфейса вопросов
        public void RedrawQuestions()
        {
            spItems.Children.Clear();
            
            //добавление существующих вопросов в интерфейс
            foreach (var question in _assignment.Questions)
            {
                var existingQuestionControl = new QuestionEditControl(question);
                existingQuestionControl.Discarded += OnQuestionDiscarded;

                spItems.Children.Add(existingQuestionControl);
            }
            
            //создание пустого поля для нового вопроса
            var newQuestionControl = new QuestionEditControl();
            newQuestionControl.Committed += OnQuestionCommitted;
            spItems.Children.Add(newQuestionControl);

            //ContentStore.ItemUpdated += Question_Updated;
        }

        private void OnQuestionCommitted(Question question)
        {
            //добавление вопроса
            _assignment.Questions.Add(question);
            RedrawQuestions();
        }

        private void OnQuestionDiscarded(Question question)
        {
            //удаление вопроса
            _assignment.Questions.Remove(question);
            RedrawQuestions();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void Save()
        {
            if (_assignment == null || !_assignment.Questions.Any() || string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Убедитесь что вы заполнили заголовок и добавили вопросы!");
                return;
            }

            // Update assignment data
            _assignment.Title = txtTitle.Text;

            // Extract Questions from UI
            _assignment.Questions.Clear();
            foreach (var item in spItems.Children)
            {
                var questionEditControl = item as QuestionEditControl;
                if (questionEditControl == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(questionEditControl.Question.Text))
                {
                    continue;
                }

                _assignment.Questions.Add(questionEditControl.Question);
            }

            // If it's a new task, add it to the selected segment
            var existingAssignment = ContentStore.SelectedSegment!.TestingAssignments.FirstOrDefault(st => st.Id == _assignment.Id);
            if (existingAssignment == null)
            {
                ContentStore.SelectedSegment!.TestingAssignments.Add(_assignment);
            }

            // Save and notify the changes
            ContentStore.DbContext.ChangeTracker.DetectChanges();
            ContentStore.DbContext.SaveChanges();

            if (existingAssignment == null)
            {
                ContentStore.RaiseItemAddedEvent(_assignment);
            }
            else
            {
                ContentStore.RaiseItemUpdatedEvent(_assignment);
            }

            Close();
        }
    }
}

