using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using System.Text;
using System.Threading.Tasks;
using UQLT.ViewModels;
using System.IO;
using System.Diagnostics;

namespace UQLT.Core.Chat
{
	/// <summary>
	/// Class responsible for logging the history of chat messages
	/// </summary>
	public class ChatHistory
	{
		private ChatMessageViewModel CMVM;
		private SQLiteConnection sqlcon;

		public ChatHistory(ChatMessageViewModel cmvm)
		{
			CMVM = cmvm;
			CreateHistoryDb();

		}

		// Return the database filename on the disk.
		private string GetHistoryDbName()
		{
			return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data\\chist.udb");
		}

		// Create the database file on the disk if it does not exist.
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

		// Add the chat message to the database of messages.
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

		// Retrieve the message history between the current logged in user (profile) and the remote user (otheruser)
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

		// Make the database connection
		private void ConnectToDb()
		{
			sqlcon = new SQLiteConnection("Data Source=" + GetHistoryDbName());
			sqlcon.Open();
		}

		// Dispose of the database connection and all of its resources
		private void DisconnectDb()
		{
			sqlcon.Dispose();
		}

	}
}