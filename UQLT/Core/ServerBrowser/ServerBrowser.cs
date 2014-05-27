using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using UQLT.Models;
using UQLT.Models.Filters.Remote;
using UQLT.Models.Filters.User;
using UQLT.Models.QLRanks;
using UQLT.Models.QuakeLiveAPI;
using UQLT.ViewModels;
using UQLT.Interfaces;
using System.Timers;
using Caliburn.Micro;

namespace UQLT.Core.ServerBrowser
{

	/// <summary>
	/// Helper class responsible for server retrieval, pinging servers, and elo details for a ServerBrowserViewModel
	/// </summary>
	public class ServerBrowser : IQLRanksUpdater
	{
		private Regex port = new Regex(@"[\:]\d{4,}"); // port regexp: colon with at least 4 numbers
		private Timer ServerRefreshTimer;
		private int timesqlrankscompleted = 0;

		public ServerBrowserViewModel SBVM
		{
			get;
			private set;
		}

		public ServerBrowser(ServerBrowserViewModel sbvm)
		{
			SBVM = sbvm;
			SBVM.FilterURL = GetFilterUrlOnLoad();
			ServerRefreshTimer = new Timer();
			if (SBVM.IsAutoRefreshEnabled)
			{
				StartServerRefreshTimer();
			}
			// Don't hit QL servers (debugging)
			var l = LoadServerListAsync(SBVM.FilterURL);
		}

		public void StartServerRefreshTimer()
		{
			ServerRefreshTimer.Elapsed += new ElapsedEventHandler(OnServerRefresh);
			ServerRefreshTimer.Interval = (SBVM.AutoRefreshSeconds * 1000);
			ServerRefreshTimer.Enabled = true;
			ServerRefreshTimer.AutoReset = true;
		}

		public void StopServerRefreshTimer()
		{
			//TODO: stop timer if we have launched a game, to prevent lag during game
			ServerRefreshTimer.Enabled = false;
		}

		// This method is called every X seconds by the ServerRefreshTimer
		private void OnServerRefresh(object source, ElapsedEventArgs e)
		{
			// Don't want to hit QLRanks API for every single server refresh since that API is frequently slow
			bool doqlranksupdate = true;

			if (timesqlrankscompleted >= 3)
			{
				doqlranksupdate = false;
			}

			Debug.WriteLine("Performing automatic server refresh...");
			var l = LoadServerListAsync(SBVM.FilterURL, doqlranksupdate);

		}

