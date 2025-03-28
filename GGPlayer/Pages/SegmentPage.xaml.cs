using GGPlayer.Models;
using Data.Entities;
using Data.Interfaces;
using Shared.Services;
using System.Windows.Controls;
using System.Windows.Input;
using Shared.Controls;
using GGPlayer.Services;
using System.Collections.ObjectModel;
using System.Windows;
using Serilog;

namespace GGPlayer.Pages
{
    public partial class SegmentPage : Page
    {
        private readonly ShellNavigationService _navigationService;
        private readonly AssignmentsPage _assignmentsPage;
        private readonly MaterialViewerPage _materialViewerPage;
        public ObservableCollection<IMaterial> Materials { get; } = new();
        private Segment? _segment;

        public SegmentPage(ShellNavigationService navigationService,
            AssignmentsPage assignmentsPage,
            MaterialViewerPage materialViewerPage)
        {
            InitializeComponent();
            DataContext = this;
            Log.Information("Segment page was initialized");

            _navigationService = navigationService;
            _assignmentsPage = assignmentsPage;
            _materialViewerPage = materialViewerPage;
        }

        public void Initialize(Segment segment)
        {
            if (segment == null)
            {
                throw new ArgumentNullException(nameof(segment), "Segment cannot be null");
            }
            _segment = segment;

            if (!string.IsNullOrWhiteSpace(_segment.Description))
            {
                RtfService.LoadRtfFromText(rtbDescription, _segment.Description);
            }

            tbSegmentTitle.Text = _segment.Title;

            // Заполнение списка материалов
            Application.Current.Dispatcher.Invoke(() =>
            {
                Materials.Clear();
                if (_segment.Materials != null)
                {
                    foreach (var material in _segment.Materials.Cast<IMaterial>())
                    {
                        Materials.Add(material);
                    }
                }
            });

            // Проверка наличия заданий и добавление кнопки
            var assignments = GetAllAssignments();
            if (assignments != null && assignments.Count > 0)
            {
                // Add a dummy separator
                Materials.Add(new FakeSegmentMaterial()
                {
                    Title = ""
                });

                var fakeSegmentMaterial = new FakeSegmentMaterial()
                {
                    Id = "tasks",
                    Title = "Хаарш зер"
                };
                Materials.Add(fakeSegmentMaterial);
            }
        }

        private List<IAssignment> GetAllAssignments()
        {
            var allAssignments = new List<IAssignment>();

            if (_segment.MatchingAssignments != null)
            {
                allAssignments.AddRange(_segment.MatchingAssignments);
            }
            if (_segment.FillingAssignments != null)
            {
                allAssignments.AddRange(_segment.FillingAssignments);
            }
            if (_segment.BuildingAssignments != null)
            {
                allAssignments.AddRange(_segment.BuildingAssignments);
            }
            if (_segment.TestingAssignments != null)
            {
                allAssignments.AddRange(_segment.TestingAssignments);
            }
            if (_segment.SelectionAssignments != null)
            {
                allAssignments.AddRange(_segment.SelectionAssignments);
            }

            return allAssignments;
        }

        private void OnListViewItemSelected()
        {
            if (lvMaterials.SelectedItem == null)
            {
                return;
            }

            var segmentItem = (IMaterial)lvMaterials.SelectedItem;

            if (segmentItem is FakeSegmentMaterial && segmentItem.Id == "tasks")
            {
                var assignments = GetAllAssignments();
                _assignmentsPage.Initialize(assignments);
                _navigationService.NavigateTo(_assignmentsPage);
            }
            else if (segmentItem is Material material)
            {
                _materialViewerPage.Initialize(material);
                _navigationService.NavigateTo(_materialViewerPage);
            }
        }

        private void lvMaterialsItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OnListViewItemSelected();
        }

        private void lvMaterials_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                OnListViewItemSelected();
            }
        }
    }
}
