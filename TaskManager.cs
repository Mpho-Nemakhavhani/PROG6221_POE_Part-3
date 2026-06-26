using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace CyberSecurityChatBotPart2
{
    public class TaskManager
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? ReminderDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }

        public override string ToString()
        {
            string status = IsCompleted ? "[Done]" : "[Pending]";
            string reminderText = ReminderDate.HasValue
                ? " | Reminder: " + ReminderDate.Value.ToString("dd MMM yyyy")
                : "";

            return status + " " + Title + " - " + Description + reminderText;
        }
    }
}
