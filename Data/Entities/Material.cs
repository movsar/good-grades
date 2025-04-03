using Data.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    [Table("materials")]
    public class Material : IEntityBase, IMaterial
    {
        [Key] public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset ModifiedAt { get; set; } = DateTimeOffset.Now;
        /******************************************************************/
        public string Title { get; set; }
        public byte[]? PdfData { get; set; } = null;
        public byte[]? Audio { get; set; } = null;

        public string? PdfPath { get; set; } = null;
        public string? AudioPath { get; set; } = null;
        public int Order { get; set; }
    }
}
