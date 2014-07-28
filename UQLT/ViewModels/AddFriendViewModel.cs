using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using agsXMPP;
using Caliburn.Micro;
using HtmlAgilityPack;
using UQLT.Core.Chat;
using Uri = System.Uri;

namespace UQLT.ViewModels
{
    /// <summary>
    /// ViewModel responsible for the "Add a Friend" view, AddAFriendView
    /// </summary>
    [Export(typeof(AddFriendViewModel))]
    public class AddFriendViewModel : PropertyChangedBase, IHaveDisplayName, IViewAware
    {
        private readonly ChatHandler _handler;
        private Window _dialogWindow;
        private string _friendToAdd;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddFriendViewModel"/> class.
        /// </summary>
        [ImportingConstructor]
        public AddFriendViewModel(ChatHandler handler)
        {
            _handler = handler;
            DisplayName = "Add a friend";
        }

        /// <summary>
        /// Raised when a view is attached.
        /// </summary>
        public event EventHandler<ViewAttachedEventArgs> ViewAttached;

        /// <summary>
        /// Gets or sets the name of this window.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets the friend image.
        /// </summary>
        /// <value>The friend image.</value>
        public ImageSource FriendImage
        {
            get
            {
                return new BitmapImage(new Uri("pack://application:,,,/UQLTRes;component/images/chat/friend.gif", UriKind.RelativeOrAbsolute));
            }
        }

        /// <summary>
        /// Gets or sets the user to add to the contact list.
        /// </summary>
        /// <value>
        /// The user to add to the contact list.
        /// </value>
        public string FriendToAdd
        {
            get
            {
                return _friendToAdd;
            }
            set
            {
                _friendToAdd = value;
                NotifyOfPropertyChange(() => FriendToAdd);
            }
        }

        /// <summary>
        /// Adds the friend to the contact list.
        /// </summary>
        public async Task AddFriendAsync()
        {
            if (string.IsNullOrEmpty(FriendToAdd))
            {
                MessageBox.Show("The player name cannot be blank!", "Error: player name cannot be blank!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (FriendToAdd.Equals(_handler.MyJidUser()))
            {
                MessageBox.Show("You cannot add yourself!", "Error: unable to add yourself.", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (ContactAlreadyExists())
            {
                MessageBox.Show(string.Format("A request has already been sent to {0} or {0} is already on your friend list!", FriendToAdd),
                    "Error: contact exists.", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (await IsValidQuakeLiveUserAsync())
            {
                // Manual jid construction. Only the bare jid is needed here.
                var friend = FriendToAdd.ToLowerInvariant();
                var jid = new Jid(friend + "@" + UQltGlobals.QlXmppDomain);
                _handler.AddFriend(jid);

                MessageBox.Show(string.Format("Friend request sent to player {0}", FriendToAdd), "Friend request sent.",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Close the window from VM instead of view so the above success message can be sent.
                CloseWin();
            }
            else
            {
                MessageBox.Show(string.Format("Unable to add '{0}' because that is not a valid Quake Live player!", FriendToAdd), "Error: unable to add: " + FriendToAdd, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Attaches a view to this instance.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="context">The context in which the view appears.</param>
        public void AttachView(object view, object context = null)
        {
            _dialogWindow = view as Window;
            if (ViewAttached != null)
            {
                ViewAttached(this, new ViewAttachedEventArgs()
                {
                    Context = context,
                    View = view
                });
            }
        }

        /// <summary>
        /// Closes the window.
        /// </summary>
        /// <remarks>For different ways to implement window closing, see: http://stackoverflow.com/questions/10090584/how-to-close-dialog-window-from-viewmodel-caliburnwpf
        /// </remarks>
        public void CloseWin()
        {
            _dialogWindow.Close();
        }

        /// <summary>
        /// Gets a view previously attached to this instance.
        /// </summary>
        /// <param name="context">The context denoting which view to retrieve.</param>
        /// <returns>The view.</returns>
        public object GetView(object context = null)
        {
            return _dialogWindow;
        }

        /// <summary>
        /// Checks to see whether the contact to be added already exists.
        /// </summary>
        /// <returns></returns>
        private bool ContactAlreadyExists()
        {
            _handler.ManualRosterUpdateFromServer();
            agsXMPP.protocol.iq.roster.RosterItem val;
            return _handler.CurrentRosterItems.TryGetValue(FriendToAdd.ToLowerInvariant(), out val);
        }

        /// <summary>
        /// Asynchrounously scrapes the Quakelive.com site to determine whether the player to add is a valid player.
        /// </summary>
        /// <returns><c>true</c> if the player was found on QL site, otherwise <c>false</c>.</returns>
        /// <remarks>Unfortunately, this is the best way to do this, since there is no exposed QL API for this.
        /// </remarks>
        private async Task<bool> IsValidQuakeLiveUserAsync()
        {
            var httpClientHandler = new HttpClientHandler();
            var httpClient = new HttpClient(httpClientHandler);
            // TODO: don't forget to change this back to the real QL site:
            //var playerurl = "http://www.quakelive.com/profile/summary/" + FriendToAdd.ToLowerInvariant();
            var playerurl = "http://10.0.0.7/parsetest.html";

            using (httpClient)
            {
                try
                {
                    if (httpClientHandler.SupportsAutomaticDecompression)
                    {
                        httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip |
                                                                    DecompressionMethods.Deflate;
                    }

                    httpClient.DefaultRequestHeaders.Add("User-Agent", UQltGlobals.QlUserAgent);
                    var response = await httpClient.GetAsync(playerurl);
                    response.EnsureSuccessStatusCode();

                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    {
                        using (var sr = new StreamReader(responseStream))
                        {
                            var result = sr.ReadToEnd();
                            var htmlDocument = new HtmlDocument();
                            htmlDocument.LoadHtml(result);
                            // <div class="playername player_nick_light">xxxxxxxxx</div>
                            var div = htmlDocument.DocumentNode.SelectSingleNode("//div[contains(@class,'playername')]");
                            if (div.InnerHtml.Equals("Unknown Player"))
                            {
                                Debug.WriteLine("Player: " + FriendToAdd.ToLowerInvariant() + " NOT FOUND on QL site!");
                                return false;
                            }
                            else
                            {
                                Debug.WriteLine("Player: " + FriendToAdd.ToLowerInvariant() + " FOUND on QL site!");
                                return true;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show(string.Format("Unable to add '{0}' because an error occurred when attempting to access Quake Live server.", FriendToAdd), "Error: unable to access QL server", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
        }
    }
}