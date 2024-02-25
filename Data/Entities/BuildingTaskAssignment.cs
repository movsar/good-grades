﻿using Data.Entities.TaskItems;
using Data.Interfaces;

using Realms;

namespace Data.Entities
{
    public class BuildingTaskAssignment : RealmObject, IAssignment, IMaterial
    {
        [Required] public string Title { get; set; }
        [Required][PrimaryKey] public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset ModifiedAt { get; set; } = DateTimeOffset.Now;
        /******************************************************************/
        public IList<AssignmentItem> Items { get; }
        public bool IsContentSet => Items.Count() > 0;

    }
}
