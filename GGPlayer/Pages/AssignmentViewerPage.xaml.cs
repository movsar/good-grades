using Data;
using Data.Entities;
using Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Shared.Controls;
using Shared.Controls.Assignments;
using Shared.Interfaces;
using System.Diagnostics;
using System.Windows.Controls;

namespace GGPlayer.Pages.Assignments
{
    public partial class AssignmentViewerPage : Page
    {
        public event Action<IAssignment, bool> AssignmentCompleted;
        public AssignmentViewerPage()
        {
            InitializeComponent();
            Log.Information("Assignment viewer page was initialized");
        }

        public void LoadAssignment(IAssignment assignment)
        {
            IAssignmentControl assignmentControl = null!;
            switch (assignment)
            {
                case MatchingAssignment:
                    assignmentControl = App.AppHost!.Services.GetRequiredService<MatchingAssignmentControl>();
                    break;

                case TestingAssignment:
                    assignmentControl = App.AppHost!.Services.GetRequiredService<TestingAssignmentControl>();
                    break;

                case FillingAssignment:
                    assignmentControl = App.AppHost!.Services.GetRequiredService<FillingAssignmentControl>();
                    break;

                case SelectingAssignment:
                    assignmentControl = App.AppHost!.Services.GetRequiredService<SelectingAssignmentControl>();
                    break;

                case BuildingAssignment:
                    assignmentControl = App.AppHost!.Services.GetRequiredService<BuildingAssignmentControl>();
                    break;
            }

            var viewer = App.AppHost!.Services.GetRequiredService<AssignmentViewerControl>();
            viewer.Initialize(assignment, assignmentControl);
            viewer.AssignmentCompleted -= Viewer_AssignmentCompleted;
            viewer.AssignmentCompleted += Viewer_AssignmentCompleted;
            ucRoot.Content = viewer;
            Log.Debug($"{assignment} was loaded");
        }

        private void Viewer_AssignmentCompleted(IAssignment assignment, bool success)
        {
            AssignmentCompleted?.Invoke(assignment, success);
        }
        
    }
}