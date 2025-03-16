using Data.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    [Table("db_metas")]
    public class DbMeta : IEntityBase
    {
        [Key] public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset CreatedAt { get; } = DateTimeOffset.Now;
        public DateTimeOffset ModifiedAt { get; set; } = DateTimeOffset.Now;
        /******************************************************************/
        public byte[]? BackgroundImage { get; set; } = null;
        public string? BackgroundImagePath { get; set; } = null;
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? AppVersion { get; set; }
        public int DbVersion { get; set; } = 1;
    }
}