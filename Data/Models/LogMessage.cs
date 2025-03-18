using System.ComponentModel.DataAnnotations;

namespace Data
{
    public class LogMessage
    {
        [Key]
        public long Id { get; set; }

        public string? StackTrace { get; set; }
        public string Message { get; set; } = null!;
        public int Level { get; set; } = (int)LogLevel.Information;

        public string WindowsVersion { get; set; } = null!;
        public string SystemDetails { get; set; } = null!;
        public string ProgramVersion { get; set; } = null!;
        public string ProgramName { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
    public enum LogLevel
    {
        Debug = 0,
        Information = 1,
        Warning = 2,
        Error = 3
    }
}
