using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace CyberSecurityChatBotPart2
{
    //Handles all MySQL database operations for the Task Assistant feature

    public class Databasehelper
    {
        // ---------------------------------------------------------------
        // IMPORTANT: change "uid" and "pwd" to match your own MySQL login.
        // If you used the default root account with no password while
        // testing locally, you can leave pwd empty (not recommended for
        // production, but fine for a POE submission running on your PC).
        // ---------------------------------------------------------------
        private const string CONNECTION_STRING =
            "server=localhost;database=CyberChatBotDB;uid=root;pwd=IIEjoey2026@;";

        /// <summary>
        /// Quick check used at startup so the GUI can show a friendly
        /// message instead of crashing if MySQL isn't reachable.
        /// </summary>
        public bool TestConnection(out string errorMessage)
        {
            errorMessage = "";
            try
            {
                using (MySqlConnection conn = new MySqlConnection(CONNECTION_STRING))
                {
                    conn.Open();
                }
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        public int AddTask(string title, string description, DateTime? reminderDate)
        {
            using (MySqlConnection conn = new MySqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string query = @"INSERT INTO Tasks (Title, Description, ReminderDate, IsCompleted)
                                  VALUES (@title, @description, @reminderDate, 0);
                                  SELECT LAST_INSERT_ID();";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.AddWithValue("@description", description ?? "");
                    cmd.Parameters.AddWithValue("@reminderDate",
                        reminderDate.HasValue ? (object)reminderDate.Value : DBNull.Value);

                    object result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        public List<TaskManager> GetAllTasks()
        {
            List<TaskManager> tasks = new List<TaskManager>();

            using (MySqlConnection conn = new MySqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string query = "SELECT TaskId, Title, Description, ReminderDate, IsCompleted, CreatedAt FROM Tasks ORDER BY CreatedAt DESC;";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tasks.Add(new TaskManager
                        {
                            TaskId = reader.GetInt32("TaskId"),
                            Title = reader.GetString("Title"),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? "" : reader.GetString("Description"),
                            ReminderDate = reader.IsDBNull(reader.GetOrdinal("ReminderDate")) ? (DateTime?)null : reader.GetDateTime("ReminderDate"),
                            IsCompleted = reader.GetBoolean("IsCompleted"),
                            CreatedAt = reader.GetDateTime("CreatedAt")
                        });
                    }
                }
            }

            return tasks;
        }

        public bool MarkTaskCompleted(int taskId)
        {
            using (MySqlConnection conn = new MySqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string query = "UPDATE Tasks SET IsCompleted = 1 WHERE TaskId = @id;";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", taskId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteTask(int taskId)
        {
            using (MySqlConnection conn = new MySqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string query = "DELETE FROM Tasks WHERE TaskId = @id;";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", taskId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool SetReminder(int taskId, DateTime reminderDate)
        {
            using (MySqlConnection conn = new MySqlConnection(CONNECTION_STRING))
            {
                conn.Open();

                string query = "UPDATE Tasks SET ReminderDate = @reminderDate WHERE TaskId = @id;";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@reminderDate", reminderDate);
                    cmd.Parameters.AddWithValue("@id", taskId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}
