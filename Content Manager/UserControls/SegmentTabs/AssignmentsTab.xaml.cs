﻿using Content_Manager.Stores;
using Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Content_Manager.UserControls.SegmentTabs
{
    public partial class AssignmentsTab : UserControl
    {
        private ContentStore ContentStore => App.AppHost!.Services.GetRequiredService<ContentStore>();
        public AssignmentsTab()
        {
            InitializeComponent();
            DataContext = this;

            ContentStore.ItemAdded += ContentStore_ItemChanged;
            ContentStore.ItemDeleted += ContentStore_ItemChanged;
            ContentStore.ItemUpdated += ContentStore_ItemChanged;

            RedrawUi();
        }
        public void RedrawUi()
        {
            spTaskAssignmentControls.Children.Clear();

            if (ContentStore.SelectedSegment == null)
            {
                return;
            }

            List<IAssignment> allTasks = ContentStore.SelectedSegment!.MatchingAssignments.Cast<IAssignment>().ToList();
            allTasks.AddRange(ContentStore.SelectedSegment!.FillingAssignments);
            allTasks.AddRange(ContentStore.SelectedSegment!.BuildingAssignments);
            allTasks.AddRange(ContentStore.SelectedSegment!.TestingAssignments);
            allTasks.AddRange(ContentStore.SelectedSegment!.SelectionAssignments);

            foreach (var material in allTasks.OrderBy(t => t.CreatedAt))
            {
                spTaskAssignmentControls.Children.Add(new AssignmentControl(material));
            }

            var newMaterial = new AssignmentControl();

            spTaskAssignmentControls.Children.Add(newMaterial);
        }

        private void ContentStore_ItemChanged(IEntityBase entity)
        {
            RedrawUi();
        }
    }
}
