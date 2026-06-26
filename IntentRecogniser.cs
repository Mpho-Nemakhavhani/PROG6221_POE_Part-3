using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CyberSecurityChatBotPart2
{
    public enum Intent
    {
        AddTask,
        SetReminder,
        ViewTasks,
        CompleteTask,
        DeleteTask,
        StartQuiz,
        ShowActivityLog,
        ShowFullLog,
        Unknown
    }

    /// <summary>
    /// Simulates basic NLP by detecting the user's *intent* even when they
    /// phrase a request in different ways, using keyword and pattern
    /// matching (string.Contains + simple regex) rather than a strict
    /// command syntax.
    ///
    /// e.g. "remind me to update my password tomorrow",
    ///      "can you set a reminder for reviewing my privacy settings",
    ///      "I want to add a task to enable 2FA"
    /// should all be recognised correctly.
    /// </summary>
    public class IntentRecognizer
    {
        // Phrases that imply the user wants to create a task/reminder.
        private static readonly string[] AddTaskPhrases =
        {
            "add a task", "add task", "create a task", "new task",
            "i need to", "i should", "i have to", "remind me to",
            "set a reminder", "set reminder", "add a reminder"
        };

        private static readonly string[] ViewTasksPhrases =
        {
            "show my tasks", "view tasks", "view my tasks", "list tasks",
            "list my tasks", "what tasks", "show tasks", "my to-do",
            "my todo", "what do i need to do"
        };

        private static readonly string[] CompleteTaskPhrases =
        {
            "mark as done", "mark as complete", "mark complete",
            "i finished", "i completed", "i'm done with", "completed task"
        };

        private static readonly string[] DeleteTaskPhrases =
        {
            "delete task", "remove task", "delete the task", "cancel task"
        };

        private static readonly string[] QuizPhrases =
        {
            "quiz", "play a game", "start the game", "test my knowledge",
            "start quiz", "mini game", "minigame"
        };

        private static readonly string[] ActivityLogPhrases =
        {
            "activity log", "what have you done", "show log",
            "show activity", "recent actions", "what did you do"
        };

        private static readonly string[] FullLogPhrases =
        {
            "show full log", "full activity log", "show more", "full history"
        };

        /// <summary>
        /// Returns the detected intent for a free-form piece of user input.
        /// </summary>
        public Intent DetectIntent(string input)
        {
            string lower = input.ToLower().Trim();

            if (ContainsAny(lower, FullLogPhrases)) return Intent.ShowFullLog;
            if (ContainsAny(lower, ActivityLogPhrases)) return Intent.ShowActivityLog;
            if (ContainsAny(lower, QuizPhrases)) return Intent.StartQuiz;
            if (ContainsAny(lower, DeleteTaskPhrases)) return Intent.DeleteTask;
            if (ContainsAny(lower, CompleteTaskPhrases)) return Intent.CompleteTask;
            if (ContainsAny(lower, ViewTasksPhrases)) return Intent.ViewTasks;

            // "remind me ... in X days" implies both add + reminder; treat as AddTask,
            // the calling code extracts the reminder timeframe separately.
            if (ContainsAny(lower, AddTaskPhrases)) return Intent.AddTask;

            return Intent.Unknown;
        }

        private bool ContainsAny(string input, string[] phrases)
        {
            foreach (string phrase in phrases)
            {
                if (input.Contains(phrase)) return true;
            }
            return false;
        }

        /// <summary>
        /// Strips common command phrases out of the input so what remains
        /// can be used as the task title, e.g.
        /// "remind me to update my password" -> "update my password"
        /// </summary>
        public string ExtractTaskTitle(string input)
        {
            string cleaned = input.ToLower();

            string[] phrasesToStrip =
            {
                "can you ", "could you ", "please ",
                "add a task to ", "add a task ", "add task to ", "add task ",
                "create a task to ", "create a task ", "new task ",
                "remind me to ", "set a reminder to ", "set reminder to ",
                "i need to ", "i should ", "i have to ", "i want to "
            };

            foreach (string phrase in phrasesToStrip)
            {
                cleaned = cleaned.Replace(phrase, "");
            }

            // Remove trailing timeframe phrases like "tomorrow" / "in 3 days"
            cleaned = Regex.Replace(cleaned, @"\bin \d+ days?\b", "", RegexOptions.IgnoreCase);
            cleaned = Regex.Replace(cleaned, @"\btomorrow\b", "", RegexOptions.IgnoreCase);
            cleaned = Regex.Replace(cleaned, @"\btoday\b", "", RegexOptions.IgnoreCase);

            cleaned = cleaned.Trim().TrimEnd('.', '!', '?');

            if (cleaned.Length > 0)
            {
                cleaned = char.ToUpper(cleaned[0]) + cleaned.Substring(1);
            }

            return string.IsNullOrWhiteSpace(cleaned) ? "New cybersecurity task" : cleaned;
        }

        /// <summary>
        /// Looks for a relative timeframe ("in 3 days", "tomorrow", "in a week")
        /// or absolute date hints in free text and converts it to a DateTime.
        /// Returns null if no reminder timing was detected.
        /// </summary>
        public DateTime? ExtractReminderDate(string input)
        {
            string lower = input.ToLower();

            if (lower.Contains("tomorrow"))
            {
                return DateTime.Now.AddDays(1);
            }

            if (lower.Contains("next week") || lower.Contains("in a week"))
            {
                return DateTime.Now.AddDays(7);
            }

            Match match = Regex.Match(lower, @"in (\d+) days?");
            if (match.Success)
            {
                int days = int.Parse(match.Groups[1].Value);
                return DateTime.Now.AddDays(days);
            }

            match = Regex.Match(lower, @"in (\d+) weeks?");
            if (match.Success)
            {
                int weeks = int.Parse(match.Groups[1].Value);
                return DateTime.Now.AddDays(weeks * 7);
            }

            return null;
        }

        /// <summary>
        /// Tries to find a task number/id mentioned in input like
        /// "mark task 3 as done" or "delete task 2".
        /// </summary>
        public int? ExtractTaskNumber(string input)
        {
            Match match = Regex.Match(input, @"\d+");
            if (match.Success)
            {
                return int.Parse(match.Value);
            }
            return null;
        }
    }
}
