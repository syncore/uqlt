﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Dynamic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using agsXMPP;
using agsXMPP.Collections;
using Caliburn.Micro;
using Newtonsoft.Json;
using UQLT.Core;
using UQLT.Core.Modules.Chat;
using UQLT.Events;
using UQLT.Models.QuakeLiveAPI;
using UQLT.ViewModels.ServerBrowser;

namespace UQLT.ViewModels.Chat
{
    /// <summary>
    /// ViewModel representing a ChatMessageView
    /// </summary>

    [Export(typeof(ChatMessageViewModel))]
    public class ChatMessageViewModel : PropertyChangedBase, IHaveDisplayName, IHandle<ClearChatHistoryEvent>
    {
        private readonly ChatHistory _chatHistory;
        private readonly IEventAggregator _events;
        private readonly ChatHandler _handler;
        private readonly Jid _jid;
        private readonly StringBuilder _receivedMessages = new StringBuilder();
        private readonly IWindowManager _windowManager;
        private readonly XmppClientConnection _xmppCon;
        private string _incomingMessage;
        private string _outgoingMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatMessageViewModel" /> class.
        /// </summary>
        /// <param name="jid">The jid of the user we are chatting with.</param>
        /// <param name="xmppcon">The XmppClientConnection to use for this viewmodel.</param>
        /// <param name="handler">The ChatHandler to use for this viewmodel.</param>
        /// <param name="windowManager">The window manager.</param>
        /// <param name="events">The events.</param>
        [ImportingConstructor]
        public ChatMessageViewModel(Jid jid, XmppClientConnection xmppcon, ChatHandler handler, IWindowManager windowManager, IEventAggregator events)
        {
            _jid = jid;
            DisplayName = "Chatting with " + _jid.User;
            _xmppCon = xmppcon;
            _handler = handler;
            _windowManager = windowManager;
            _events = events;
            _events.Subscribe(this);
            _chatHistory = new ChatHistory(this);
            var fromUser = _jid.Bare.ToLowerInvariant();
            if (!ChatHandler.ActiveChats.ContainsKey(fromUser))
            {
                ChatHandler.ActiveChats.Add(fromUser, this);
            }

            Debug.WriteLine("*** ADDING TO MESSAGE GRABBER: " + _jid);
            xmppcon.MessageGrabber.Add(_jid, new BareJidComparer(), MessageCallback, null);

            AppendChatHistory();
        }

