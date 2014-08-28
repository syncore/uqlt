using System;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using Caliburn.Micro;
using UQLT.Events;
using UQLT.Models.Configuration;
using UQLT.ViewModels;

namespace UQLT.Core.Modules.Chat
{
    /// <summary>
    /// Class responsible for logging and retrieval of the history of chat messages.
    /// </summary>
    public class ChatHistory
    {
        private readonly ConfigurationHandler _cfgHandler = new ConfigurationHandler();
        private readonly ChatMessageViewModel _cmvm;
        private readonly IEventAggregator _events;
        private readonly string _sqlConString = "Data Source=" + UQltFileUtils.GetChatHistoryDatabasePath();
        private readonly string _sqlDbPath = UQltFileUtils.GetChatHistoryDatabasePath();

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatHistory" /> class.
        /// </summary>
        /// <param name="cmvm">The ChatMessageViewModel associated with this class.</param>
        public ChatHistory(ChatMessageViewModel cmvm)
        {
            _cmvm = cmvm;
            VerifyHistoryDb();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatHistory" /> class.
        /// </summary>
        /// <param name="events">The events.</param>
        /// <remarks>
        /// This constructor is used solely to clear the chat history from
        /// another viewmodel such as the <see cref="ChatListViewModel" />
        /// </remarks>
        public ChatHistory(IEventAggregator events)
        {
            _events = events;
            VerifyHistoryDb();
        }

        /// <summary>
        /// Start a new thread to add the chat message to the chat database.
        /// </summary>
        /// <param name="profile">The name of the currently-logged in user.</param>
        /// <param name="otheruser">The name of the remote user we are chatting with.</param>
        /// <param name="msgtype">The type of message.</param>
        /// <param name="message">The message.</param>
        /// <param name="date">The date.</param>
        public void AddMessageToHistoryDb(string profile, string otheruser, ChatMessageTypes msgtype, string message, string date)
        {
            if (!IsChatHistoryEnabled()) { return; }

            object[] param = new object[5];
            param[0] = profile;
            param[1] = otheruser;
            param[2] = msgtype;
            param[3] = message;
            param[4] = date;

            // Start a new thread.
            var backgroundthread = new Thread(AddMessageToHistoryInBackground);
            backgroundthread.Start(param);
        }

        /// <summary>
        /// Deletes the chat history between the currently-logged in user and the remote user.
        /// </summary>
        /// <param name="profile">The currently-logged in user.</param>
        /// <param name="otheruser">The remote user we are chatting with.</param>
        /// <param name="sentfrombuddylist">if set to <c>true</c> then method was called from the <see cref="ChatListViewModel"/>.</param>
        public void DeleteChatHistoryForUser(string profile, string otheruser, bool sentfrombuddylist)
        {
            if (!VerifyHistoryDb()) { return; }

            try
            {
                using (var sqlcon = new SQLiteConnection(_sqlConString))
                {
                    sqlcon.Open();

                    using (var cmd = new SQLiteCommand(sqlcon))
                    {
                        cmd.CommandText = "DELETE FROM chathistory WHERE profile = @profile AND otheruser = @otheruser";
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@profile", profile);
                        cmd.Parameters.AddWithValue("@otheruser", otheruser);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        // Cleared from ChatListView; send event directly to ChatListMessageView
                        if (sentfrombuddylist)
                        {
                            _events.PublishOnUIThread(new ClearChatHistoryEvent(profile, otheruser));
                        }

                        MessageBox.Show("Deleted all " + rowsAffected + " messages between " + profile + " and " + otheruser);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
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
            if (!IsChatHistoryEnabled()) { return; }

            object[] param = new object[2];
            param[0] = profile;
            param[1] = otheruser;

            Thread backgroundthread = new Thread(RetrieveMessageHistoryInBackground);
            backgroundthread.Start(param);
        }

        /// <summary>
        /// Adds the message to the chat history in the background.
        /// </summary>
        /// <param name="data">The data.</param>
        private void AddMessageToHistoryInBackground(object data)
        {
            object[] parameters = data as object[];

            if (parameters == null) { return; }

            string profile = (string)parameters[0];
            string otheruser = (string)parameters[1];
            ChatMessageTypes msgtype = (ChatMessageTypes)parameters[2];
            string message = (string)parameters[3];
            string date = (string)parameters[4];
            if (VerifyHistoryDb())
            {
                try
                {
                    using (var sqlcon = new SQLiteConnection(_sqlConString))
                    {
                        sqlcon.Open();

                        using (var cmd = new SQLiteCommand(sqlcon))
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
            return (File.Exists(_sqlDbPath));
        }

        /// <summary>
        /// Creates the history database file on the disk if it doesn't already exist.
        /// </summary>
        private void CreateHistoryDb()
        {
            if (ChatHistoryDbExists()) { return; }

            SQLiteConnection.CreateFile(_sqlDbPath);

            try
            {
                using (var sqlcon = new SQLiteConnection(_sqlConString))
                {
                    sqlcon.Open();

                    string s = "CREATE TABLE chathistory (id INTEGER PRIMARY KEY AUTOINCREMENT, profile TEXT NOT NULL, otheruser TEXT NOT NULL, msgtype INTEGER, message TEXT, date DATETIME)";
                    using (var cmd = new SQLiteCommand(s, sqlcon))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                Debug.WriteLine("Chat history database created.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                DeleteHistoryDb();
            }
        }

        /// <summary>
        /// Deletes the entire history database file, if it exists.
        /// </summary>
        private void DeleteHistoryDb()
        {
            if (!ChatHistoryDbExists()) return;
            try
            {
                File.Delete(_sqlDbPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to delete chat history database: " + ex.Message);
            }
        }

        /// <summary>
        /// Determines whether the is chat history enabled.
        /// </summary>
        /// <returns><c>true</c> if the chat history is enabled, otherwise <c>false</c></returns>
        private bool IsChatHistoryEnabled()
        {
            _cfgHandler.ReadConfig();
            return (_cfgHandler.ChatOptLogging);
        }

        /// <summary>
        /// Retrieves the message history in the background.
        /// </summary>
        /// <param name="data">The data.</param>
        private void RetrieveMessageHistoryInBackground(object data)
        {
            object[] parameters = data as object[];
            if (parameters == null) { return; }

            string profile = (string)parameters[0];
            string otheruser = (string)parameters[1];

            if (!VerifyHistoryDb()) { return; }

            try
            {
                using (var sqlcon = new SQLiteConnection(_sqlConString))
                {
                    sqlcon.Open();

                    using (var cmd = new SQLiteCommand(sqlcon))
                    {
                        cmd.CommandText = "SELECT * FROM chathistory WHERE profile = @profile AND otheruser = @otheruser ORDER BY date(date) ASC";
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@profile", profile);
                        cmd.Parameters.AddWithValue("@otheruser", otheruser);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                var messages = new StringBuilder();

                                while (reader.Read())
                                {
                                    switch ((long)reader["msgtype"])
                                    {
                                        case (long)ChatMessageTypes.Incoming:
                                            messages.Append("[" + reader["date"] + "] " + reader["otheruser"] + ": " + reader["message"]);
                                            break;

                                        case (long)ChatMessageTypes.Outgoing:
                                            messages.Append("[" + reader["date"] + "] " + reader["profile"] + ": " + reader["message"]);
                                            break;
                                    }
                                }

                                _cmvm.ReceivedMessages = messages.ToString();
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
                Debug.WriteLine(ex.Message);
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
            using (var sqlcon = new SQLiteConnection(_sqlConString))
            {
                sqlcon.Open();

                using (var cmd = new SQLiteCommand(sqlcon))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "SELECT * FROM sqlite_master WHERE type = 'table' AND name = 'chathistory'";

                    using (var sdr = cmd.ExecuteReader())
                    {
                        if (sdr.Read())
                        {
                            //Debug.WriteLine("Chat history table found in DB");
                            return true;
                        }
                        Debug.WriteLine("Chat history table not found in DB... Creating...");
                        CreateHistoryDb();
                        return false;
                    }
                }
            }
        }
    }
}