using Shared;
using System.Windows;

namespace GGPlayer
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();

            tbMain.Text = Constants.About;
        }
    }
}
