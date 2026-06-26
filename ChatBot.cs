using Mysqlx.Crud;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace CyberSecurityChatBotPart2
{
    enum ConversationState
    {
        Normal,
        AwaitingReminderConfirmation,
        AwaitingReminderTimeframe,
        InQuiz
    }

    class ChatBot
    {
        private Dictionary<string, List<string>> keywordResponses;
        private Dictionary<string, string> memory;
        private Random random;

        public string LastTopic = "";

        // ---- Part 3 additions ----
        private Databasehelper db;
        private QuizEngine quiz;
        private IntentRecognizer intentRecognizer;
        private ActivityLog activityLog;

        private ConversationState state;
        private string pendingTaskTitle;
        private string pendingTaskDescription;
        private int pendingTaskId; // task row just created, in case a reminder follows

        public ChatBot()
        {
            random = new Random();
            memory = new Dictionary<string, string>();

            db = new Databasehelper();
            quiz = new QuizEngine();
            intentRecognizer = new IntentRecognizer();
            activityLog = new ActivityLog();

            state = ConversationState.Normal;

            keywordResponses = new Dictionary<string, List<string>>()
            {
                {
                    "password", new List<string>()
                    { "Use strong and unique passwords for every account.",
                      "Avoid using birthdays or names in passwords.",
                      "Enable two-factor authentication for extra security." }
                },
                {
                   "phishing", new List<string>()
                   { "Be careful of suspicious emails asking for personal information.",
                     "Never click unknown links from emails or SMS messages.",
                     "Scammers often pretend to be trusted companies." }
                },
                {
                    "privacy", new List<string>()
                    { "Always review your account privacy settings.",
                      "Avoid sharing personal information publicly online.",
                      "Use secure websites that begin with HTTPS." }
                },
                {
                    "scam", new List<string>()
                    { "Online scams often create urgency to trick victims.",
                      "Never share banking details with unknown people.",
                      "Verify company websites before making payments." }
                },
                {
                    "malware", new List<string>()
                    { "Malware is harmful software designed to damage or steal data.",
                      "Common types include viruses, worms, and trojans.",
                      "Install antivirus software to protect your device.",
                      "Avoid downloading files from unknown websites." }
                },
                {
                    "virus", new List<string>()
                    { "A computer virus spreads between devices and files.",
                      "Viruses can corrupt or delete important information.",
                      "Keep your antivirus software updated regularly.",
                      "Do not open suspicious email attachments." }
                },
                {
                    "hacker", new List<string>()
                    { "Hackers attempt to gain unauthorized access to systems.",
                      "Some hackers steal information or money online.",
                      "Use strong passwords to reduce hacking risks.",
                      "Enable two-factor authentication for better security." }
                },
                {
                    "firewall", new List<string>()
                    { "A firewall helps block unauthorized access to your network.",
                      "Firewalls monitor incoming and outgoing traffic.",
                      "They are important for both home and business security.",
                      "Always keep your firewall enabled." }
                },
                {
                    "vpn", new List<string>()
                    { "A VPN helps protect your privacy online.",
                      "VPN stands for Virtual Private Network.",
                      "It encrypts your internet connection.",
                      "VPNs are useful when using public Wi-Fi." }
                },
                {
                    "cyberbullying", new List<string>()
                    { "Cyberbullying happens through online platforms and social media.",
                      "Always report abusive online behaviour.",
                      "Avoid sharing harmful messages or content.",
                      "Talk to a trusted person if you experience cyberbullying." }
                },
                {
                    "identity theft", new List<string>()
                    { "Identity theft happens when someone steals personal information.",
                      "Protect your ID numbers and banking details.",
                      "Do not share sensitive information online.",
                      "Monitor your accounts regularly for suspicious activity." }
                },
                {
                    "data breach", new List<string>()
                    { "A data breach occurs when sensitive information is exposed.",
                      "Companies can lose customer information during breaches.",
                      "Use strong passwords to protect your accounts.",
                      "Change passwords immediately after a breach." }
                },
                {
                    "social engineering", new List<string>()
                    { "Social engineering tricks people into revealing information.",
                      "Attackers often pretend to be trusted individuals.",
                      "Never share passwords with anyone.",
                      "Be cautious of urgent requests for sensitive data." }
                },
                {
                    "ransomware", new List<string>()
                    { "Ransomware locks files until money is paid.",
                      "Never download suspicious attachments.",
                      "Keep backups of important files.",
                      "Update your software regularly to reduce risks." }
                }
            };
        }

        // Exposed so MainWindow can render the quiz / task list in dedicated
        // GUI controls if it wants to, instead of only plain text.
        public bool IsQuizActive => state == ConversationState.InQuiz;
        public QuizEngine Quiz => quiz;
        public ActivityLog Log => activityLog;
        public Databasehelper Database => db;

        public string GetResponse(string input)
        {
            string rawInput = input;
            input = input.ToLower();

            // EXIT COMMAND
            if (input == "bye" || input == "exit" || input == "quit" ||
                input.Contains("end chat") || input.Contains("goodbye") || input.Contains("close the app"))
            {
                activityLog.AddEntry("User ended the chat session.");
                return "EXIT_REQUESTED";
            }

            // STATE-DEPENDENT HANDLING FIRST

            if (state == ConversationState.InQuiz)
            {
                return HandleQuizAnswer(rawInput);
            }

            if (state == ConversationState.AwaitingReminderConfirmation)
            {
                return HandleReminderConfirmation(rawInput);
            }

            if (state == ConversationState.AwaitingReminderTimeframe)
            {
                return HandleReminderTimeframe(rawInput);
            }

            // -----------------------------------------------------------
            // NLP-STYLE INTENT DETECTION (Task 3)
            // -----------------------------------------------------------
            Intent intent = intentRecognizer.DetectIntent(input);

            switch (intent)
            {
                case Intent.AddTask:
                    return HandleAddTask(rawInput);

                case Intent.ViewTasks:
                    return HandleViewTasks();

                case Intent.CompleteTask:
                    return HandleCompleteTask(rawInput);

                case Intent.DeleteTask:
                    return HandleDeleteTask(rawInput);

                case Intent.StartQuiz:
                    return HandleStartQuiz();

                case Intent.ShowActivityLog:
                    activityLog.AddEntry("User requested activity log summary.");
                    return activityLog.GetRecentSummary();

                case Intent.ShowFullLog:
                    return activityLog.GetFullLog();
            }

            // -----------------------------------------------------------
            // MEMORY (from Part 2)
            // -----------------------------------------------------------
            if (input.Contains("my name is"))
            {
                string name = input.Replace("my name is", "").Trim();
                memory["name"] = name;
                return "Nice to meet you, " + name;
            }

            if (input.Contains("i like"))
            {
                string interest = input.Replace("i like", "").Trim();
                memory["interest"] = interest;
                return "Great! I'll remember that you're interested in " + interest + ".";
            }

            // SENTIMENT DETECTION
            if (input.Contains("worried"))
            {
                return "It's understandable to feel worried. Cybersecurity threats are common, but learning safety tips can help protect you.";
            }

            if (input.Contains("frustrated"))
            {
                return "I understand this can feel frustrating. Let's work through it together step by step.";
            }

            if (input.Contains("curious"))
            {
                return "Curiosity is great in cybersecurity! Learning more helps you stay protected online.";
            }

            // FOLLOW-UP FLOW
            if (input.Contains("tell me more") || input.Contains("another tip") || input.Contains("explain more"))
            {
                if (keywordResponses.ContainsKey(LastTopic))
                {
                    StringBuilder followUp = new StringBuilder();
                    followUp.AppendLine("Here is more information about " + LastTopic + ":");

                    foreach (string response in keywordResponses[LastTopic])
                    {
                        followUp.AppendLine("- " + response);
                    }

                    return followUp.ToString();
                }
            }

            // KEYWORD RECOGNITION
            foreach (var keyword in keywordResponses.Keys)
            {
                if (input.Contains(keyword))
                {
                    LastTopic = keyword;
                    StringBuilder responseBuilder = new StringBuilder();
                    responseBuilder.AppendLine("Here is what I know about " + keyword + ":");

                    foreach (string response in keywordResponses[keyword])
                    {
                        responseBuilder.AppendLine("- " + response);
                    }

                    return responseBuilder.ToString();
                }
            }

            // MEMORY RECALL
            if (memory.ContainsKey("interest"))
            {
                return "Since you're interested in " + memory["interest"] + ", remember to stay safe online.";
            }

            // DEFAULT RESPONSE
            return "I'm not sure I understand. Could you try rephrasing? You can also try things like 'add a task', 'start quiz', or 'show activity log'.";
        }

        // =====================================================================
        // TASK ASSISTANT (Task 1)
        // =====================================================================

        private string HandleAddTask(string rawInput)
        {
            string title = intentRecognizer.ExtractTaskTitle(rawInput);
            DateTime? reminder = intentRecognizer.ExtractReminderDate(rawInput);
            string description = "Cybersecurity task: " + title + ".";

            try
            {
                int newId = db.AddTask(title, description, reminder);
                pendingTaskId = newId;

                activityLog.AddEntry("Task added: '" + title + "'" +
                    (reminder.HasValue ? " (Reminder set for " + reminder.Value.ToString("dd MMM yyyy") + ")" : " (no reminder set)"));

                if (reminder.HasValue)
                {
                    return "Task added with the description \"" + description + "\" Reminder set for " +
                           reminder.Value.ToString("dd MMM yyyy") + ".";
                }
                else
                {
                    pendingTaskTitle = title;
                    state = ConversationState.AwaitingReminderConfirmation;
                    return "Task added with the description \"" + description + "\" Would you like a reminder?";
                }
            }
            catch (Exception ex)
            {
                return "I added the task to memory, but couldn't save it to the database. (" + ex.Message + ")";
            }
        }

        private string HandleReminderConfirmation(string rawInput)
        {
            string lower = rawInput.ToLower();

            if (lower.Contains("yes") || lower.Contains("sure") || lower.Contains("ok") || lower.StartsWith("y"))
            {
                state = ConversationState.AwaitingReminderTimeframe;
                return "Got it! When would you like to be reminded? (e.g. 'in 3 days', 'tomorrow', 'in a week')";
            }
            else
            {
                state = ConversationState.Normal;
                return "No problem, I won't set a reminder for that task. Let me know if you change your mind.";
            }
        }

        private string HandleReminderTimeframe(string rawInput)
        {
            DateTime? reminder = intentRecognizer.ExtractReminderDate(rawInput);
            state = ConversationState.Normal;

            if (!reminder.HasValue)
            {
                return "I didn't catch a timeframe there. Try something like 'remind me in 3 days'.";
            }

            try
            {
                db.SetReminder(pendingTaskId, reminder.Value);
                activityLog.AddEntry("Reminder set for '" + pendingTaskTitle + "' on " + reminder.Value.ToString("dd MMM yyyy") + ".");
                return "Got it! I'll remind you about \"" + pendingTaskTitle + "\" on " + reminder.Value.ToString("dd MMM yyyy") + ".";
            }
            catch (Exception ex)
            {
                return "I couldn't save that reminder to the database. (" + ex.Message + ")";
            }
        }

        private string HandleViewTasks()
        {
            try
            {
                List<TaskManager> tasks = db.GetAllTasks();

                if (tasks.Count == 0)
                {
                    return "You don't have any cybersecurity tasks yet. Try saying 'add a task to enable 2FA'.";
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Here are your tasks:");

                foreach (TaskManager t in tasks)
                {
                    sb.AppendLine("#" + t.TaskId + " " + t.ToString());
                }

                activityLog.AddEntry("User viewed their task list (" + tasks.Count + " tasks).");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return "I couldn't load your tasks from the database. (" + ex.Message + ")";
            }
        }

        private string HandleCompleteTask(string rawInput)
        {
            int? taskId = intentRecognizer.ExtractTaskNumber(rawInput);

            if (!taskId.HasValue)
            {
                return "Which task would you like to mark as completed? Please include the task number, e.g. 'mark task 3 as done'.";
            }

            try
            {
                bool success = db.MarkTaskCompleted(taskId.Value);
                if (success)
                {
                    activityLog.AddEntry("Task #" + taskId.Value + " marked as completed.");
                    return "Nice work! Task #" + taskId.Value + " has been marked as completed.";
                }
                else
                {
                    return "I couldn't find a task with that number.";
                }
            }
            catch (Exception ex)
            {
                return "I couldn't update that task in the database. (" + ex.Message + ")";
            }
        }

        private string HandleDeleteTask(string rawInput)
        {
            int? taskId = intentRecognizer.ExtractTaskNumber(rawInput);

            if (!taskId.HasValue)
            {
                return "Which task would you like to delete? Please include the task number, e.g. 'delete task 2'.";
            }

            try
            {
                bool success = db.DeleteTask(taskId.Value);
                if (success)
                {
                    activityLog.AddEntry("Task #" + taskId.Value + " deleted.");
                    return "Task #" + taskId.Value + " has been deleted.";
                }
                else
                {
                    return "I couldn't find a task with that number.";
                }
            }
            catch (Exception ex)
            {
                return "I couldn't delete that task in the database. (" + ex.Message + ")";
            }
        }

        // =====================================================================
        // QUIZ MINI-GAME (Task 2)
        // =====================================================================

        private string HandleStartQuiz()
        {
            quiz.StartQuiz();
            state = ConversationState.InQuiz;
            activityLog.AddEntry("Quiz started - " + quiz.TotalQuestions + " questions.");

            return "Let's test your cybersecurity knowledge! " + FormatCurrentQuestion();
        }

        private string FormatCurrentQuestion()
        {
            QuizQuestion q = quiz.GetCurrentQuestion();
            if (q == null) return "";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Question " + quiz.CurrentQuestionNumber + " of " + quiz.TotalQuestions + ":");
            sb.AppendLine(q.QuestionText);

            if (q.Type == QuestionType.MultipleChoice)
            {
                foreach (string option in q.Options)
                {
                    sb.AppendLine(option);
                }
                sb.Append("(Answer with A, B, C, or D)");
            }
            else
            {
                sb.Append("(Answer True or False)");
            }

            return sb.ToString();
        }

        private string HandleQuizAnswer(string rawInput)
        {
            string feedback = quiz.SubmitAnswer(rawInput);

            if (quiz.HasNextQuestion())
            {
                return feedback + "\n\n" + FormatCurrentQuestion();
            }
            else
            {
                string finalResult = quiz.EndQuiz();
                state = ConversationState.Normal;
                activityLog.AddEntry("Quiz completed - scored " + quiz.Score + "/" + quiz.TotalQuestions + ".");
                return feedback + "\n\n" + finalResult;
            }
        }
    }
}