        /// <summary>
        /// Gets or Sets the display name of the window (ChatMessageView) associated with this view model.
        /// </summary>
        public string DisplayName { get; set; }

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
                _receivedMessages.AppendLine(value);
                NotifyOfPropertyChange(() => ReceivedMessages);
            }
        }

        /// <summary>
        /// Gets the user we're currently chatting with for data-binding purposes in UI.
        /// </summary>
        /// <value>The user we're currently chatting with in the view.</value>
        public string UserChattingWith
        {
            get
            {
                return _jid.User;
            }
        }

        /// <summary>
        /// Appends the chat history between the currently-logged in user and the remote user to the
        /// chat window.
        /// </summary>
        public void AppendChatHistory()
        {
            _chatHistory.RetrieveMessageHistory(_handler.MyJidUser(), _jid.User);
        }

        /// <summary>
        /// Deletes the chat history for this user.
        /// </summary>
        /// <remarks>This is called from the view itself.</remarks>
        public void DeleteChatHistory()
        {
            _receivedMessages.Clear();
            NotifyOfPropertyChange(() => ReceivedMessages);
            _chatHistory.DeleteChatHistoryForUser(_handler.MyJidUser(), _jid.User, false);
        }

        /// <summary>
        /// Handles the specified message (event).
        /// </summary>
        /// <param name="message">The message.</param>
        public void Handle(ClearChatHistoryEvent message)
        {
            Debug.WriteLine("[EVENT RECEIVED]: Received ClearChatHistoryEvent from the buddylist itself.");
            _receivedMessages.Clear();
            NotifyOfPropertyChange(() => ReceivedMessages);
        }

        /// <summary>
        /// Displays the incoming message.
        /// </summary>
        /// <param name="msg">The message.</param>
        public async Task MessageIncoming(agsXMPP.protocol.client.Message msg)
        {
            var fromUser = msg.From.User.ToLowerInvariant();
            if (IsMessageInvite(msg.Body))
            {
                var sound = ChatSoundTypes.InvitationSound;
                _handler.PlayMessageSound(sound);
                IncomingMessage = "" + fromUser + " has invited you to match!" + "\n";
                ReceivedMessages = "[" + DateTime.Now.ToShortTimeString() + "] " + fromUser + " has invited you to match!";
                // Get server info for the invitation popup
                // TODO: Some kind of flood limiting thing so people can't spam this invite feature.
                string inviteId = Regex.Match(msg.Body, @"\d{6,}").Groups[0].Value;
                var server = await RetrieveInvitationGameServerAsync(inviteId);

                //// Show invitation popup.
                dynamic settings = new ExpandoObject();
                settings.Topmost = true;
                settings.WindowStartupLocation = WindowStartupLocation.Manual;
                Execute.OnUIThread(() => _windowManager.ShowWindow(new GameInvitationViewModel(server, fromUser), null, settings));
            }
            else
            {
                var sound = ChatSoundTypes.MessageSound;
                _handler.PlayMessageSound(sound);
                IncomingMessage = msg.Body + "\n";
                ReceivedMessages = "[" + DateTime.Now.ToShortTimeString() + "] " + fromUser + ": " + msg.Body;
            }
            // Log the message
            _chatHistory.AddMessageToHistoryDb(_handler.MyJidUser(), _jid.User, ChatMessageTypes.Incoming, IncomingMessage, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            IncomingMessage = string.Empty;
        }

        /// <summary>
        /// Removes this chat the active chats.
        /// </summary>
        /// <remarks>This is called from the view itself (when the view closes).</remarks>
        public void RemoveActiveChat()
        {
            ChatHandler.ActiveChats.Remove(_jid.Bare.ToLowerInvariant());
            _xmppCon.MessageGrabber.Remove(_jid);

            // in the case where the manual string jid was added from the ChatListViewModel:
            _xmppCon.MessageGrabber.Remove(_jid.Bare);
            Debug.WriteLine("Window closed. Removed active chat from user: " + _jid.Bare.ToLowerInvariant() + " Current active chat count: " + ChatHandler.ActiveChats.Count);
        }

        /// <summary>
        /// Retrieves the game invitation server information.
        /// </summary>
        public async Task<ServerDetailsViewModel> RetrieveInvitationGameServerAsync(string serverId)
        {
            Debug.WriteLine("--> Got invitation. Server id is: " + serverId);
            var gzipHandler = new HttpClientHandler();
            var client = new HttpClient(gzipHandler);
            ServerDetailsViewModel server = null;

            try
            {
                // QL site sends gzip compressed responses
                if (gzipHandler.SupportsAutomaticDecompression)
                {
                    gzipHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                }

                client.DefaultRequestHeaders.Add("User-Agent", UQltGlobals.QlUserAgent);
                var response = await client.GetAsync(UQltGlobals.QlDomainDetailsIds + serverId);
                response.EnsureSuccessStatusCode();

                // QL site actually doesn't send "application/json", but "text/html" even though it
                // is actually JSON HtmlDecode replaces &gt;, &lt; same as quakelive.js's EscapeHTML function

                string json = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
                // QL API returns an array, even for individual servers as in this case
                var qlservers = JsonConvert.DeserializeObject<List<Server>>(json);

                // Create the Server (ServerDetailsViewModel) object for the player
                foreach (var qlserver in qlservers)
                {
                    server = new ServerDetailsViewModel(qlserver);
                }

                return server;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <remarks>This is called from the view itself.</remarks>
        public void SendMessage(string message)
        {
            if (message.Length == 0) { return; }

            if (_handler.SendMessage(_jid, message))
            {
                OutgoingMessage = message + "\n";

                Debug.WriteLine("Attempting to send recipient: " + _jid + " message: " + message);
                ReceivedMessages = "[" + DateTime.Now.ToShortTimeString() + "] " + _handler.MyJidUser() + ": " + message;

                // Log the message
                _chatHistory.AddMessageToHistoryDb(_handler.MyJidUser(), _jid.User, ChatMessageTypes.Outgoing, OutgoingMessage, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                OutgoingMessage = string.Empty;
            }
            else
            {
                ReceivedMessages = "" + _jid.User + " is offline. Message was not sent.\n";
                OutgoingMessage = string.Empty;
            }
        }

        /// <summary>
        /// Determines whether the incoming message is an invitation to a game
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns><c>true</c> if the message is an invite, otherwise <c>false</c></returns>
        private bool IsMessageInvite(string message)
        {
            return (message.StartsWith(_handler.StrInvite));
        }

        /// <summary>
        /// This is called when a message is received within the view associated with this viewmodel.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="msg">The message.</param>
        /// <param name="data">The data.</param>
        /// <remarks>This is only called when the window is actually open.</remarks>
        private void MessageCallback(object sender, agsXMPP.protocol.client.Message msg, object data)
        {
            if (msg.Body == null) { return; }

            Debug.WriteLine("Incoming message body is: " + msg.Body);
            // Async: suppress warning - http://msdn.microsoft.com/en-us/library/hh965065.aspx
            var m = MessageIncoming(msg);
        }
    }
}