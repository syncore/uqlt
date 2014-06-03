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

		private string GetHistoryDbName()
		{
			return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data\\chist.udb");
		}

		private void CreateHistoryDb()
		{
			if (!File.Exists(GetHistoryDbName()))
			{
				SQLiteConnection.CreateFile(GetHistoryDbName());
			}
			try
			{
				ConnectToDb();

				string s = "CREATE TABLE chathistory (id INTEGER PRIMARY KEY AUTOINCREMENT, msgfrom TEXT NOT NULL, msgto TEXT NOT NULL, message TEXT, date DATETIME)";
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

		private void ConnectToDb()
		{
			sqlcon = new SQLiteConnection("Data Source=" + GetHistoryDbName());
			sqlcon.Open();
		}

		private void DisconnectDb()
		{
			//sqlcon.Close();
			sqlcon.Dispose();
		}

	}
}
