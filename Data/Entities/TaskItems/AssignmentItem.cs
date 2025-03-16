using Data.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities.TaskItems
{
    [Table("assignment_items")]
    public class AssignmentItem : IEntityBase
    {
        [Key] public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset ModifiedAt { get; set; } = DateTimeOffset.Now;
        /******************************************************************/
        public string Text { get; set; }
        public byte[]? Image { get; set; } = null;
        public string? ImagePath { get; set; } = null;
        // For questions etc
        public bool IsChecked { get; set; } = false;

        public int Order { get; set; }
    }
}
