using GGManager.Stores;
using Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System;
using System.Diagnostics;

namespace GGManager.UserControls.SegmentTabs
{
    public partial class AssignmentsTab : UserControl
    {
        private ContentStore ContentStore => App.AppHost!.Services.GetRequiredService<ContentStore>();
        public AssignmentsTab()
        {
            InitializeComponent();
            DataContext = this;
            //подписка на события изменений в ContentStore
            ContentStore.ItemAdded += ContentStore_ItemChanged;
            ContentStore.ItemDeleted += ContentStore_ItemChanged;
            ContentStore.ItemUpdated += ContentStore_ItemChanged;

            RedrawUi();
        }
        //перерисовка интерфейса
        public void RedrawUi()
        {
            spTaskAssignmentControls.Children.Clear();

            if (ContentStore.SelectedSegment == null)
            {
                return;
            }
            //получение заданий выбранного сегмента
            List<IAssignment> allTasks = ContentStore.SelectedSegment!.MatchingAssignments.Cast<IAssignment>().ToList();
            allTasks.AddRange(ContentStore.SelectedSegment!.FillingAssignments);
            allTasks.AddRange(ContentStore.SelectedSegment!.BuildingAssignments);
            allTasks.AddRange(ContentStore.SelectedSegment!.TestingAssignments);
            allTasks.AddRange(ContentStore.SelectedSegment!.SelectionAssignments);

            // Добавляем задания в интерфейс
            foreach (var material in allTasks.OrderBy(t => t.Order).ToList()) 
            {
                spTaskAssignmentControls.Children.Add(new AssignmentControl(material));
            }

            //создание пустого поля для нового задания
            var newMaterial = new AssignmentControl();
            
            spTaskAssignmentControls.Children.Add(newMaterial);
        }

        private void ContentStore_ItemChanged(IEntityBase entity)
        {
            RedrawUi();
        }
    }
}
