using Serilog;
using System.Windows;

namespace GGManager.Windows
{
    public partial class DbInfoWindow : Window
    {
        public DbInfoWindow()
        {
            InitializeComponent();
            ucDbInformation.Saved += DbInformation_Saved;
            Log.Information("Info about database opened");
        }

        private void DbInformation_Saved()
        {
            Close();
        }
    }
}
