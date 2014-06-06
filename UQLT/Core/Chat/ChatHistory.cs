using System;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using UQLT.ViewModels;

namespace UQLT.Core.Chat
{
    /// <summary>
    /// Class responsible for logging the history of chat messages
    /// </summary>
    public class ChatHistory
    {
        private ChatMessageViewModel CMVM;
        private string sqlConString = "Data Source=" + System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data\\chist.udb");
        private string sqlDbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data\\chist.udb");

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatHistory" /> class.
        /// </summary>
        /// <param name="cmvm">The ChatMessageViewModel associated with this class.</param>
        public ChatHistory(ChatMessageViewModel cmvm)
        {
            CMVM = cmvm;
            VerifyHistoryDb();
        }

        /// <summary>
        /// Gets the SQL connection string.
        /// </summary>
        /// <value>
        /// The SQL connection string.
        /// </value>
        //private string SqlConnString
        //{
        //    get
        //    {
        //        return "Data Source=" + sqlDbPath;
        //    }
        //}

        /// <summary>
        /// Start a new thread to add the chat message to the chat database.
        /// </summary>
        /// <param name="profile">The name of the currently-logged in user.</param>
        /// <param name="otheruser">The name of the remote user we are chatting with.</param>
        /// <param name="msgtype">The type of message.</param>
        /// <param name="message">The message.</param>
        /// <param name="date">The date.</param>
        public void AddMessageToHistoryDb(string profile, string otheruser, TypeOfMessage msgtype, string message, string date)
        {
            object[] param = new object[5];
            param[0] = profile;
            param[1] = otheruser;
            param[2] = msgtype;
            param[3] = message;
            param[4] = date;

            // Start a new thread.
            Thread backgroundthread = new Thread(new ParameterizedThreadStart(AddMessageToHistoryInBackground));
            backgroundthread.Start(param);
        }

