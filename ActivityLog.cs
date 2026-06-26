using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CyberSecurityChatBotPart2
{
    public class ActivityLog
    {
        private List<string> entries;

        public ActivityLog()
        {
            entries = new List<string>();
        }

        public void AddEntry(string description)
        {
            string timeStamp = DateTime.Now.ToString("HH:mm:ss");
            entries.Add("[" + timeStamp + "] " + description);
        }

        /// <summary>
        /// Returns the most recent entries, newest first.
        /// </summary>
        public string GetRecentSummary(int count = 5)
        {
            if (entries.Count == 0)
            {
                return "I haven't logged any actions yet this session.";
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Here's a summary of recent actions:");

            List<string> recent = entries.Skip(Math.Max(0, entries.Count - count)).ToList();
            recent.Reverse();

            int number = 1;
            foreach (string entry in recent)
            {
                sb.AppendLine(number + ". " + entry);
                number++;
            }

            if (entries.Count > count)
            {
                sb.AppendLine("(Type 'show full log' to see all " + entries.Count + " actions.)");
            }

            return sb.ToString();
        }

        public string GetFullLog()
        {
            if (entries.Count == 0)
            {
                return "I haven't logged any actions yet this session.";
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Full activity log (" + entries.Count + " actions):");

            List<string> all = new List<string>(entries);
            all.Reverse();

            int number = 1;
            foreach (string entry in all)
            {
                sb.AppendLine(number + ". " + entry);
                number++;
            }

            return sb.ToString();
        }

        public int Count => entries.Count;
    }
}
