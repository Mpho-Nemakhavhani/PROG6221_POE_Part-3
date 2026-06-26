using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace CyberSecurityChatBotPart2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        // Declare chatbot object
        private ChatBot bot;

        // Store user's name
        private string userName = "";

        // Check if name was entered
        private bool nameCaptured = false;

        public MainWindow()
        {
            InitializeComponent();

            bot = new ChatBot();

            Voice_Greeting.PlayGreeting();

            ChatDisplay.Items.Add("Bot: Hello! I'm your Cybersecurity Awareness Chat Bot.");
            ChatDisplay.Items.Add("Bot: Please enter your name?");

            // Quick sanity check that the database is reachable. If not,
            // the chatbot's text features still work; only the Task
            // Assistant feature needs the DB.
            string dbError;
            if (!bot.Database.TestConnection(out dbError))
            {
                ChatDisplay.Items.Add("Bot: (Note: I couldn't connect to the task database. " +
                    "The Task Assistant feature won't work until the connection is fixed. Error: " + dbError + ")");
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessUserMessage();
        }

        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessUserMessage();
            }
        }

        private void ProcessUserMessage()
        {
            string userMessage = UserInput.Text;

            if (!string.IsNullOrWhiteSpace(userMessage))
            {
                // Display first message using the user's name
                if (!nameCaptured)
                {
                    userName = userMessage;
                    nameCaptured = true;

                    ChatDisplay.Items.Add("Bot: Nice to meet you, " + userName + "!");
                    ChatDisplay.Items.Add("Bot: How can I help you with cybersecurity today? " +
                        "You can chat with me, add tasks, start the quiz, or ask to see the activity log.");
                }
                else
                {
                    // Use stored username instead of "You"
                    ChatDisplay.Items.Add(userName + ": " + userMessage);
                    string response = bot.GetResponse(userMessage);

                    if (response == "EXIT_REQUESTED")
                    {
                        ChatDisplay.Items.Add("Bot: Goodbye, " + userName + "! Stay safe online.");
                        UserInput.Clear();

                        // Give the user a moment to see the goodbye message before closing
                        var closeTimer = new System.Windows.Threading.DispatcherTimer();
                        closeTimer.Interval = TimeSpan.FromSeconds(1.5);
                        closeTimer.Tick += (s, args) =>
                        {
                            closeTimer.Stop();
                            this.Close();
                        };
                        closeTimer.Start();
                        return;
                    }

                    AddBotResponse(response);
                }
            }

            UserInput.Clear();
            ScrollToBottom();
        }

        /// <summary>
        /// Multi-line bot responses (task lists, quiz questions, activity
        /// logs) are split so each line appears as its own ListBox row,
        /// which keeps the chat area readable.
        /// </summary>
        private void AddBotResponse(string response)
        {
            string[] lines = response.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            bool firstLine = true;
            foreach (string line in lines)
            {
                ChatDisplay.Items.Add(firstLine ? "Bot: " + line : "      " + line);
                firstLine = false;
            }
        }

        private void ScrollToBottom()
        {
            if (ChatDisplay.Items.Count > 0)
            {
                ChatDisplay.ScrollIntoView(ChatDisplay.Items[ChatDisplay.Items.Count - 1]);
            }
        }

        // ---------------------------------------------------------------
        // Part 3 feature buttons. These act as GUI shortcuts for the same
        // intents the NLP layer recognises from typed text, so a user can
        // either click a button or type naturally ("start quiz" / "show
        // activity log" / "view my tasks").
        // ---------------------------------------------------------------

        private void ViewTasksButton_Click(object sender, RoutedEventArgs e)
        {
            if (!nameCaptured) return;
            ChatDisplay.Items.Add(userName + ": View Tasks");
            AddBotResponse(bot.GetResponse("show my tasks"));
            ScrollToBottom();
        }

        private void StartQuizButton_Click(object sender, RoutedEventArgs e)
        {
            if (!nameCaptured) return;
            ChatDisplay.Items.Add(userName + ": Start Quiz");
            AddBotResponse(bot.GetResponse("start quiz"));
            ScrollToBottom();
        }

        private void ActivityLogButton_Click(object sender, RoutedEventArgs e)
        {
            if (!nameCaptured) return;
            ChatDisplay.Items.Add(userName + ": Show Activity Log");
            AddBotResponse(bot.GetResponse("show activity log"));
            ScrollToBottom();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ChatDisplay.Items.Clear();

            // Reset chatbot
            bot = new ChatBot();
            userName = "";
            nameCaptured = false;

            ChatDisplay.Items.Add("Bot: Hello! I'm your Cybersecurity Awareness Chat Bot.");
            ChatDisplay.Items.Add("Bot: Please enter your name?");
        }
    }
}