		private string GetFilterUrlOnLoad()
		{
			string url = null;
			if (File.Exists(UQLTGlobals.SavedUserFilterPath))
			{
				try
				{
					using(StreamReader sr = new StreamReader(UQLTGlobals.SavedUserFilterPath))
					{
						string saved = sr.ReadToEnd();
						SavedFilters savedFilterJson = JsonConvert.DeserializeObject<SavedFilters>(saved);
						url = UQLTGlobals.QLDomainListFilter + savedFilterJson.fltr_enc;
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Unable to retrieve filter url: " + ex);
					url = UQLTGlobals.QLDefaultFilter;
				}
			}
			else
			{
				url = UQLTGlobals.QLDefaultFilter;
			}
			return url;
		}

		public async Task LoadServerListAsync(string filterurl, bool doqlranksupdate = true)
		{
			filterurl = SBVM.FilterURL;

			SBVM.IsUpdatingServers = true;

			string detailsurl = await MakeDetailsUrlAsync(filterurl);

			IList<Server> servers = await GetServersFromDetailsUrlAsync(detailsurl);

			//SBVM.NumberOfServersToUpdate = servers.Count;

			// Must be done on the UI thread
			Execute.OnUIThread(() =>
			{
				SBVM.Servers.Clear();
				if (servers != null)
				{
					//SBVM.Servers.Clear();
					foreach (var server in servers)
					{
						SBVM.Servers.Add(new ServerDetailsViewModel(server));
					}
				}
			});

			SBVM.IsUpdatingServers = false;
			//SBVM.NumberOfServersToUpdate = 0;
			//SBVM.NumberOfPlayersToUpdate = 0;

			if (doqlranksupdate)
			{
				var g = GetQLRanksPlayersAsync(servers);
			}
		}

		// Get the list of server ids for a given filter, then return a nicely formatted details url

		private async Task<string> MakeDetailsUrlAsync(string url)
		{
			url = SBVM.FilterURL;
			List<string> ids = new List<string>();
			HttpClientHandler gzipHandler = new HttpClientHandler();
			HttpClient client = new HttpClient(gzipHandler);

			try
			{

				// QL site sends gzip compressed responses
				if (gzipHandler.SupportsAutomaticDecompression)
				{
					gzipHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
				}

				client.DefaultRequestHeaders.Add("User-Agent", UQLTGlobals.QLUserAgent);
				HttpResponseMessage response = await client.GetAsync(url);
				response.EnsureSuccessStatusCode(); // Throw on error code

				// TODO: Parse server ids from string as a stream, since its frequently larger than 85kb
				// QL site actually doesn't send "application/json", but "text/html" even though it is actually JSON
				// HtmlDecode replaces &gt;, &lt; same as quakelive.js's EscapeHTML function
				string serverfilterjson = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

				QLAPIFilterObject qlfilter = JsonConvert.DeserializeObject<QLAPIFilterObject>(serverfilterjson);

				foreach (QLAPIFilterServer qfs in qlfilter.servers)
				{
					ids.Add(qfs.public_id.ToString());
				}

				Debug.WriteLine("Formatted details URL: " + UQLTGlobals.QLDomainDetailsIds + string.Join(",", ids));
				return UQLTGlobals.QLDomainDetailsIds + string.Join(",", ids);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				MessageBox.Show("Unable to load Quake Live server data. Refresh and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return null;
			}

		}

		// Get the actual server details for the list of servers based on the server ids

		private async Task<IList<Server>> GetServersFromDetailsUrlAsync(string url)
		{
			HttpClientHandler gzipHandler = new HttpClientHandler();
			HttpClient client = new HttpClient(gzipHandler);

			try
			{
				// 1.json, 2.json, bigtest.json, hugetest.json, hugetest2.json

				// QL site sends gzip compressed responses
				if (gzipHandler.SupportsAutomaticDecompression)
				{
					gzipHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
				}

				UQLTGlobals.IPAddressDict.Clear();
				client.DefaultRequestHeaders.Add("User-Agent", UQLTGlobals.QLUserAgent);
				HttpResponseMessage response = await client.GetAsync(url);
				response.EnsureSuccessStatusCode(); // Throw on error code

				// QL site actually doesn't send "application/json", but "text/html" even though it is actually JSON
				// HtmlDecode replaces &gt;, &lt; same as quakelive.js's EscapeHTML function
				string serverdetailsjson = System.Net.WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
				ObservableCollection<Server> serverlist = JsonConvert.DeserializeObject<ObservableCollection<Server>>(serverdetailsjson);
				List<string> addresses = new List<string>();

				foreach (Server s in serverlist)
				{
					string cleanedip = port.Replace(s.host_address, string.Empty);
					addresses.Add(cleanedip);
					UQLTGlobals.IPAddressDict[cleanedip] = 0; // initially set ping at 0

					// set a custom property for game_type for each server's players
					s.setPlayerGameTypeFromServer(s.game_type);

					// create EloData for each player in the given server
					s.createEloData();

					// Elo "caching" - otherwise elo will be 0. TODO: come up with something better than this ugly hack
					if (timesqlrankscompleted >= 3)
					{
						foreach (var player in s.players)
						{
							s.setPlayerElos();
						}
					}

				}

				List<Task<PingReply>> pingTasks = new List<Task<PingReply>>();

				foreach (string address in addresses)
				{
					pingTasks.Add(PingAsync(address));
				}

				// wait for all the tasks to complete
				await Task.WhenAll(pingTasks.ToArray());

				// iterate over list of pingTasks
				foreach (var pingTask in pingTasks)
				{
					UQLTGlobals.IPAddressDict[pingTask.Result.Address.ToString()] = pingTask.Result.RoundtripTime;
					// Debug.WriteLine("IP Address: " + pingTask.Result.Address + " time: " + pingTask.Result.RoundtripTime + " ms ");
				}

				return serverlist;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex); // TODO: create a debug log on the disk
				MessageBox.Show("Unable to load Quake Live server data. Refresh and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return null;
			}
		}

		//timer

		public void SetQLranksDefaultElo(string player)
		{
			UQLTGlobals.PlayerEloInfo[player] = new EloData()
			{
				DuelElo = 0,
				CaElo = 0,
				TdmElo = 0,
				FfaElo = 0,
				CtfElo = 0
			};
		}

		// Extract the players from the server list in order to send to QLRanks API

		public async Task GetQLRanksPlayersAsync(IList<Server> servers, int maxPlayers = 150)
		{
			List<string> playerstoupdate = new List<string>();
			List<List<string>> qlrapicalls = new List<List<string>>();

			// extract players, add to a list to update, split the list, then update
			foreach (var server in servers)
			{
				foreach (var player in server.players)
				{
					// add to a list of players to be updated
					playerstoupdate.Add(player.name.ToLower());
				}

			}

			//SBVM.NumberOfPlayersToUpdate = playerstoupdate.Count;

			// split servers
			for (int i = 0; i < playerstoupdate.Count; i += maxPlayers)
			{
				qlrapicalls.Add(playerstoupdate.GetRange(i, Math.Min(maxPlayers, playerstoupdate.Count - i)));
				Debug.WriteLine("QLRANKS: API Call Index: " + i);
			}

			// perform the tasks
			List<Task<QLRanks>> qlranksTasks = new List<Task<QLRanks>>();

			for (int i = 0; i < qlrapicalls.Count; i++)
			{
				qlranksTasks.Add(GetEloDataFromQLRanksAPIAsync(string.Join("+", qlrapicalls[i])));
				Debug.WriteLine("QLRANKS: API Task " + i + " URL: http://www.qlranks.com/api.aspx?nick=" + string.Join("+", qlrapicalls[i]));
			}

			// all the combined n API calls must finish
			await Task.WhenAll(qlranksTasks.ToArray());

			// set the player elos
			try
			{

				foreach (var qlrt in qlranksTasks)
				{
					foreach (QLRanksPlayer qp in qlrt.Result.players)
					{
						UQLTGlobals.PlayerEloInfo[qp.nick.ToLower()].DuelElo = qp.duel.elo;
						UQLTGlobals.PlayerEloInfo[qp.nick.ToLower()].CaElo = qp.ca.elo;
						UQLTGlobals.PlayerEloInfo[qp.nick.ToLower()].TdmElo = qp.tdm.elo;
						UQLTGlobals.PlayerEloInfo[qp.nick.ToLower()].FfaElo = qp.ffa.elo;
						UQLTGlobals.PlayerEloInfo[qp.nick.ToLower()].CtfElo = qp.ctf.elo;
					}
				}
				// Player elos have been set in dictionary, now set on the Player object itself
				// TODO: this will allow using Properties and NotifyPropertyChange to update the player view in the server browser.
				foreach (var s in servers)
				{
					foreach (var player in s.players)
					{
						s.setPlayerElos();
					}
				}

				// Important counter for elo "caching"
				timesqlrankscompleted++;
			}
			catch (Exception e)
			{
				MessageBox.Show("Unable to load QLRanks player data. Refresh and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				Debug.WriteLine(e);
			}
		}

		// Make the actual HTTP GET request to the QLRanks API

		public async Task<QLRanks> GetEloDataFromQLRanksAPIAsync(string players)
		{
			HttpClientHandler gzipHandler = new HttpClientHandler();
			HttpClient client = new HttpClient(gzipHandler);

			try
			{
				client.BaseAddress = new Uri("http://www.qlranks.com");
				//client.BaseAddress = new Uri("http://10.0.0.7");
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				client.DefaultRequestHeaders.Add("User-Agent", UQLTGlobals.QLUserAgent);

				HttpResponseMessage response = await client.GetAsync("/api.aspx?nick=" + players);
				response.EnsureSuccessStatusCode(); // Throw on error code
				string eloinfojson = await response.Content.ReadAsStringAsync();

				QLRanks qlr = JsonConvert.DeserializeObject<QLRanks>(eloinfojson);

				return qlr;
			}
			catch (Newtonsoft.Json.JsonException jEx)
			{
				// This exception indicates a problem deserializing the request body.
				Debug.WriteLine(jEx.Message);
				return null;

				// MessageBox.Show(jEx.Message);
			}
			catch (HttpRequestException ex)
			{
				Debug.WriteLine(ex.Message);
				return null;

				// MessageBox.Show(ex.Message);
			}
		}

		private Task<PingReply> PingAsync(string address)
		{
			var tcs = new TaskCompletionSource<PingReply>();
			Ping ping = new Ping();
			ping.PingCompleted += (obj, sender) =>
			{
				tcs.SetResult(sender.Reply);
			};
			ping.SendAsync(address, new object());
			return tcs.Task;
		}

	}
}