using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public class TaskFilteringTests
    {
        public class TaskItem
        {
            public int Id { get; set; }
            public int? AssignedToUserId { get; set; }
            public string TaskName { get; set; }
        }

        [Fact]
        public void FilterWorkerTasks_ReturnsOnlyAssigned()
        {
            var tasks = new List<TaskItem>
            {
                new TaskItem { Id = 1, AssignedToUserId = 1, TaskName = "Task 1" },
                new TaskItem { Id = 2, AssignedToUserId = 2, TaskName = "Task 2" },
                new TaskItem { Id = 3, AssignedToUserId = 1, TaskName = "Task 3" }
            };
            var workerId = 1;

            var filtered = tasks.Where(t => t.AssignedToUserId == workerId).ToList();

            Assert.Equal(2, filtered.Count);
            Assert.All(filtered, t => Assert.Equal(workerId, t.AssignedToUserId));
        }

        [Fact]
        public void FilterAdminTasks_ReturnsAll()
        {
            var tasks = new List<TaskItem>
            {
                new TaskItem { Id = 1, AssignedToUserId = 1 },
                new TaskItem { Id = 2, AssignedToUserId = null },
                new TaskItem { Id = 3, AssignedToUserId = 3 }
            };

            var filtered = tasks.ToList();

            Assert.Equal(3, filtered.Count);
        }
    }
}