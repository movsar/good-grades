using GGManager.Stores;
using Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using System.Linq;

namespace GGManager.UserControls.SegmentTabs
{
    public partial class MaterialsTab : UserControl
    {
        private ContentStore ContentStore => App.AppHost!.Services.GetRequiredService<ContentStore>();

        public MaterialsTab()
        {
            InitializeComponent();
            DataContext = this;
            //подписка на события изменений в contentStore
            ContentStore.ItemAdded += ContentStore_ItemChanged;
            ContentStore.ItemDeleted += ContentStore_ItemChanged;
            ContentStore.ItemUpdated += ContentStore_ItemChanged;

            RedrawUi();
        }
        //перерисовка интерфейса
        public void RedrawUi()
        {
            spListeningControls.Children.Clear();

            if (ContentStore.SelectedSegment == null) return;

            foreach (var material in ContentStore.SelectedSegment.Materials.OrderBy(m => m.Order))
            {
                spListeningControls.Children.Add(new MaterialControl(material));
            }

            spListeningControls.Children.Add(new MaterialControl());
        }

        private void ContentStore_ItemChanged(IEntityBase entity)
        {
            RedrawUi();
        }
    }
}
