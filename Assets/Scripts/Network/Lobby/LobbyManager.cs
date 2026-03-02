using System.Collections.Generic;
using System.Net;
using FishNet;
using FishNet.Managing;
using UnityEngine;
using FishNet.Discovery;

namespace Network.Lobby
{
    public class LobbyManager : MonoBehaviour
    {
        [SerializeField] private LobbyUI _lobbyUI;
        
        private NetworkDiscovery _discovery;
        private NetworkManager _networkManager;

        private readonly List<string> _foundServers = new List<string>();

        public bool forceLocalhostForTesting = true;

        private void Awake()
        {
            _networkManager = InstanceFinder.NetworkManager;

            _discovery = _networkManager.GetComponent<NetworkDiscovery>();
            if (_discovery == null) return;

            _discovery.ServerFoundCallback += OnServerFound;

            _lobbyUI.SetLobbyManager(this);
        }

        private void OnEnable()
        {
            if (!_discovery.IsSearching && !_networkManager.IsHostStarted)
            {
                _discovery.SearchForServers();
            }
        }

        private void OnDisable()
        {
            if (_discovery != null)
            {
                _discovery.ServerFoundCallback -= OnServerFound;
            }
        }

        private void OnServerFound(IPEndPoint endPoint)
        {
            string ip = endPoint.Address.ToString();
            string display = $"Partie sur {ip}";

            lock (_foundServers)
            {
                if (!_foundServers.Contains(ip))
                {
                    _foundServers.Add(ip);
                    UpdateLobbyUI();
                }
            }
        }

        public void ReloadList()
        {
            if (_discovery == null) return;

            _foundServers.Clear();
            UpdateLobbyUI();

            if (!_discovery.IsSearching)
            {
                _discovery.SearchForServers();
            }
        }

        private void UpdateLobbyUI()
        {
            _lobbyUI.UpdateUIList(_foundServers);
        }

        public void CreateGame()
        {
            if (_networkManager.IsHostStarted) return;

            if (forceLocalhostForTesting)
            {
                InstanceFinder.TransportManager.Transport.SetClientAddress("127.0.0.1");
            }

            _networkManager.ServerManager.StartConnection();
            _networkManager.ClientManager.StartConnection();

            // Advertise SEULEMENT si on est serveur (host)
            if (_networkManager.IsServerStarted && !_discovery.IsAdvertising)
            {
                _discovery.AdvertiseServer();
            }
        }

        // Pour rejoindre (appelée depuis LobbyInfoUI)
        public void JoinGame(string ip)
        {
            // Le port de connexion FishNet est celui de ton transport (souvent 7770 par défaut)
            // Pas le port discovery (7777 par ex.)
            InstanceFinder.ClientManager.StartConnection(ip);  // Port par défaut
            // Ou avec port explicite : StartConnection(ip, 7770);
        }
    }
}