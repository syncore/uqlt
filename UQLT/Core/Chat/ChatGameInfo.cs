using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.Timers;
using UQLT.Helpers;
using UQLT.Models.QuakeLiveAPI;
using UQLT.ViewModels;

namespace UQLT.Core.Chat
{
    /// <summary>
    /// Class responsible for handling the game server information for players on the buddy list
    /// </summary>
    public class ChatGameInfo
    {
        private readonly Timer _gameServerUpdateTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatGameInfo" /> class.
        /// </summary>
        /// <param name="handler">A chat handler class.</param>
        public ChatGameInfo(ChatHandler handler)
        {
            Handler = handler;
            _gameServerUpdateTimer = new Timer();
        }

        /// <summary>
        /// Gets the chat handler associated with this class.
        /// </summary>
        /// <value>The chat handler.</value>
        public ChatHandler Handler
        {
            get;
            private set;
        }

        /// <summary>
        /// Asynchronously creates the server information for status.
        /// </summary>
        /// <param name="friend">The friend.</param>
        /// <param name="serverId">The server id.</param>
        /// <remarks>
        /// This is used for an initial one-time creation of a Server object, which is a <see cref="ChatGameInfoViewModel"/>
        /// object for an in-game friend on the friend list
        /// </remarks>
        public async Task CreateServerInfoForStatusAsync(string friend, string serverId)
        {
            string url = UQltGlobals.QlDomainDetailsIds + serverId;
            try
            {
                var query = new RestApiQuery();
                var serverdata = await (query.QueryRestApiAsync<List<Server>>(url));

                // Create the Server (SrvDetailsBuddyViewModel) object for the player
                //foreach (var server in serverdata)
                //{
                    Handler.Clvm.OnlineGroup.Friends[friend.ToLowerInvariant()].Server = new ChatGameInfoViewModel(serverdata[0]);
                //}
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                // need something here so it re-tries again in X seconds on failure
            }
        }

        /// <summary>
        /// Starts the game server update timer for buddy list contacts.
        /// </summary>
        public void StartServerUpdateTimer()
        {
            _gameServerUpdateTimer.Elapsed += OnTimedServerInfoUpdate;
            _gameServerUpdateTimer.Interval = 75000;
            _gameServerUpdateTimer.Enabled = true;
            _gameServerUpdateTimer.AutoReset = true;
        }

        /// <summary>
        /// Asynchronously manually updates the server information for status.
        /// </summary>
        /// <param name="friend">The friend.</param>
        /// <remarks>
        /// This is used for subsequent updates of a single in-game friend's game server information
        /// (i.e. when a user clicks a friend on his friend list)
        /// </remarks>
        public async Task UpdateServerInfoForStatusAsync(string friend)
        {
            try
            {
                // server_id (i.e. PublicId) should have already been set on the initial creation of
                // the Server (SrvDetailsBuddyViewModel) object
                string serverId = Handler.Clvm.OnlineGroup.Friends[friend.ToLowerInvariant()].Server.PublicId.ToString(CultureInfo.InvariantCulture);
                string url = UQltGlobals.QlDomainDetailsIds + serverId;

                var query = new RestApiQuery();
                var serverdata = await (query.QueryRestApiAsync<List<Server>>(url));

                Handler.Clvm.OnlineGroup.Friends[friend.ToLowerInvariant()].Server = new ChatGameInfoViewModel(serverdata[0]);
                    
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Called when game server update timer has elapsed.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs" /> instance containing the event data.</param>
        private void OnTimedServerInfoUpdate(object source, ElapsedEventArgs e)
        {
            var ingamefriends = new List<string>();

            foreach (KeyValuePair<string, FriendViewModel> kvp in Handler.Clvm.OnlineGroup.Friends)
            {
                if (kvp.Value.IsInGame)
                {
                    ingamefriends.Add(kvp.Key);
                }
                else
                {
                    Debug.WriteLine("" + kvp.Key + " is not in a game server. Skipping...");
                }
            }
            Debug.WriteLine("Processing batch game server update for " + ingamefriends.Count + " players: " + string.Join(",", ingamefriends));

            // Batch process these in game friends
            if (ingamefriends.Count != 0)
            {
                // Async: suppress warning - http://msdn.microsoft.com/en-us/library/hh965065.aspx
                var u = UpdateServerInfoForStatusAsync(ingamefriends);
            }
        }

        /// <summary>
        /// Stops the game server update timer for buddy list contacts.
        /// </summary>
        /// <remarks>This is called from the view itself.</remarks>
        private void StopServerUpdateTimer()
        {
            // TODO: stop timer if we have launched a game, to prevent lag during game
            _gameServerUpdateTimer.Enabled = false;
        }

        /// <summary>
        /// Asynchronously performs batch updates of in-game buddies' server information when called
        /// by the game server timer.
        /// </summary>
        /// <param name="ingamefriends">The list of in-game friends on the buddy list.</param>
        /// <remarks>
        /// This will receive a list of all of the players on friends list who are currently on a
        /// game server. It will then extract the public_id for each and send a concatenated list of
        /// ids to QL API in one pass. Then it individually update the friends' game server
        /// information from whatever is received from QL API This was created to avoid having
        /// multiple HTTP GET requests for every single in-game friend on the list.
        /// </remarks>
        private async Task UpdateServerInfoForStatusAsync(IReadOnlyList<string> ingamefriends)
        {
            // Get the server ids (public_id)s of all in-game players to send to QL API
            var serverIds = new List<string>();

            for (int i = 0; i < ingamefriends.Count; i++)
            {
                serverIds.Add(Handler.Clvm.OnlineGroup.Friends[ingamefriends[i]].Server.PublicId.ToString(CultureInfo.InvariantCulture));
            }

            try
            {
                string url = UQltGlobals.QlDomainDetailsIds + string.Join(",", serverIds);
                var query = new RestApiQuery();
                var servers = await (query.QueryRestApiAsync<List<Server>>(url));

                // set the player info for status
                for (int i = 0; i < ingamefriends.Count; i++)
                {
                    Handler.Clvm.OnlineGroup.Friends[ingamefriends[i].ToLowerInvariant()].Server = new ChatGameInfoViewModel(servers[i]);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}