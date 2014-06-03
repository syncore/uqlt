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
		private ChatHandler _handler;
		private Jid _jid;
		private ChatHistory _chathistory;


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

		private StringBuilder _receivedMessages = new StringBuilder();
		public string ReceivedMessages
		{
			get
			{
				return _receivedMessages.ToString();
			}
			set
			{
				_receivedMessages.Append(value);
				NotifyOfPropertyChange(() => ReceivedMessages);
			}
		}


		private string _incomingMessage;
		public string IncomingMessage
		{
			get
			{
				return _incomingMessage;
			}
			set
			{
				_incomingMessage = value;
				NotifyOfPropertyChange(() => IncomingMessage);
			}
		}

		private string _outgoingMessage;
		public string OutgoingMessage
		{
			get
			{
				return _outgoingMessage;
			}
			set
			{
				_outgoingMessage = value;
				NotifyOfPropertyChange(() => OutgoingMessage);
			}
		}


		[ImportingConstructor]
		public ChatMessageViewModel(Jid jid, XmppClientConnection xmppcon, ChatHandler handler)
		{
			_jid = jid;
			_displayName = "Chatting with " + _jid.User;
			_xmppcon = xmppcon;
			_handler = handler;

			if (!ChatHandler.ActiveChats.ContainsKey(_jid.Bare.ToLowerInvariant()))
			{
				ChatHandler.ActiveChats.Add(_jid.Bare.ToLowerInvariant(), this);
			}

			Debug.WriteLine("*** ADDING TO MESSAGE GRABBER: " + _jid.ToString());
			xmppcon.MessageGrabber.Add(_jid, new BareJidComparer(), new MessageCB(MessageCallback), null);
			_chathistory = new ChatHistory(this);
			AppendChatHistory();

		}

		// Append the chat history between the current logged in user and the remote user to the chat window.
		public void AppendChatHistory()
		{
			_chathistory.RetrieveMessageHistory(_handler.MyJidUser(), _jid.User);

		}


		// This is called from the view itself.
		public void RemoveActiveChat()
		{
			ChatHandler.ActiveChats.Remove(_jid.Bare.ToLowerInvariant());
			_xmppcon.MessageGrabber.Remove(_jid);

			// in the case where the manual string jid was added from the ChatListViewModel:
			_xmppcon.MessageGrabber.Remove(_jid.Bare);
			Debug.WriteLine("Window closed. Removed active chat from user: " + _jid.Bare.ToLowerInvariant() + " Current active chat count: " + ChatHandler.ActiveChats.Count);
		}

		// This is called from the view itself.
		public void SendMessage(string message)
		{
			if (message.Length != 0)
			{

				if (_handler.SendMessage(_jid, message))
				{
					OutgoingMessage = message + "\n";
					Debug.WriteLine("Attempting to send recipient: " + _jid.ToString() + " message: " + message);
					ReceivedMessages = "[" + DateTime.Now.ToShortTimeString() + "] " + _handler.MyJidUser() + ": " + OutgoingMessage;
					// Log the message
					_chathistory.AddMessageToHistoryDb(_handler.MyJidUser(), _jid.User, TypeOfMessage.Outgoing, OutgoingMessage, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
					OutgoingMessage = string.Empty;
				}
				else
				{
					ReceivedMessages = "" + _jid.User + " is offline. Message was not sent.\n";
					OutgoingMessage = string.Empty;
				}
			}
		}

		private void MessageCallback(object sender, agsXMPP.protocol.client.Message msg, object data)
		{
			if (msg.Body != null)
			{
				Debug.WriteLine("Incoming message body is: " + msg.Body);
				MessageIncoming(msg);
			}
		}

		// Display the incoming message
		public void MessageIncoming(agsXMPP.protocol.client.Message msg)
		{
			_handler.PlayNotificationSound();
			IncomingMessage = msg.Body + "\n";
			ReceivedMessages = "[" + DateTime.Now.ToShortTimeString() + "] " + msg.From.User.ToLowerInvariant() + ": " + IncomingMessage;
			// Log the message
			_chathistory.AddMessageToHistoryDb(_handler.MyJidUser(), _jid.User, TypeOfMessage.Incoming, IncomingMessage, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
			IncomingMessage = string.Empty;
		}

	}
}