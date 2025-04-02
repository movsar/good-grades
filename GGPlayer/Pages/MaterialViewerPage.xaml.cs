using Data;
using Data.Entities;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Shared.Controls;
using System.Windows.Controls;

namespace GGPlayer.Pages
{
    public partial class MaterialViewerPage : Page
    {
        public MaterialViewerPage()
        {
            InitializeComponent();
            DataContext = this;
            Log.Information("Material View page was initialized");
        }

        public void Initialize(Material material)
        {
            Title = material.Title;

            var materialControl = App.AppHost!.Services.GetRequiredService<MaterialViewerControl>();
            var pdfData = Storage.ReadDbAsset(material.PdfPath);
            var audioData = Storage.ReadDbAsset(material.AudioPath);

            materialControl.Initialize(material.Title, pdfData, audioData);

            ucRoot.Content = materialControl;
            InitializeComponent();
        }
    }
}
