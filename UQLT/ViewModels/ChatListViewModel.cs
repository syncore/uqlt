using System;
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

namespace UQLT.ViewModels
{
    [Export(typeof(ChatListViewModel))]
    public class ChatListViewModel : PropertyChangedBase
    {
        
        private XmppClientConnection XmppCon;
        
        private Dictionary<string, string> _friends;

        public Dictionary<string, string> Friends
        {
            get
            {
                return _friends;
            }

            set
            {
                _friends = value;
                NotifyOfPropertyChange(() => Friends);
            }
        }
        
        [ImportingConstructor]
        public ChatListViewModel()
        {
            _friends = new Dictionary<string, string>();
            XmppCon = new XmppClientConnection(UQLTGlobals.QLXMPPDomain);
            XmppCon.OnRosterItem += new XmppClientConnection.RosterHandler(XmppCon_OnRosterItem);
            XmppCon.OnLogin += new ObjectHandler(XmppCon_OnLogin);
            XmppCon.Open(***REMOVED***, ***REMOVED***);
            XmppCon.ClientSocket.OnValidateCertificate += new RemoteCertificateValidationCallback(ClientSocket_OnValidateCertificate);
        }

        private void XmppCon_OnRosterItem(object sender, RosterItem item)
        {

            
            try 
            { 
            Friends.Add(item.Jid.Bare.ToString(), item.GetAttribute("name").ToString());
            XmppCon.MessageGrabber.Add(new Jid(item.Jid.Bare.ToString()), new BareJidComparer(), new MessageCB(XmppCon_OnMessage), null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                //MessageBox.Show(ex.ToString());
            }
            
          
        }

        private void XmppCon_OnMessage(object sender, agsXMPP.protocol.client.Message msg, object data)
        {
            if (msg.Body != null)
            {
                MessageBox.Show(Friends[msg.From.Bare.ToString()] + ": " + msg.Body);
            }
        }

        private void XmppCon_OnLogin(object sender)
        {
            Presence p = new Presence(ShowType.chat, "");
            p.Type = PresenceType.available;
            XmppCon.Send(p);
            //XmppCon.OnRosterItem += new XmppClientConnection.RosterHandler(XmppCon_OnRosterItem);
        }

        private bool ClientSocket_OnValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

     

    }
}
