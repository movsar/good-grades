﻿using GGPlayer.Pages;
using Shared;
using System.Windows;
using System.Windows.Navigation;

namespace GGPlayer
{
    public partial class ShellWindow : Window
    {
        public ShellWindow()
        {
            InitializeComponent();

            var _mainPage = new MainPage(this);
            CurrentFrame.Navigate(_mainPage);
        }
        private void GoBack(object sender, RoutedEventArgs e)
        {
            if (CurrentFrame.CanGoBack)
            {
                CurrentFrame.GoBack();
            }
        }

        private void GoForward(object sender, RoutedEventArgs e)
        {
            if (CurrentFrame.CanGoForward)
            {
                CurrentFrame.GoForward();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show(String.Format(Translations.GetValue("AreYouSureToExit")), "Good Grades", MessageBoxButton.YesNo, MessageBoxImage.Information) != MessageBoxResult.Yes)
            {
                e.Cancel = true;
            }
        }

        private void CurrentFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            BackButton.Visibility = Visibility.Hidden;
        }

        private void CurrentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            BackButton.Visibility = CurrentFrame.CanGoBack ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
