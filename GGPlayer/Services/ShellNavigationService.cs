using GGPlayer.Pages.Assignments;
using GGPlayer.Pages;
using Microsoft.Extensions.DependencyInjection;
using Plugin.SimpleAudioPlayer;
using System.Windows.Controls;
using System.Windows.Navigation;
using Serilog;

namespace GGPlayer.Services
{
    public class ShellNavigationService
    {
        private static readonly ILogger Logger = Log.ForContext<ShellNavigationService>();

        public event Action<Page> Navigated;
        private Frame _frame;
        private Page _currentPage;

        public bool CanGoBack => _frame?.CanGoBack ?? false;
        public bool IsNavigating { get; set; } = false;

        public void Initialize(Frame frame)
        {
            Log.Information("Initializing navigation service");
            _frame = frame;
            _frame.Navigated += _frame_Navigated;
            _frame.Navigating += _frame_Navigating;
            _frame.NavigationStopped += _frame_NavigationStopped;
        }

        private void _frame_NavigationStopped(object sender, NavigationEventArgs e)
        {
            Log.Debug("Navigation stopped");
            IsNavigating = false;
        }

        private void _frame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            IsNavigating = true;
        }

        public void NavigateTo(Page page)
        {
            Log.Debug($"Navigating to page: {page.GetType().Name}");
            _currentPage = page;
            _frame.Navigate(page);
        }

        private void _frame_Navigated(object sender, NavigationEventArgs e)
        {
            IsNavigating = false;
            Navigated?.Invoke(_currentPage);
        }

        public void NavigateTo<T>() where T : Page
        {
            Log.Debug($"Resolving and navigating to page: {typeof(T).Name}");
            var page = App.AppHost!.Services.GetRequiredService<T>();
            NavigateTo(page);
        }

        public void GoBack(string? originatingTypeName)
        {
            if (!CanGoBack)
            {
                Log.Debug("Cannot go back - no navigation history");
                return;
            }

            if (originatingTypeName == null)
            {
                Log.Debug("Performing standard back navigation");
                _frame.GoBack();
                return;
            }

            Log.Debug($"Handling custom back navigation from {originatingTypeName}");

            switch (originatingTypeName)
            {
                case nameof(MainPage):
                    Log.Debug("Custom back to StartPage");
                    NavigateTo<StartPage>();
                    break;

                case nameof(SegmentPage):
                    Log.Debug("Custom back to MainPage");
                    NavigateTo<MainPage>();
                    break;

                case nameof(MaterialViewerPage):
                    Log.Debug("Stopping audio and navigating back to SegmentPage");
                    CrossSimpleAudioPlayer.Current.Stop();
                    NavigateTo<SegmentPage>();
                    break;

                case nameof(AssignmentsPage):
                    Log.Debug("Custom back to SegmentPage");
                    NavigateTo<SegmentPage>();
                    break;

                case nameof(AssignmentViewerPage):
                    Log.Debug("Custom back to AssignmentsPage");
                    NavigateTo<AssignmentsPage>();
                    break;
            }
        }
    }
}