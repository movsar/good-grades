using GGPlayer.Pages;
using GGPlayer.Services;
using Serilog;
using Shared;
using Shared.Controls;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace GGPlayer
{
    public partial class ShellWindow : Window
    {
        private readonly ShellNavigationService _navigationService;

        public ShellWindow(ShellNavigationService navigationService)
        {
            InitializeComponent();
            Log.Information("Shell Window was initialized");
            _navigationService = navigationService;
            _navigationService.Initialize(CurrentFrame);

            _navigationService.NavigateTo<StartPage>();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (YesNoDialog.Show(Translations.GetValue("AreYouSureToExit")) != MessageBoxResult.Yes)
            {
                e.Cancel = true;
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        private void CurrentFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            BackButton.Visibility = Visibility.Hidden;
        }

        private void CurrentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            var isStartPage = nameof(StartPage) == e.Content.GetType().Name;
            if (isStartPage || !_navigationService.CanGoBack)
            {
                BackButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                BackButton.Visibility = Visibility.Visible;
            }
        }

        private void BackButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var originatingType = CurrentFrame.Content.GetType();
            _navigationService.GoBack(originatingType.Name);
        }
    }
}
