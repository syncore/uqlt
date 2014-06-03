using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using agsXMPP;
using agsXMPP.Collections;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.roster;
using agsXMPP.Xml.Dom;
using Caliburn.Micro;
using Newtonsoft.Json;
using UQLT.Helpers;
using UQLT.Models.Chat;
using UQLT.Models.QuakeLiveAPI;
using UQLT.ViewModels;

namespace UQLT.Core.Chat
{

	/// <summary>
	/// Helper class that handles backend XMPP connection and related XMPP events for the buddy list (ChatListViewModel) and Messages (ChatListMessageViewModel)
	/// </summary>
	public class ChatHandler
	{
		private XmppClientConnection _xmppCon;
		private ChatGameInfo _qlChatGameInfo;
		private readonly IWindowManager windowManager;
		private QLFormatHelper QLFormatter = QLFormatHelper.Instance;
		public static Hashtable ActiveChats = new Hashtable();
		private SoundPlayer sp = new SoundPlayer(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data\\sounds\\notice.wav"));

		public ChatGameInfo ChatGameInfo
		{
			get
			{
				return _qlChatGameInfo;
			}
			set
			{
				_qlChatGameInfo = value;
			}
		}

		public ChatListViewModel CLVM
		{
			get;
			private set;
		}

		public ChatMessageViewModel CMVM
		{
			get;
			private set;
		}

		public Dictionary<string, string> Roster
		{
			get;
			set;
		}
		public XmppClientConnection XmppCon
		{
			get
			{
				return _xmppCon;
			}
			set
			{
				_xmppCon = value;
			}
		}

		// Buddylist
		public ChatHandler(ChatListViewModel clvm, IWindowManager wm)
		{
			CLVM = clvm;
			windowManager = wm;
			Roster = new Dictionary<string, string>();
			_xmppCon = new XmppClientConnection();


			// XmppClientConnection event handlers
			XmppCon.OnLogin += new ObjectHandler(XmppCon_OnLogin);
			XmppCon.OnRosterItem += new XmppClientConnection.RosterHandler(XmppCon_OnRosterItem);
			// TODO: will probably need an OnRosterEnd event when Roster is fully loaded
			XmppCon.OnRosterEnd += new ObjectHandler(XmppCon_OnRosterEnd);
			XmppCon.OnPresence += new PresenceHandler(XmppCon_OnPresence);
			XmppCon.OnMessage += new MessageHandler(XmppCon_OnMessage);
			XmppCon.ClientSocket.OnValidateCertificate += new RemoteCertificateValidationCallback(ClientSocket_OnValidateCertificate);

			ConnectToXMPP();
			_qlChatGameInfo = new ChatGameInfo(this);

		}

		private void ConnectToXMPP()
		{
			XmppCon.Username = ***REMOVED***;
			XmppCon.Password = ***REMOVED***;
			XmppCon.Server = UQLTGlobals.QLXMPPDomain;
			XmppCon.Port = 5222;
			XmppCon.Resource = "quakelive";
			XmppCon.AutoRoster = true;
			XmppCon.AutoPresence = true;
			XmppCon.Open();
		}

		private void XmppCon_OnRosterItem(object sender, RosterItem item)
		{
			// TODO: if (item.Subscription != SubscriptionType.remove) stuff
			try
			{
				Roster.Add(item.Jid.Bare.ToLowerInvariant(), item.Jid.User.ToLowerInvariant());

				// Additions and removals to ObservableDictionary must be done on the UI thread since ObservableDictionary is databound
				Execute.OnUIThread(() =>
				{
					CLVM.OfflineGroup.Friends[item.Jid.User.ToLowerInvariant()] = new FriendViewModel(new Friend(item.Jid.User.ToLowerInvariant(), IsFavoriteFriend(item.Jid.User)));
				});
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
		}

		// Roster has been fully loaded

		public void PlayNotificationSound()
		{

			try
			{
				sp.Play();
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
		}

		private void XmppCon_OnRosterEnd(object sender)
		{
			// Start timer to refresh in-game friends' server information
			ChatGameInfo.StartServerUpdateTimer();
		}

		// We've received a message.

		private void XmppCon_OnMessage(object sender, agsXMPP.protocol.client.Message msg)
		{
			if (msg.Body != null)
			{
				if (!ActiveChats.ContainsKey(msg.From.Bare.ToLowerInvariant()))
				{
					var cm = new ChatMessageViewModel(msg.From, XmppCon, this);
					dynamic settings = new ExpandoObject();
					settings.WindowStartupLocation = WindowStartupLocation.Manual;

					Execute.OnUIThread(() =>
					{
						windowManager.ShowWindow(cm, null, settings);
						cm.MessageIncoming(msg);
						PlayNotificationSound();
					});

				}
				else
				{
					Debug.WriteLine("A chat window already exists for: " + msg.From.Bare.ToLowerInvariant() + " not opening another.");
				}
			}
		}


		// We have successfully authenticated to the server.

		private void XmppCon_OnLogin(object sender)
		{
		}


		// We've received a presence from a contact. Subscriptions are also handled in this event.

		private void XmppCon_OnPresence(object sender, Presence pres)
		{
			switch (pres.Type)
			{
				case PresenceType.available:
					FriendBecameAvailable(pres);
					break;

				case PresenceType.unavailable:
					FriendBecameUnavailble(pres);
					break;

				case PresenceType.subscribe:
					// Show a message indicating that friend request was sent to another user
					break;

				case PresenceType.subscribed:
					// Show a dialog that allows the user to accept or reject the incoming friend request
					break;

				case PresenceType.unsubscribe:
					// Show a message indicating that user successfully un-friended another user
					break;

				case PresenceType.unsubscribed:
					// Show a message indicating that the user chose to reject the incoming friend request
					break;
			}
		}

		// Check a user's status to determine what the user is doing in QL
		// Only fired when availability changes (user: offline -> online OR leave game server <-> join game server)

		private void CheckPlayerStatus(Presence pres)
		{
			if (string.IsNullOrEmpty(pres.Status))
			{
				ClearPlayerStatus(pres);
				Debug.WriteLine("**Status for " + pres.From.User.ToLowerInvariant() + " is empty.");
			}
			else
			{
				CLVM.OnlineGroup.Friends[pres.From.User.ToLowerInvariant()].HasXMPPStatus = true;
				UpdatePlayerStatus(pres.From.User, pres.Status);
				Debug.WriteLine("**Status for " + pres.From.User.ToLowerInvariant() + " is: " + pres.Status);
			}
		}


		// User has an XMPP status, meaning that the user is either doing one of three things: watching a demo, playing a practice match, or actually in a game server
		// This method will determine which of these three things the user is doing.

		private void UpdatePlayerStatus(string friend, string status)
		{
			try
			{
				StatusInfo si = JsonConvert.DeserializeObject<StatusInfo>(status);

				// BOT_GAME = 0: player could either be watching a demo (ADDRESS = "bot") or actually in a real server (ADDRESS = "ip:port")
				if (si.bot_game == 0)
				{
					// player is watching a demo
					if (si.address.Equals("bot"))
					{
						CLVM.OnlineGroup.Friends[friend.ToLowerInvariant()].StatusType = 1;
					}
					// player is actually in game
					else
					{
						CLVM.OnlineGroup.Friends[friend.ToLowerInvariant()].StatusType = 3;
						CLVM.OnlineGroup.Friends[friend.ToLowerInvariant()].IsInGame = true;
						// query API to get type, map, location, player count info for status message
						ChatGameInfo.CreateServerInfoForStatus(friend.ToLowerInvariant(), si.server_id);
					}
				}

				// BOT_GAME = 1: player is in a practice game or training match (we don't care about ADDRESS, but it will be = "loopback")
				if (si.bot_game == 1)
				{
					CLVM.OnlineGroup.Friends[friend.ToLowerInvariant()].StatusType = 2;
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
			}
		}

		private void ClearPlayerStatus(Presence pres)
		{
			FriendViewModel val;
			// On program start there is a timing issue where the key won't exist, so need to check first
			if (CLVM.OnlineGroup.Friends.TryGetValue(pres.From.User.ToLowerInvariant(), out val))
			{
				CLVM.OnlineGroup.Friends[pres.From.User.ToLowerInvariant()].HasXMPPStatus = false;
				CLVM.OnlineGroup.Friends[pres.From.User.ToLowerInvariant()].IsInGame = false;
				CLVM.OnlineGroup.Friends[pres.From.User.ToLowerInvariant()].StatusType = 0;
			}

		}

		private void FriendBecameAvailable(Presence pres)
		{
			if (!IsMe(pres))
			{
				// prevent "double" online status
				if (!IsFriendAlreadyOnline(pres.From.User))
				{
					Debug.WriteLine("[FRIEND AVAILABLE]: " + " Bare Jid: " + pres.From.Bare + " User: " + pres.From.User);
					Debug.WriteLine("Friends list before adding " + pres.From.User + "," + " count: " + CLVM.OnlineGroup.Friends.Count());

					// Must be done on the UI thread
					Execute.OnUIThread(() =>
					{
						CLVM.OnlineGroup.Friends[pres.From.User.ToLowerInvariant()] = new FriendViewModel(new Friend(pres.From.User.ToLowerInvariant(), IsFavoriteFriend(pres.From.User)));
						CLVM.OnlineGroup.Friends[pres.From.User.ToLowerInvariant()].IsOnline = true;
					});
					Debug.WriteLine("Friends list after adding " + pres.From.User + "," + " count: " + CLVM.OnlineGroup.Friends.Count());
				}

				// user was previously offline
				if (CLVM.OfflineGroup.Friends.ContainsKey(pres.From.User.ToLowerInvariant()))
				{
					// Additions and removals to ObservableDictionary must be done on the UI thread
					Execute.OnUIThread(() =>
					{
						CLVM.OfflineGroup.Friends.Remove(pres.From.User.ToLowerInvariant());
					});
					Debug.WriteLine("Friends list before adding " + pres.From.User + "," + " count: " + CLVM.OnlineGroup.Friends.Count() + " After: " + CLVM.OnlineGroup.Friends.Count());
				}

			}

			// Check the user's status
			CheckPlayerStatus(pres);
		}

		private void FriendBecameUnavailble(Presence pres)
		{
			if (!IsMe(pres))
			{
				Debug.WriteLine("[FRIEND UNAVAILABLE]: " + " Bare Jid: " + pres.From.Bare + " User: " + pres.From.User);
				Debug.WriteLine("Friends list before removing " + pres.From.User + "," + " count: " + CLVM.OnlineGroup.Friends.Count());
				// Additions and removals to ObservableDictionary must be done on the UI thread
				Execute.OnUIThread(() =>
				{
					CLVM.OnlineGroup.Friends.Remove(pres.From.User.ToLowerInvariant());
					CLVM.OfflineGroup.Friends[pres.From.User.ToLowerInvariant()] = new FriendViewModel(new Friend(pres.From.User.ToLowerInvariant(), IsFavoriteFriend(pres.From.User)));
					CLVM.OfflineGroup.Friends[pres.From.User.ToLowerInvariant()].IsOnline = false;


				});
				Debug.WriteLine("Friends list after removing " + pres.From.User + "," + " count: " + CLVM.OnlineGroup.Friends.Count());
			}
		}



		public bool SendMessage(Jid recipient, string message)
		{
			if (IsFriendAlreadyOnline(recipient.User.ToLowerInvariant()))
			{
				agsXMPP.protocol.client.Message msg = new agsXMPP.protocol.client.Message();
				msg.Type = MessageType.chat;
				msg.To = recipient;
				msg.Body = message;
				XmppCon.Send(msg);
				return true;
			}
			else
			{
				return false;
			}

		}

		public string MyJidUser()
		{
			return XmppCon.MyJID.User.ToLowerInvariant();
		}

		private bool IsMe(Presence pres)
		{
			return (pres.From.Bare.Equals(XmppCon.MyJID.Bare.ToLowerInvariant())) ? true : false;
		}

		private bool IsFavoriteFriend(string friend)
		{
			return (UQLTGlobals.SavedFavoriteFriends.Contains(friend.ToLowerInvariant())) ? true : false;
		}

		private bool IsFriendAlreadyOnline(string friend)
		{
			return (CLVM.OnlineGroup.Friends.ContainsKey(friend.ToLowerInvariant())) ? true : false;

		}

		private bool ClientSocket_OnValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}

	}
}