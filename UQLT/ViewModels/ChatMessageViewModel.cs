using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Text;
using agsXMPP;
using agsXMPP.Collections;
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
        private ChatHistory _chathistory;
        private string _displayName;
        private ChatHandler _handler;
        private string _incomingMessage;
        private Jid _jid;
        private string _outgoingMessage;
        private StringBuilder _receivedMessages = new StringBuilder();
        private XmppClientConnection _xmppcon;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatMessageViewModel" /> class.
        /// </summary>
        /// <param name="jid">The jid of the user we are chatting with.</param>
        /// <param name="xmppcon">The XmppClientConnection to use for this viewmodel.</param>
        /// <param name="handler">The ChatHandler to use for this viewmodel.</param>
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

        /// <summary>
        /// Gets or Sets the display name of the window (ChatMessageView) associated with this view model.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the incoming message.
        /// </summary>
        /// <value>The incoming message.</value>
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

        /// <summary>
        /// Gets or sets the outgoing message.
        /// </summary>
        /// <value>The outgoing message.</value>
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

        /// <summary>
        /// Gets or sets the received messages.
        /// </summary>
        /// <value>The received messages.</value>
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

        /// <summary>
        /// Appends the chat history between the currently-logged in user and the remote user to the
        /// chat window.
        /// </summary>
        public void AppendChatHistory()
        {
            _chathistory.RetrieveMessageHistory(_handler.MyJidUser(), _jid.User);
        }

        /// <summary>
        /// Displays the incoming message.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public void MessageIncoming(agsXMPP.protocol.client.Message msg)
        {
            _handler.PlayNotificationSound();
            IncomingMessage = msg.Body + "\n";
            ReceivedMessages = "[" + DateTime.Now.ToShortTimeString() + "] " + msg.From.User.ToLowerInvariant() + ": " + IncomingMessage;
            // Log the message
            _chathistory.AddMessageToHistoryDb(_handler.MyJidUser(), _jid.User, TypeOfMessage.Incoming, IncomingMessage, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            IncomingMessage = string.Empty;
        }

        /// <summary>
        /// Removes this chat the active chats.
        /// </summary>
        /// <remarks>This is called from the view itself (when the view closes).</remarks>
        public void RemoveActiveChat()
        {
            ChatHandler.ActiveChats.Remove(_jid.Bare.ToLowerInvariant());
            _xmppcon.MessageGrabber.Remove(_jid);

            // in the case where the manual string jid was added from the ChatListViewModel:
            _xmppcon.MessageGrabber.Remove(_jid.Bare);
            Debug.WriteLine("Window closed. Removed active chat from user: " + _jid.Bare.ToLowerInvariant() + " Current active chat count: " + ChatHandler.ActiveChats.Count);
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <remarks>This is called from the view itself.</remarks>
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

        /// <summary>
        /// This is called when a message is received within the view associated with this viewmodel.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="msg">The message.</param>
        /// <param name="data">The data.</param>
        private void MessageCallback(object sender, agsXMPP.protocol.client.Message msg, object data)
        {
            if (msg.Body != null)
            {
                Debug.WriteLine("Incoming message body is: " + msg.Body);
                MessageIncoming(msg);
            }
        }
    }
}