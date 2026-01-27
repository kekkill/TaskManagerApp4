namespace TaskManagerApp.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public int ProcessId { get; set; }
        public int Priority { get; set; } = 2;
        public int? AssignedToUserId { get; set; }
        public User? AssignedToUser { get; set; } = null!;
        public Process Process { get; set; } = null!;
        public string TaskName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}