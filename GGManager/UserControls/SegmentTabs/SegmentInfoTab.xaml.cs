using GGManager.Services;
using GGManager.Stores;
using Microsoft.Extensions.DependencyInjection;
using Shared.Services;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace GGManager.UserControls.SegmentTabs
{
    public partial class SegmentInfoTab : UserControl
    {
        #region Properties
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(SegmentInfoTab), new PropertyMetadata(string.Empty));

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(SegmentInfoTab), new PropertyMetadata(string.Empty));
        #endregion

        private readonly ContentStore _contentStore = App.AppHost!.Services.GetRequiredService<ContentStore>();

        public SegmentInfoTab()
        {
            InitializeComponent();
            DataContext = this;
            try
            {
                //загрузка описания сегмента через rtf
                RtfService.LoadRtfFromText(rtbDescription, _contentStore.SelectedSegment!.Description);
            }
            catch
            {
                Description = null;
            }
            Title = _contentStore.SelectedSegment!.Title;
        }

        //сохранение изменений в сегменте
        private void Save()
        {
            _contentStore.SelectedSegment!.Title = Title;
            _contentStore.SelectedSegment!.Description = RtfService.GetRtfDescriptionAsText(rtbDescription);
            _contentStore.DbContext.SaveChanges();
            _contentStore.RaiseSegmentUpdatedEvent(_contentStore.SelectedSegment);
        }

        #region Event Handler
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }
        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                Save();
            }
        }
        #endregion


    }
}
