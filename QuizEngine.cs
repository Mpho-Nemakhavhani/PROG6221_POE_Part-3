using System;
using System.Collections.Generic;
using System.Text;

namespace CyberSecurityChatBotPart2
{
    public enum QuestionType
    {
        MultipleChoice,
        TrueFalse
    }

    public class QuizQuestion
    {
        public string QuestionText 
        { 
            get; set; 
        }
        public QuestionType Type 
        {
            get; set; 
        }
        public List<string> Options  // null/empty for True/False
        {
            get; set; 
        }   
        public string CorrectAnswer   // "A"/"B"/"C"/"D" or "True"/"False"
        {
            get; set; 
        }    
        public string Explanation 
        {
            get; set; 
        }
    }

    // Drives the cybersecurity quiz mini-game: holds the question bank, tracks current position and score, and grades answers.

    public class QuizEngine
    {
        private List<QuizQuestion> questions;
        private int currentIndex;
        private int score;
        private Random random;

        public bool IsActive 
        {
            get; private set; 
        }

        public QuizEngine()
        {
            random = new Random();
            questions = BuildQuestionBank();
            IsActive = false;
        }

        private List<QuizQuestion> BuildQuestionBank()
        {
            return new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    QuestionText = "What should you do if you receive an email asking for your password?",
                    Type = QuestionType.MultipleChoice,
                    Options = new List<string> { "A) Reply with your password", "B) Delete the email", "C) Report the email as phishing", "D) Ignore it" },
                    CorrectAnswer = "C",
                    Explanation = "Reporting phishing emails helps your provider block future scams and protects others too."
                },

                new QuizQuestion
                {
                    QuestionText = "Using the same password across multiple accounts is safe as long as it's a strong password.",
                    Type = QuestionType.TrueFalse,
                    CorrectAnswer = "False",
                    Explanation = "Even a strong password becomes risky if reused — one breach can expose all your accounts."
                },

                new QuizQuestion
                {
                    QuestionText = "Which of these is the safest way to browse on public Wi-Fi?",
                    Type = QuestionType.MultipleChoice,
                    Options = new List<string> { "A) Log into your bank account directly", "B) Use a VPN", "C) Disable your antivirus", "D) Share your screen" },
                    CorrectAnswer = "B",
                    Explanation = "A VPN encrypts your traffic, making it much harder for others on the same network to intercept your data."
                },

                new QuizQuestion
                {
                    QuestionText = "Two-factor authentication (2FA) adds an extra layer of security beyond just a password.",
                    Type = QuestionType.TrueFalse,
                    CorrectAnswer = "True",
                    Explanation = "2FA requires a second proof of identity (like a code on your phone), so a stolen password alone isn't enough."
                },

                new QuizQuestion
                {
                    QuestionText = "What is 'phishing'?",
                    Type = QuestionType.MultipleChoice,
                    Options = new List<string> { "A) A type of antivirus software", "B) A method of encrypting data", "C) A scam to trick you into revealing information", "D) A firewall setting" },
                    CorrectAnswer = "C",
                    Explanation = "Phishing scams impersonate trusted sources to trick people into giving up passwords or personal data."
                },

                new QuizQuestion
                {
                    QuestionText = "It's safe to click links in text messages from unknown numbers if the message looks official.",
                    Type = QuestionType.TrueFalse,
                    CorrectAnswer = "False",
                    Explanation = "Scammers often spoof official-looking messages. Always verify through an official app or website instead of clicking the link."
                },

                new QuizQuestion
                {
                    QuestionText = "Which of these is a sign of a strong password?",
                    Type = QuestionType.MultipleChoice,
                    Options = new List<string> { "A) Your pet's name", "B) Your birth year", "C) A long mix of letters, numbers, and symbols", "D) The word 'password'" },
                    CorrectAnswer = "C",
                    Explanation = "Long, random combinations of characters are much harder for attackers to guess or crack than personal details."
                },

                new QuizQuestion
                {
                    QuestionText = "Social engineering attacks rely on tricking people rather than exploiting software bugs.",
                    Type = QuestionType.TrueFalse,
                    CorrectAnswer = "True",
                    Explanation = "Social engineering manipulates human trust and emotion, not technical vulnerabilities."
                },

                new QuizQuestion
                {
                    QuestionText = "What does a firewall primarily do?",
                    Type = QuestionType.MultipleChoice,
                    Options = new List<string> { "A) Speeds up your internet", "B) Monitors and blocks unauthorized network traffic", "C) Scans for viruses on USB drives", "D) Backs up your files" },
                    CorrectAnswer = "B",
                    Explanation = "Firewalls act as a gatekeeper between your device/network and the outside world, filtering suspicious traffic."
                },

