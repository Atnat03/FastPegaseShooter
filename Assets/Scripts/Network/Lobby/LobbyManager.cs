using System.Collections.Generic;
using System.Net;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using UnityEngine;
using FishNet.Discovery;
using FishNet.Object;
using FishNet.Transporting;

namespace Network.Lobby
{
    public class LobbyManager : MonoBehaviour
    {
        private const int MINI_PLAYER_TO_START = 2;
        
        [SerializeField] private LobbyUI _lobbyUI;
        
        private NetworkDiscovery _discovery;
        private NetworkManager _networkManager;

        private readonly List<string> _foundServers = new List<string>();

        public bool forceLocalhostForTesting = true;
        
        private readonly List<NetworkConnection> _lobbyPlayers = new List<NetworkConnection>();

        private void Awake()
        {
            _networkManager = InstanceFinder.NetworkManager;

            _discovery = _networkManager.GetComponent<NetworkDiscovery>();
            if (_discovery == null) return;

            _discovery.ServerFoundCallback += OnServerFound;
            _networkManager.ServerManager.OnServerConnectionState += OnServerState;
            _networkManager.ServerManager.OnRemoteConnectionState += OnPlayerConnectionState;

            _lobbyUI.SetLobbyManager(this);
        }
        
        private void OnServerState(ServerConnectionStateArgs args)
        {
            Debug.Log("Server state : " + args.ConnectionState);

            if (args.ConnectionState == LocalConnectionState.Started)
            {
                Debug.Log("START ADVERTISE");
                _discovery.AdvertiseServer();
            }
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
                _discovery.ServerFoundCallback -= OnServerFound;

            if (_networkManager != null)
            {
                _networkManager.ServerManager.OnServerConnectionState -= OnServerState;
                _networkManager.ServerManager.OnRemoteConnectionState -= OnPlayerConnectionState;
            }
        }

        private void OnPlayerConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
        {
            if (args.ConnectionState == RemoteConnectionState.Started)
            {
                _lobbyPlayers.Add(conn);
                Debug.Log($"Joueur connecté au lobby : {conn.ClientId}");

                UpdateLobbyUI();

                if (_lobbyPlayers.Count >= MINI_PLAYER_TO_START)
                {
                    StartGame();
                }
            }

            else if (args.ConnectionState == RemoteConnectionState.Stopped)
            {
                _lobbyPlayers.Remove(conn);
                UpdateLobbyUI();
            }
        }

        private void StartGame()
        {
            Debug.Log("Tous les joueurs sont prêts, chargement de la scène de jeu...");

            SceneLoadData scene = new SceneLoadData("Lobby");
            scene.ReplaceScenes = ReplaceOption.All;
            
            _networkManager.SceneManager.LoadGlobalScenes(scene);
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
            
            _lobbyUI.DesactivateUI();
        }

        
        //[ObserversRpc]
        public void JoinGame(string ip)
        {
            InstanceFinder.ClientManager.StartConnection(ip);
        }
    }
}