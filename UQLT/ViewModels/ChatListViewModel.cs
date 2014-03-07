﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Caliburn.Micro;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.roster;
using agsXMPP.Collections;
using System.Windows;
using agsXMPP.Xml.Dom;

namespace UQLT.ViewModels
{
    [Export(typeof(ChatListViewModel))]
    public class ChatListViewModel : PropertyChangedBase
    {

        private XmppClientConnection XmppCon;

        private Dictionary<string, string> _roster;

        public Dictionary<string, string> Roster
        {
            get
            {
                return _roster;
            }

            set
            {
                _roster = value;
                NotifyOfPropertyChange(() => Roster);
            }
        }

        private Dictionary<string, string> _onlineFriends;

        public Dictionary<string, string> OnlineFriends
        {
            get
            {
                return _onlineFriends;
            }

            set
            {
                _onlineFriends = value;
                NotifyOfPropertyChange(() => OnlineFriends);
            }
        }

        [ImportingConstructor]
        public ChatListViewModel()
        {
            _roster = new Dictionary<string, string>();
            _onlineFriends = new Dictionary<string, string>();
            XmppCon = new XmppClientConnection();

            // XmppClientConnection event handlers
            XmppCon.OnLogin += new ObjectHandler(XmppCon_OnLogin);
            XmppCon.OnRosterItem += new XmppClientConnection.RosterHandler(XmppCon_OnRosterItem);
            // TODO: will probably need an OnRosterEnd event when Roster is fully loaded
            XmppCon.OnPresence += new PresenceHandler(XmppCon_OnPresence);
            XmppCon.ClientSocket.OnValidateCertificate += new RemoteCertificateValidationCallback(ClientSocket_OnValidateCertificate);

            ConnectToXMPP();
        }

        private void ConnectToXMPP()
        {
            XmppCon.Username = ***REMOVED***;
            XmppCon.Password = ***REMOVED***;
            XmppCon.Server = UQLTGlobals.QLXMPPDomain;
            XmppCon.Port = 5222;
            XmppCon.Resource = "quakelive";
            XmppCon.AutoRoster = true;
            XmppCon.Open();
        }

        private void XmppCon_OnRosterItem(object sender, RosterItem item)
        {
            try
            {
                Roster.Add(item.Jid.Bare.ToLowerInvariant(), item.Jid.User.ToLowerInvariant());
                XmppCon.MessageGrabber.Add(new Jid(item.Jid.Bare.ToLowerInvariant()), new BareJidComparer(), new MessageCB(XmppCon_OnMessage), null);
                /*
                    foreach (var x in Roster)
                {
                    Console.WriteLine("Roster: " + x.ToString());
                }
                */
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                //MessageBox.Show(ex.ToString());
            }


        }

        // We've received a message.
        private void XmppCon_OnMessage(object sender, agsXMPP.protocol.client.Message msg, object data)
        {
            if (msg.Body != null)
            {
                MessageBox.Show(Roster[msg.From.Bare.ToLowerInvariant()] + ": " + msg.Body);
            }
        }

        // We have successfully authenticated to the server.
        private void XmppCon_OnLogin(object sender)
        {
            Presence p = new Presence(ShowType.chat, "");
            p.Type = PresenceType.available;
            XmppCon.Send(p);

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

        private void FriendBecameAvailable(Presence pres)
        {
            if ((!pres.From.Bare.Equals(XmppCon.MyJID.Bare.ToLowerInvariant()) && (!OnlineFriends.ContainsKey(pres.From.Bare.ToLowerInvariant()))))
            {
                Console.WriteLine("[FRIEND AVAILABLE]: " + pres.From.Bare);
                Console.WriteLine("Friends list before adding " + pres.From.Bare + "," + " count: " + OnlineFriends.Count());
                OnlineFriends.Add(pres.From.Bare.ToLowerInvariant(), pres.From.User.ToLowerInvariant());
                Console.WriteLine("Friends list after adding " + pres.From.Bare + "," + " count: " + OnlineFriends.Count());
            }
        }

        private void FriendBecameUnavailble(Presence pres)
        {
            if ((!pres.From.Bare.Equals(XmppCon.MyJID.Bare.ToLowerInvariant()) && (OnlineFriends.ContainsKey(pres.From.Bare.ToLowerInvariant()))))
            {
                Console.WriteLine("[FRIEND UNAVAILABLE]: " + pres.From.Bare);
                Console.WriteLine("Friends list before removing " + pres.From.Bare + ", " + OnlineFriends.ToArray() + "," + " count: " + OnlineFriends.Count());
                OnlineFriends.Remove(pres.From.Bare.ToLowerInvariant());
                //XmppCon.MessageGrabber.Remove(pres.From.Bare.ToLowerInvariant());
                Console.WriteLine("Friends list after removing " + pres.From.Bare + ", " + OnlineFriends.ToArray() + "," + " count: " + OnlineFriends.Count());
            }
        }

        private bool ClientSocket_OnValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }



    }
}
