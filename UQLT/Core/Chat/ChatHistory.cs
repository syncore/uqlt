using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using UQLT.ViewModels;

namespace UQLT.Core.Chat
{
    /// <summary>
    /// Class responsible for logging the history of chat messages
    /// </summary>
    public class ChatHistory
    {
        private ChatMessageViewModel CMVM;
        private SQLiteConnection sqlcon;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatHistory"/> class.
        /// </summary>
        /// <param name="cmvm">The ChatMessageViewModel associated with this class.</param>
        public ChatHistory(ChatMessageViewModel cmvm)
        {
            CMVM = cmvm;
            CreateHistoryDb();
        }

        /// <summary>
        /// Gets the name of the history database.
        /// </summary>
        /// <returns>
        /// The filename and path of the chat history database on the disk.
        /// </returns>
        private string GetHistoryDbName()
        {
            return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data\\chist.udb");
        }

        /// <summary>
        /// Creates the history database file on the disk if it doesn't already exist.
        /// </summary>
        private void CreateHistoryDb()
        {
            if (!File.Exists(GetHistoryDbName()))
            {
                SQLiteConnection.CreateFile(GetHistoryDbName());

                try
                {
                    ConnectToDb();

                    string s = "CREATE TABLE chathistory (id INTEGER PRIMARY KEY AUTOINCREMENT, profile TEXT NOT NULL, otheruser TEXT NOT NULL, msgtype INTEGER, message TEXT, date DATETIME)";
                    SQLiteCommand cmd = new SQLiteCommand(s, sqlcon);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();

                    DisconnectDb();

                    Debug.WriteLine("Chat history database created.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    File.Delete(GetHistoryDbName());
                }
            }
        }

        /// <summary>
        /// Adds the chat message to the chat history database.
        /// </summary>
        /// <param name="profile">The name of the currently-logged in user.</param>
        /// <param name="otheruser">The name of the remote user we are chatting with.</param>
        /// <param name="msgtype">The type of message.</param>
        /// <param name="message">The message.</param>
        /// <param name="date">The date.</param>
        public void AddMessageToHistoryDb(string profile, string otheruser, TypeOfMessage msgtype, string message, string date)
        {
            ConnectToDb();
            SQLiteCommand cmd = new SQLiteCommand(sqlcon);
            cmd.CommandText = "INSERT INTO chathistory(profile, otheruser, msgtype, message, date) VALUES(@profile, @otheruser, @msgtype, @message, @date)";
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@profile", profile);
            cmd.Parameters.AddWithValue("@otheruser", otheruser);
            cmd.Parameters.AddWithValue("@msgtype", (long)msgtype);
            cmd.Parameters.AddWithValue("@message", message);
            cmd.Parameters.AddWithValue("@date", date);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            DisconnectDb();
            Debug.WriteLine("Adding chat with user: " + otheruser + " [current user: " + profile + "] message: " + message + " on: " + date + " to chat history database.");
        }

        /// <summary>
        /// Retrieves the message history between the current logged in user and the remote user.
        /// </summary>
        /// <param name="profile">The currently-logged in user.</param>
        /// <param name="otheruser">The remote user we are chatting with.</param>
        public void RetrieveMessageHistory(string profile, string otheruser)
        {
            SQLiteDataReader reader = null;

            try
            {
                ConnectToDb();
                SQLiteCommand cmd = new SQLiteCommand(sqlcon);
                cmd.CommandText = "SELECT * FROM chathistory WHERE profile = @profile AND otheruser = @otheruser ORDER BY date(date) ASC";
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@profile", profile);
                cmd.Parameters.AddWithValue("@otheruser", otheruser);
                reader = cmd.ExecuteReader();

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
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                    DisconnectDb();
                }
            }
        }

        /// <summary>
        /// Connects to database.
        /// </summary>
        private void ConnectToDb()
        {
            sqlcon = new SQLiteConnection("Data Source=" + GetHistoryDbName());
            sqlcon.Open();
        }

        /// <summary>
        /// Disconnects from the database and disposes of all associated resources.
        /// </summary>
        private void DisconnectDb()
        {
            sqlcon.Dispose();
        }
    }
}