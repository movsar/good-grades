using Data.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
    public class Segment : IEntityBase, INotifyPropertyChanged
    {
        [Key] public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTimeOffset CreatedAt { get; } = DateTimeOffset.Now;
        public DateTimeOffset ModifiedAt { get; set; } = DateTimeOffset.Now;
        /******************************************************************/
        [Required]
        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }
        public string? Description { get; set; }

        public virtual IList<MatchingAssignment> MatchingAssignments { get; set; } = new List<MatchingAssignment>();
        public virtual IList<SelectingAssignment> SelectionAssignments { get; set; } = new List<SelectingAssignment>();
        public virtual IList<BuildingAssignment> BuildingAssignments { get; set; } = new List<BuildingAssignment>();
        public virtual IList<FillingAssignment> FillingAssignments { get; set; } = new List<FillingAssignment>();
        public virtual IList<TestingAssignment> TestingAssignments { get; set; } = new List<TestingAssignment>();
        public virtual IList<Material> Materials { get; set; } = new List<Material>();
        public int Order { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}