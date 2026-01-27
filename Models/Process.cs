namespace TaskManagerApp.Models
{
    public class Process
    {
        public int Id { get; set; }
        public int OwnerUserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string Status { get; set; } = "Active";
        public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}