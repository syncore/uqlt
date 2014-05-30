using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using agsXMPP;
using agsXMPP.Collections;
using agsXMPP.protocol.client;
using Caliburn.Micro;
using UQLT.Core.Chat;

namespace UQLT.ViewModels
{
	/// <summary>
	/// ViewModel representing a ChatMessageView
	/// </summary>

	[Export(typeof(ChatMessageViewModel))]
	public class ChatMessageViewModel : PropertyChangedBase, IHaveDisplayName
	{
		private XmppClientConnection _xmppcon;
		private QLChatHandler _handler;
		private Jid _jid;

		private string _displayName;

		public string DisplayName
		{
			get
			{
				return _displayName;
			}
			set
			{
				_displayName = value;
			}
		}

		private string _fromMessage;
		public string FromMessage
		{
			get
			{
				return _fromMessage;
			}
			set
			{
				_fromMessage = value;
				NotifyOfPropertyChange(() => FromMessage);
			}
		}

		private string _toMessage;
		public string ToMessage
		{
			get
			{
				return _toMessage;
			}
			set
			{
				_toMessage = value;
				NotifyOfPropertyChange(() => ToMessage);
			}
		}

		[ImportingConstructor]
		public ChatMessageViewModel(Jid jid, XmppClientConnection xmppcon, QLChatHandler handler)
		{
			_jid = jid;
			_displayName = "Chatting with " + _jid.User;
			_xmppcon = xmppcon;
			_handler = handler;

			QLChatHandler.ActiveChats.Add(_jid.Bare.ToLowerInvariant(), this);
			Debug.WriteLine("*** ADDING TO MESSAGE GRABBER: " + _jid.ToString());
			xmppcon.MessageGrabber.Add(_jid, new BareJidComparer(), new MessageCB(MessageCallback), null);
		}

		public void RemoveActiveChat()
		{
			QLChatHandler.ActiveChats.Remove(_jid.Bare.ToLowerInvariant());
			_xmppcon.MessageGrabber.Remove(_jid);

			// in the case where the manual string jid was added from the ChatListViewModel:
			_xmppcon.MessageGrabber.Remove(_jid.Bare);
			Debug.WriteLine("Window closed. Removed active chat from user: " + _jid.Bare.ToLowerInvariant() + " Current active chat count: " + QLChatHandler.ActiveChats.Count);
		}

		// This is called from the view itself.
		public void SendMessage(string message)
		{
			if (message.Length != 0)
			{

				ToMessage = "[" + DateTime.Now.ToShortTimeString() + "] Me: " + message + "\n";
				_handler.SendMessage(_jid, message);
				Debug.WriteLine("Sending recipient: " + _jid.ToString() + " message: " + message);
				FromMessage += ToMessage;
				ToMessage = string.Empty;
			}
		}

		private void MessageCallback(object sender, agsXMPP.protocol.client.Message msg, object data)
		{
			if (msg.Body != null)
			{
				Debug.WriteLine("Incoming message body is: " + msg.Body);
				IncomingMessage(msg);
			}
			else
			{
				Debug.WriteLine("Null message body...");
			}
		}

		public void IncomingMessage(agsXMPP.protocol.client.Message msg)
		{
			FromMessage += "[" + DateTime.Now.ToShortTimeString() + "] " + msg.From.User.ToLowerInvariant() + ": " + msg.Body + "\n";
		}

	}
}