                new QuizQuestion
                {
                    QuestionText = "Ransomware encrypts your files and demands payment to restore access.",
                    Type = QuestionType.TrueFalse,
                    CorrectAnswer = "True",
                    Explanation = "Ransomware locks victims out of their own data, and paying doesn't guarantee recovery — backups are the best defence."
                },

                new QuizQuestion
                {
                    QuestionText = "Which of these details is generally safest to share publicly on social media?",
                    Type = QuestionType.MultipleChoice,
                    Options = new List<string> { "A) Your home address", "B) Your favourite hobby", "C) Your ID number", "D) Your banking PIN" },
                    CorrectAnswer = "B",
                    Explanation = "Harmless interests are fine to share, but personal identifiers like addresses, ID numbers, or PINs should stay private."
                },

                new QuizQuestion
                {
                    QuestionText = "Antivirus software alone is enough to guarantee your device can never be hacked.",
                    Type = QuestionType.TrueFalse,
                    CorrectAnswer = "False",
                    Explanation = "Antivirus helps a lot, but good security needs multiple layers: updates, strong passwords, 2FA, and safe browsing habits."
                },

                new QuizQuestion
                {
                    QuestionText = "What's the best first step after discovering your data was part of a company's data breach?",
                    Type = QuestionType.MultipleChoice,
                    Options = new List<string> { "A) Ignore it, it's not your fault", "B) Change your passwords for affected accounts", "C) Delete your email account", "D) Share the news on social media" },
                    CorrectAnswer = "B",
                    Explanation = "Changing passwords (and enabling 2FA where possible) quickly limits how much an attacker can do with the leaked data."
                }
            };
        }

        public void StartQuiz()
        {
            IsActive = true;
            currentIndex = 0;
            score = 0;
            ShuffleQuestions();
        }

        private void ShuffleQuestions()
        {
            for (int i = questions.Count - 1; i > 0; i--)
            {
                int swapIndex = random.Next(i + 1);
                QuizQuestion temp = questions[i];
                questions[i] = questions[swapIndex];
                questions[swapIndex] = temp;
            }
        }

        public bool HasNextQuestion()
        {
            return currentIndex < questions.Count;
        }

        public QuizQuestion GetCurrentQuestion()
        {
            if (!HasNextQuestion()) return null;

            return questions[currentIndex];
        }

        public int CurrentQuestionNumber => currentIndex + 1;
        public int TotalQuestions => questions.Count;
        public int Score => score;

        // Grades the user's answer against the current question, advances to the next question, and returns feedback text.

        public string SubmitAnswer(string userAnswer)
        {
            QuizQuestion q = GetCurrentQuestion();
            if (q == null) return "The quiz has already ended.";

            string normalizedAnswer = NormalizeAnswer(userAnswer, q);
            bool isCorrect = string.Equals(normalizedAnswer, q.CorrectAnswer, StringComparison.OrdinalIgnoreCase);

            string feedback;
            if (isCorrect)
            {
                score++;
                feedback = "Correct! " + q.Explanation;
            }
            else
            {
                feedback = "Not quite. The correct answer was " + q.CorrectAnswer + ". " + q.Explanation;
            }

            currentIndex++;
            return feedback;
        }

        private string NormalizeAnswer(string raw, QuizQuestion q)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";
            string trimmed = raw.Trim();

            if (q.Type == QuestionType.TrueFalse)
            {
                string lower = trimmed.ToLower();
                if (lower.StartsWith("t")) return "True";
                if (lower.StartsWith("f")) return "False";
                return trimmed;
            }
            else
            {
                // Accept "A", "a", "A)", "a)" etc.
                string firstChar = trimmed.Substring(0, 1).ToUpper();
                return firstChar;
            }
        }

        public string EndQuiz()
        {
            IsActive = false;
            string performance;

            double percentage = (double)score / questions.Count * 100;

            if (percentage >= 80)
                performance = "Great job! You're a cybersecurity pro!";
            else if (percentage >= 50)
                performance = "Good effort! A bit more practice and you'll be a pro.";
            else
                performance = "Keep learning to stay safe online! Try the quiz again to improve your score.";

            return "Quiz complete! You scored " + score + " out of " + questions.Count + ".\n" + performance;
        }
    }
}