        /// <summary>
        /// Deletes the chat history between the currently-logged in user and the remote user.
        /// </summary>
        /// <param name="profile">The currently-logged in user.</param>
        /// <param name="otheruser">The remote user we are chatting with.</param>
        public void DeleteChatHistoryForUser(string profile, string otheruser)
        {
            if (VerifyHistoryDb())
            {
                try
                {
                    using (SQLiteConnection sqlcon = new SQLiteConnection(sqlConString))
                    {
                        sqlcon.Open();

                        using (SQLiteCommand cmd = new SQLiteCommand(sqlcon))
                        {
                            cmd.CommandText = "DELETE FROM chathistory WHERE profile = @profile AND otheruser = @otheruser";
                            cmd.Prepare();
                            cmd.Parameters.AddWithValue("@profile", profile);
                            cmd.Parameters.AddWithValue("@otheruser", otheruser);
                            int rowsAffected = cmd.ExecuteNonQuery();

                            MessageBox.Show("Deleted all " + rowsAffected + " messages between " + profile + " and " + otheruser);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        /// <summary>
        /// Starts a new thread to retrieve the message history between the current logged in user
        /// and the remote user.
        /// </summary>
        /// <param name="profile">The currently-logged in user.</param>
        /// <param name="otheruser">The remote user we are chatting with.</param>
        public void RetrieveMessageHistory(string profile, string otheruser)
        {
            object[] param = new object[2];
            param[0] = profile;
            param[1] = otheruser;

            Thread backgroundthread = new Thread(new ParameterizedThreadStart(RetrieveMessageHistoryInBackground));
            backgroundthread.Start(param);
        }

        /// <summary>
        /// Adds the message to the chat history in the background.
        /// </summary>
        /// <param name="data">The data.</param>
        private void AddMessageToHistoryInBackground(object data)
        {
            object[] parameters = data as object[];

            string profile = (string)parameters[0];
            string otheruser = (string)parameters[1];
            TypeOfMessage msgtype = (TypeOfMessage)parameters[2];
            string message = (string)parameters[3];
            string date = (string)parameters[4];
            if (VerifyHistoryDb())
            {
                try
                {
                    using (SQLiteConnection sqlcon = new SQLiteConnection(sqlConString))
                    {
                        sqlcon.Open();

                        using (SQLiteCommand cmd = new SQLiteCommand(sqlcon))
                        {
                            cmd.CommandText = "INSERT INTO chathistory(profile, otheruser, msgtype, message, date) VALUES(@profile, @otheruser, @msgtype, @message, @date)";
                            cmd.Prepare();
                            cmd.Parameters.AddWithValue("@profile", profile);
                            cmd.Parameters.AddWithValue("@otheruser", otheruser);
                            cmd.Parameters.AddWithValue("@msgtype", (long)msgtype);
                            cmd.Parameters.AddWithValue("@message", message);
                            cmd.Parameters.AddWithValue("@date", date);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Problem writing message to history database: " + ex);
                }
            }
            Debug.WriteLine("Added chat with user: " + otheruser + " [current user: " + profile + "] message: " + message + " on: " + date + " to chat history database on background thread");
        }

        /// <summary>
        /// Checks whether the chat history database file exists on the disk.
        /// </summary>
        /// <returns><c>true</c> if file exists, otherwise <c>false</c></returns>
        private bool ChatHistoryDbExists()
        {
            return (File.Exists(sqlDbPath)) ? true : false;
        }

        /// <summary>
        /// Creates the history database file on the disk if it doesn't already exist.
        /// </summary>
        private void CreateHistoryDb()
        {
            if (!ChatHistoryDbExists())
            {
                SQLiteConnection.CreateFile(sqlDbPath);

                try
                {
                    using (SQLiteConnection sqlcon = new SQLiteConnection(sqlConString))
                    {
                        sqlcon.Open();

                        string s = "CREATE TABLE chathistory (id INTEGER PRIMARY KEY AUTOINCREMENT, profile TEXT NOT NULL, otheruser TEXT NOT NULL, msgtype INTEGER, message TEXT, date DATETIME)";
                        using (SQLiteCommand cmd = new SQLiteCommand(s, sqlcon))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }

                    Debug.WriteLine("Chat history database created.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    DeleteHistoryDb();
                }
            }
        }

        /// <summary>
        /// Deletes the entire history database file, if it exists.
        /// </summary>
        private void DeleteHistoryDb()
        {
            if (ChatHistoryDbExists())
            {
                File.Delete(sqlDbPath);
            }
        }

        /// <summary>
        /// Retrieves the message history in the background.
        /// </summary>
        /// <param name="data">The data.</param>
        private void RetrieveMessageHistoryInBackground(object data)
        {
            object[] parameters = data as object[];
            string profile = (string)parameters[0];
            string otheruser = (string)parameters[1];

            if (VerifyHistoryDb())
            {
                try
                {
                    using (SQLiteConnection sqlcon = new SQLiteConnection(sqlConString))
                    {
                        sqlcon.Open();

                        using (SQLiteCommand cmd = new SQLiteCommand(sqlcon))
                        {
                            cmd.CommandText = "SELECT * FROM chathistory WHERE profile = @profile AND otheruser = @otheruser ORDER BY date(date) ASC";
                            cmd.Prepare();
                            cmd.Parameters.AddWithValue("@profile", profile);
                            cmd.Parameters.AddWithValue("@otheruser", otheruser);

                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        if ((long)reader["msgtype"] == (long)TypeOfMessage.Incoming)
                                        {
                                            CMVM.ReceivedMessages = "[" + reader["date"] + "] " + reader["otheruser"] + ": " + reader["message"];
                                        }
                                        else if ((long)reader["msgtype"] == (long)TypeOfMessage.Outgoing)
                                        {
                                            CMVM.ReceivedMessages = "[" + reader["date"] + "] " + reader["profile"] + ": " + reader["message"];
                                        }
                                    }
                                }
                                else
                                {
                                    Debug.WriteLine("No chat history between current user: " + profile + " and other user: " + otheruser + " found.");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        /// <summary>
        /// Verifies that the chat history database contains the proper table.
        /// </summary>
        private bool VerifyHistoryDb()
        {
            if (!ChatHistoryDbExists())
            {
                CreateHistoryDb();
                return true;
            }
            else
            {
                using (SQLiteConnection sqlcon = new SQLiteConnection(sqlConString))
                {
                    sqlcon.Open();

                    using (SQLiteCommand cmd = new SQLiteCommand(sqlcon))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "SELECT * FROM sqlite_master WHERE type = 'table' AND name = 'chathistory'";

                        using (SQLiteDataReader sdr = cmd.ExecuteReader())
                        {
                            if (sdr.Read())
                            {
                                //Debug.WriteLine("Chat history table found in DB");
                                return true;
                            }
                            else
                            {
                                Debug.WriteLine("Chat history table not found in DB... Creating...");
                                CreateHistoryDb();
                                return false;
                            }
                        }
                    }
                }
            }
        }
    }
}