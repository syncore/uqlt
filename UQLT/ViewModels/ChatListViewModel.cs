using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using Caliburn.Micro;
using Newtonsoft.Json;
using UQLT.Models.Chat;
using UQLT.Core.Chat;
using UQLT.Helpers;
using System.Windows.Input;
using System.Dynamic;
using System.Windows;

namespace UQLT.ViewModels
{

	[Export(typeof(ChatListViewModel))]

	/// <summary>
	/// Viewmodel for the buddy list
	/// </summary>
	public class ChatListViewModel : PropertyChangedBase
	{

		private ChatHandler Handler;
		private agsXMPP.Jid Jid;

		public IWindowManager windowManager
		{
			get;
			private set;
		}

		private static string OnlineGroupTitle = "Online Friends";
		private static string OfflineGroupTitle = "Offline Friends";

		private BindableCollection<RosterGroupViewModel> _buddyList;

		public BindableCollection<RosterGroupViewModel> BuddyList
		{
			get
			{
				return _buddyList;
			}

			set
			{
				_buddyList = value;
				NotifyOfPropertyChange(() => BuddyList);
			}
		}

		public RosterGroupViewModel OnlineGroup
		{
			get
			{
				return BuddyList[0];
			}
		}

		public RosterGroupViewModel OfflineGroup
		{
			get
			{
				return BuddyList[1];
			}
		}


		[ImportingConstructor]
		public ChatListViewModel(IWindowManager WindowManager)
		{
			windowManager = WindowManager;
			_buddyList = new BindableCollection<RosterGroupViewModel>();
			BuddyList.Add(new RosterGroupViewModel(new RosterGroup(OnlineGroupTitle), true));
			BuddyList.Add(new RosterGroupViewModel(new RosterGroup(OfflineGroupTitle), false));
			LoadFavoriteFriends();

			// Instantiate a XMPP connection and hook up related events for this viewmodel
			Handler = new ChatHandler(this, windowManager);

		}

		// Load saved friends from JSON file on disk on launch and add to global list

		private void LoadFavoriteFriends()
		{
			try
			{
				using(StreamReader sr = new StreamReader(UQLTGlobals.SavedFavFriendPath))
				{
					var s = sr.ReadToEnd();
					var favorites = JsonConvert.DeserializeObject<List<string>>(s);
					foreach (var favorite in favorites)
					{
						UQLTGlobals.SavedFavoriteFriends.Add(favorite);
						Debug.WriteLine("Auto-added " + favorite + " to favorite friends");
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
		}

		// Dump the global saved friends list to JSON file on disk

		private void SaveFavoriteFriends()
		{
			try
			{
				string friendjson = JsonConvert.SerializeObject(UQLTGlobals.SavedFavoriteFriends);
				using(FileStream fs = File.Create(UQLTGlobals.SavedFavFriendPath))
				using(TextWriter writer = new StreamWriter(fs))
				{
					writer.WriteLine(friendjson);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}

		}

		// This user can be added as a favorite friend. Also a Caliburn.Micro action guard
		// Automatically hooks up IsEnabled in View, see: https://caliburnmicro.codeplex.com/wikipage?title=All%20About%20Actions

		public bool CanAddFavoriteFriend(KeyValuePair<string, FriendViewModel> kvp)
		{
			return (!UQLTGlobals.SavedFavoriteFriends.Contains(kvp.Key)) ? true : false;
		}

		// This user can be removed from favorite friends. Also a Caliburn.Micro action guard
		// Automatically hooks up IsEnabled in View, see: https://caliburnmicro.codeplex.com/wikipage?title=All%20About%20Actions

		public bool CanRemoveFavoriteFriend(KeyValuePair<string, FriendViewModel> kvp)
		{
			return (UQLTGlobals.SavedFavoriteFriends.Contains(kvp.Key)) ? true : false;
		}


		// Add user to favorite friends. Called from the view itself.
		public void AddFavoriteFriend(KeyValuePair<string, FriendViewModel> kvp)
		{
			if (CanAddFavoriteFriend(kvp))
			{
				UQLTGlobals.SavedFavoriteFriends.Add(kvp.Key);
				Debug.WriteLine("Added " + kvp.Key + " to favorite friends");
				// Dump to disk
				SaveFavoriteFriends();
				// Reflect changes now
				kvp.Value.IsFavorite = true;

			}
			else
			{
				Debug.WriteLine("Favorites already contains " + kvp.Key);
			}

		}

		// Remove user from favorite friends. Called from the view itself.

		public void RemoveFavoriteFriend(KeyValuePair<string, FriendViewModel> kvp)
		{
			if (CanRemoveFavoriteFriend(kvp))
			{
				UQLTGlobals.SavedFavoriteFriends.Remove(kvp.Key);
				Debug.WriteLine("Removed " + kvp.Key + " from favorite friends");
				// Dump to disk
				SaveFavoriteFriends();
				// Reflect changes now
				kvp.Value.IsFavorite = false;
			}
			else
			{
				Debug.WriteLine("Favorites did not contain " + kvp.Key);
			}
		}

		// Refresh a player's game server information when the user highlights the player (via clicking his name or with keyboard) in the buddy list
		// but only do so if the player is currently in a server. Called from the view itself.

		public void UpdatePlayerGameServerInfo(KeyValuePair<string, FriendViewModel> kvp)
		{

			if (kvp.Value.IsInGame)
			{
				Debug.WriteLine("Requesting server information for friend: " + kvp.Key + " server id: " + kvp.Value.Server.PublicId);
				Handler.ChatGameInfo.UpdateServerInfoForStatus(kvp.Key);
			}
			else
			{
				Debug.WriteLine("Not refreshing server info for player: " + kvp.Key + " because player isn't currently in a game server.");
			}

		}
		// This is called from the view itself
		public void OpenChatWindow(KeyValuePair<string, FriendViewModel> kvp)
		{
			// manual jid (missing resource, but shouldn't matter)
			Jid = new agsXMPP.Jid(kvp.Key + "@" + UQLTGlobals.QLXMPPDomain);
			dynamic settings = new ExpandoObject();
			settings.Topmost = true;
			settings.WindowStartupLocation = WindowStartupLocation.Manual;

			windowManager.ShowWindow(new ChatMessageViewModel(Jid, Handler.XmppCon, Handler), null, settings);

		}

	}
}