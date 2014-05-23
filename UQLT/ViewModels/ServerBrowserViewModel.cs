using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using Caliburn.Micro;
using Newtonsoft.Json;
using UQLT;
using UQLT.Events;
using UQLT.Models;
using UQLT.Models.Filters.Remote;
using UQLT.Models.QLRanks;
using UQLT.Models.QuakeLiveAPI;
using UQLT.Models.Filters.User;
using UQLT.Core.ServerBrowser;
using System.Net;
using System.Windows;


namespace UQLT.ViewModels
{
    [Export(typeof(ServerBrowserViewModel))]

    // ViewModel for Server Browser view
    public class ServerBrowserViewModel : PropertyChangedBase, IHandle<ServerRequestEvent>
    {
        private ServerBrowser SB;

        private ObservableCollection<ServerDetailsViewModel> _servers;

        public ObservableCollection<ServerDetailsViewModel> Servers
        {
            get
            {
                return _servers;
            }

            set
            {
                _servers = value;
                NotifyOfPropertyChange(() => Servers);
            }
        }

        private ServerDetailsViewModel _selectedServer;

        public ServerDetailsViewModel SelectedServer
        {
            get
            {
                return _selectedServer;
            }

            set
            {
                _selectedServer = value;
                NotifyOfPropertyChange(() => SelectedServer);
            }
        }



        private string _filterURL;

        public string FilterURL
        {
            get
            {
                return _filterURL;
            }

            set
            {
                _filterURL = value + Math.Truncate((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds);
                NotifyOfPropertyChange(() => FilterURL);
            }
        }


        private bool _isUpdatingServers;
        public bool IsUpdatingServers
        {
            get
            {
                return _isUpdatingServers;
            }
            set
            {
                _isUpdatingServers = value;
                NotifyOfPropertyChange(() => IsUpdatingServers);
            }
        }

        private int _numberOfServersToUpdate;
        public int NumberOfServersToUpdate
        {
            get
            {
                return _numberOfServersToUpdate;
            }
            set
            {
                _numberOfServersToUpdate = value;
                NotifyOfPropertyChange(() => NumberOfServersToUpdate);
            }
        }

        private int _numberOfPlayersToUpdate;
        public int NumberOfPlayersToUpdate
        {
            get
            {
                return _numberOfPlayersToUpdate;
            }
            set
            {
                _numberOfPlayersToUpdate = value;
                NotifyOfPropertyChange(() => NumberOfPlayersToUpdate);
            }
        }

        private string _serverUpdatingProgressText;
        public string ServerUpdatingProgressText
        {
            get
            {
                return _serverUpdatingProgressText;
            }
            set
            {
                _serverUpdatingProgressText = value;
                NotifyOfPropertyChange(() => ServerUpdatingProgressText);
            }
        }
        

        [ImportingConstructor]
        public ServerBrowserViewModel(IEventAggregator events)
        {
            events.Subscribe(this);
            _servers = new ObservableCollection<ServerDetailsViewModel>();
            DoServerBrowserAutoSort("FullLocationName");
            // Instantiate a new server browser for this viewmodel
            SB = new ServerBrowser(this);

        }

        // This is fired whenever we receive a new default filter, either through the "make new default" button or "reset filters" button.
        public void Handle(ServerRequestEvent message)
        {
            FilterURL = message.ServerRequestURL;
            var l = SB.LoadServerListAsync(FilterURL);
            Debug.WriteLine("[EVENT RECEIVED] Filter URL Change: " + message.ServerRequestURL);
        }

        // Sort the server browser based on specified criteria
        private void DoServerBrowserAutoSort(string property)
        {
            var view = CollectionViewSource.GetDefaultView(Servers);
            var sortDescription = new SortDescription(property, ListSortDirection.Ascending);
            view.SortDescriptions.Add(sortDescription);
        }


    }
}