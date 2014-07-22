using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using UQLT.ViewModels;

namespace UQLT.Core.ServerBrowser
{
    /// <summary>
    /// Class responsible for handling the pinging of servers.
    /// </summary>
    public class ServerPinger
    {

        /// <summary>
        /// Asynchronously sets the ping information for a single server.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <returns>Nothing</returns>
        /// <remarks>This is used when refreshing an individual server from the <see cref="ServerBrowserViewModel"/></remarks>
        public async Task SetPingInformationAsync(ServerDetailsViewModel server)
        {
            var response = await PingAsync(server.CleanedIp);
            server.Ping = response.RoundtripTime;
        }

        /// <summary>
        /// Asynchronously sets the ping information for a single server.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <returns>Nothing</returns>
        /// <remarks>This is used when refreshing an individual server from the <see cref="ChatListViewModel"/></remarks>
        public async Task SetPingInformationAsync(ChatGameInfoViewModel server)
        {
            var response = await PingAsync(server.CleanedIp);
            server.Ping = response.RoundtripTime;
        }

        /// <summary>
        /// Asynchronously sets the ping information for a list of servers.
        /// </summary>
        /// <param name="servers">The list of servers.</param>
        /// <returns>Nothing.</returns>
        /// /// <remarks>This is used when refreshing a list of servers, either initially, or manually
        /// from the <see cref="ServerBrowserViewModel"/></remarks>
        public async Task SetPingInformationAsync(IEnumerable<ServerDetailsViewModel> servers)
        {
            var ipaddresses = new HashSet<string>();

            var serverDetailsViewModels = servers as ServerDetailsViewModel[] ?? servers.ToArray();
            foreach (var server in serverDetailsViewModels)
            {
                ipaddresses.Add(server.CleanedIp);
            }
            var pingTasks = ipaddresses.Select(PingAsync).ToList();
            await Task.WhenAll(pingTasks.ToArray());

            foreach (var pingTask in pingTasks)
            {
                var task = pingTask;
                foreach (var server in serverDetailsViewModels.Where(server => server.CleanedIp == task.Result.Address.ToString()))
                {
                    server.Ping = pingTask.Result.RoundtripTime;
                }
            }
        }

        /// <summary>
        /// Asynchronously pings a given address.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <returns>The round-trip time.</returns>
        public Task<PingReply> PingAsync(string address)
        {
            var tcs = new TaskCompletionSource<PingReply>();
            var ping = new Ping();
            ping.PingCompleted += (obj, sender) => tcs.SetResult(sender.Reply);
            ping.SendAsync(address, new object());
            return tcs.Task;
        }
    
    }
}
