using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using FishNet;
using MyPrint;
using TMPro;
using FishNet.Managing;
using FishNet.Managing.Scened;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;
    
    [SerializeField] TMP_InputField _playerNameTextField;
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private NetworkSceneLoader _networkSceneLoader;
    
    float _heartBeatTimer;
    [SerializeField] float _refreshUiTImer = 1f;

    Lobby _hostLobby;
    Lobby _joinedLobby;
    
    readonly string _keyGameMode = "GameMode";
    readonly string _keyMap = "Map";
    readonly string _keyPlayerName = "PlayerName";
    readonly string _keyStartGameHostAddress = "HostAddress";
    readonly string _keyStartGamePort = "HostPort";
    readonly string _keyGunId = "0";
    
    float _lobbyUpdateTimer;
    string _playerName;

    private float _elaspedTimeUpdate = 0;
    bool _gameStarting = false;
    float _lobbyListTimer;
    private bool _isAuthenticated = false;

    public Action<List<Player>> OnUpdatePlayerList;
    public Action<bool, int> OnSetLocalReadyPlayer;
    public Action OnJoinLobby;
    public Action OnAllPlayerReady;
    public Action<List<Lobby>> OnLobbyListChanged;
    public Action<int> OnSetGun;

    private void Awake()
    {
        instance = this;
        
        if (_networkManager == null)
            _networkManager = FindObjectOfType<NetworkManager>();
    }

    async void Start()
    {
        InitializationOptions options = new InitializationOptions();
        options.SetEnvironmentName("production");

#if UNITY_EDITOR
        options.SetProfile("editor_" + UnityEngine.Random.Range(0, 10000));
#else
        options.SetProfile("build_" + System.Diagnostics.Process.GetCurrentProcess().Id);
#endif

        await UnityServices.InitializeAsync(options);

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        _isAuthenticated = true;
        Debug.Log($"Auth OK | Profile: {AuthenticationService.Instance.Profile} | PlayerId: {AuthenticationService.Instance.PlayerId}");

        _playerName = "Player " + Random.Range(10, 99);
    }

    private void OnEnable()
    {
        _elaspedTimeUpdate = _refreshUiTImer;
    }

    void Update()
    {
        HandleLobbyHeartBeat();
        HandleLobbyPollForUpdates();
        HandleUpdateUI();
        HandleLobbyListRefresh();
    }

    void HandleLobbyListRefresh()
    {
        if (!_isAuthenticated) return;
        if (_joinedLobby != null) return;

        _lobbyListTimer -= Time.deltaTime;

        if (_lobbyListTimer <= 0f)
        {
            _lobbyListTimer = 8f;
            ListLobbies();
        }
    }

    private void HandleUpdateUI()
    {
        if (_joinedLobby == null) return;
        if (_elaspedTimeUpdate > 0)
        {
            _elaspedTimeUpdate -= Time.deltaTime;

            if (_elaspedTimeUpdate <= 0)
            {
                _elaspedTimeUpdate = _refreshUiTImer;

                if (_joinedLobby == null) return;

                PrintPlayers();

                bool allReady = CheckAllPlayerReady();

                if (allReady && !_gameStarting)
                {
                    Cons.Print("Start game...", ColorConsole.Green);
                    OnAllPlayerReady?.Invoke();
                    _gameStarting = true;
                    StartGame();
                }
            }
        }
    }

    async void HandleLobbyHeartBeat()
    {
        if (_hostLobby != null)
        {
            _heartBeatTimer -= Time.deltaTime;

            if (_heartBeatTimer < 0f)
            {
                float heartbeatTimerMax = 15;
                _heartBeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);
            }
        }
    }

    async void HandleLobbyPollForUpdates()
{
    if (_joinedLobby == null) return;

    _lobbyUpdateTimer -= Time.deltaTime;

    if (_lobbyUpdateTimer < 0f)
    {
        float lobbyUpdateTimerMax = 5f;
        _lobbyUpdateTimer = lobbyUpdateTimerMax;

        try
        {
            Lobby lobby = await LobbyService.Instance.GetLobbyAsync(_joinedLobby.Id);
            _joinedLobby = lobby;
        }
        catch (LobbyServiceException e)
        {
            if (e.Reason == LobbyExceptionReason.RateLimited)
            {
                Debug.LogWarning("Too many requests, slowing down...");
                _lobbyUpdateTimer = 6f;
                return;
            }
            else
            {
                Debug.Log(e);
                return;
            }
        }

        OnUpdatePlayerList?.Invoke(_joinedLobby.Players);

        if (_joinedLobby.Data.ContainsKey(_keyStartGameHostAddress) &&
            _joinedLobby.Data[_keyStartGameHostAddress].Value != "0")
        {
            if (!IsLobbyHost())
            {
                string myPlayerId = AuthenticationService.Instance.PlayerId;
                int myGun = 0;
                string myName = _playerName;

                Player me = _joinedLobby.Players.Find(p => p.Id == myPlayerId);

                if (me != null && me.Data != null)
                {
                    if (me.Data.ContainsKey(_keyPlayerName))
                        myName = me.Data[_keyPlayerName].Value;

                    if (me.Data.ContainsKey(_keyGunId))
                        int.TryParse(me.Data[_keyGunId].Value, out myGun);
                }

                PlayerLocalData localData = PlayerLocalData.Instance;
                if (localData == null)
                {
                    GameObject dataHolder = new GameObject("PlayerLocalData");
                    localData = dataHolder.AddComponent<PlayerLocalData>();
                }

                localData.SetPlayerData(myGun, myName, _joinedLobby.Players.Count);

                string hostAddress = _joinedLobby.Data[_keyStartGameHostAddress].Value;
                string hostPort = _joinedLobby.Data.ContainsKey(_keyStartGamePort) 
                    ? _joinedLobby.Data[_keyStartGamePort].Value 
                    : "7777";

                string mapSceneName = _joinedLobby.Data.ContainsKey(_keyMap) 
                    ? _joinedLobby.Data[_keyMap].Value 
                    : "Map1";

                _joinedLobby = null;

                ConnectAsClientWithScene(hostAddress, ushort.Parse(hostPort), mapSceneName);
                
                Debug.Log($"Connecting to Host: {hostAddress}:{hostPort} | Map: {mapSceneName}");
            }
            else
            {
                _joinedLobby = null;
            }
        }
    }
}

  void ConnectAsClientWithScene(string address, ushort port, string sceneName)
  {
    var transport = _networkManager.TransportManager.Transport as FishNet.Transporting.Tugboat.Tugboat;
      
    if (transport != null)
    {
      transport.SetClientAddress(address);
      transport.SetPort(port);
    }

    _networkManager.ClientManager.StartConnection();

    Debug.Log("Client connecting...");
  }

    public async Task<Lobby> CreateLobby(string lobbyName)
    {
        try
        {
            if (!_isAuthenticated)
            {
                Debug.LogError("Pas encore authentifié !");
                return null;
            }

            int maxPlayers = 12;
            
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            string mapSceneName = System.IO.Path.GetFileNameWithoutExtension(
              SceneUtility.GetScenePathByBuildIndex(currentSceneIndex + 1)
            );           


            CreateLobbyOptions createLobbyOptions = new()
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { _keyGameMode, new DataObject(DataObject.VisibilityOptions.Public, "CaptureTheFlag") },
                    { _keyMap, new DataObject(DataObject.VisibilityOptions.Public, mapSceneName) },
                    { _keyStartGameHostAddress, new DataObject(DataObject.VisibilityOptions.Member, "0") },
                    { _keyStartGamePort, new DataObject(DataObject.VisibilityOptions.Member, "7777") },
                    { "LobbyName", new DataObject(DataObject.VisibilityOptions.Public, lobbyName) },
                    { "LobbyLogo", new DataObject(DataObject.VisibilityOptions.Public, "0") },
                    { "LobbyCode", new DataObject(DataObject.VisibilityOptions.Public, "0") }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            _hostLobby = lobby;
            _joinedLobby = _hostLobby;

            Debug.Log("Lobby Created  Name: " + lobby.Name + "  MaxPlayers: " + maxPlayers + "  Id: " + lobby.Id + "  Code: " + lobby.LobbyCode);
            PrintPlayers(_hostLobby);
            
            OnJoinLobby.Invoke();

            return _joinedLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            throw;
        }
    }


  [ContextMenu("List Lobbies")]
  public void ListLobbiesButton()
  {
    ListLobbies();
  }

  async void ListLobbies()
  {
    try
    {
      QueryLobbiesOptions queryLobbiesOptions = new()
      {
        Count   = 25,
        Filters = new List<QueryFilter>
        {
          new(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
        },
        Order = new List<QueryOrder>
        {
          new(false, QueryOrder.FieldOptions.Created)
        }
      };

      QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);

      Debug.Log("Lobbies found: " + queryResponse.Results.Count);

      OnLobbyListChanged?.Invoke(queryResponse.Results);
    }
    catch (LobbyServiceException e)
    {
      Debug.Log(e);
    }
  }

  public async void JoinLobbyById(string lobbyId)
  {
    try
    {
      if (_joinedLobby != null && _joinedLobby.Id == lobbyId)
      {
        Debug.Log("Already in this lobby locally, skipping join.");
        return;
      }

      JoinLobbyByIdOptions options = new() { Player = GetPlayer() };
      Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);
      _joinedLobby = lobby;

      Debug.Log("Joined Lobby by Id: " + lobbyId);
      OnJoinLobby?.Invoke();
      
      PrintPlayers(lobby);
    }
    catch (LobbyServiceException e)
    {
      if (e.Reason == LobbyExceptionReason.LobbyConflict)
      {
        try
        {
          _joinedLobby = await LobbyService.Instance.GetLobbyAsync(lobbyId);
          OnJoinLobby?.Invoke();
          
          PrintPlayers(_joinedLobby);
        }
        catch (LobbyServiceException inner)
        {
          Debug.Log(inner);
        }
      }
      else
      {
        Debug.Log(e);
      }
    }
  }

  public async void JoinLobbyByCode(string lobbyCode)
  {
    try
    {
      JoinLobbyByCodeOptions joinLobbyByCodeOptions = new()
      {
        Player = GetPlayer()
      };

      Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
      _joinedLobby = lobby;

      Debug.Log("Joined Lobby with code: " + lobbyCode);

      OnJoinLobby?.Invoke();
      
      PrintPlayers(lobby);
    }
    catch (LobbyServiceException e)
    {
      Debug.Log(e);
    }
  }

  public async void QuickJoinLobby()
  {
    try
    {
      if (!_isAuthenticated)
      {
        Debug.LogError("Pas encore authentifié !");
        return;
      }

      Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(
        new QuickJoinLobbyOptions { Player = GetPlayer() });
      _joinedLobby = lobby;
      
      OnJoinLobby?.Invoke();

      PrintPlayers(lobby);
    }
    catch (LobbyServiceException e)
    {
      if (e.Reason == LobbyExceptionReason.NoOpenLobbies)
        Debug.Log("No open lobbies found — show a message to the player.");
      else
        Debug.Log(e);
    }
  }

  Player GetPlayer()
  {
    return new Player
    {
      Data = new Dictionary<string, PlayerDataObject>
      {
        { _keyPlayerName, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _playerName) },
        { _keyGameMode, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "0") },
        { "IsReady", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "0") },
        { _keyGunId, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, "0") }
      }
    };
  }

  [ContextMenu("Print Players")]
  public bool PrintPlayers()
  {
    return PrintPlayers(_joinedLobby);
  }

  bool PrintPlayers(Lobby lobby)
  {
    if (lobby.Players == null || lobby.Players.Count <= 0) return false;

    foreach (Player player in lobby.Players)
    {
      if (player == null) return false;
      Debug.Log("Player Id: " + player.Id + "   Player Name: " + player.Data[_keyPlayerName].Value);
    }

    OnUpdatePlayerList?.Invoke(lobby.Players);
    return true;
  }

  public string GetPlayerName(int playerId)
  {
    return _joinedLobby.Players[playerId].Data[_keyPlayerName].Value;
  }

  public void UpdateLobbyGameModeToHideAndSeek()
  {
    UpdateLobbyGameMode("HideAndSeek");
  }

  async void UpdateLobbyGameMode(string gameMode)
  {
    try
    {
      _hostLobby = await LobbyService.Instance.UpdateLobbyAsync(_hostLobby.Id, new UpdateLobbyOptions
      {
        Data = new Dictionary<string, DataObject>
        {
          { _keyGameMode, new DataObject(DataObject.VisibilityOptions.Public, gameMode) }
        }
      });

      _joinedLobby = _hostLobby;

      PrintPlayers(_hostLobby);
    }
    catch (LobbyServiceException e)
    {
      Debug.Log(e);
    }
  }

  [ContextMenu("Update Player Name")]
  public void UpdatePlayerNameButton()
  {
    UpdatePlayerName(_playerNameTextField.text);
  }

  async void UpdatePlayerName(string newPlayerName)
  {
    try
    {
      _playerName = newPlayerName;
      await LobbyService.Instance.UpdatePlayerAsync(
        _joinedLobby.Id,
        AuthenticationService.Instance.PlayerId,
        new UpdatePlayerOptions
        {
          Data = new Dictionary<string, PlayerDataObject>
          {
            { _keyPlayerName, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, newPlayerName) }
          }
        });
    }
    catch (LobbyServiceException e)
    {
      Debug.Log(e);
    }
  }

  [ContextMenu("Leave Lobby")]
  public async void LeaveLobby()
  {
    try
    {
      await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId);
      Debug.Log("Left Lobby");
    }
    catch (LobbyServiceException e)
    {
      Debug.Log(e);
    }
  }

  [ContextMenu("Kick Player")]
  public async void KickPlayer()
  {
    try
    {
      await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, _joinedLobby.Players[1].Id);
      Debug.Log("Kicked Player");
    }
    catch (LobbyServiceException e)
    {
      Debug.Log(e);
    }
  }

  async void MigrateLobbyHost()
  {
    try
    {
      _hostLobby = await LobbyService.Instance.UpdateLobbyAsync(_hostLobby.Id, new UpdateLobbyOptions
      {
        HostId = _joinedLobby.Players[1].Id
      });

      _joinedLobby = _hostLobby;

      PrintPlayers(_hostLobby);
    }
    catch (LobbyServiceException e)
    {
      Debug.Log(e);
    }
  }

  async void DeleteLobby()
  {
    try
    {
      await LobbyService.Instance.DeleteLobbyAsync(_joinedLobby.Id);
      Debug.Log("Deleted Lobby");
    }
    catch (LobbyServiceException e)
    {
      Debug.Log(e);
    }
  }

  [ContextMenu("Start Game")]
  public async void StartGame()
  {
    if (!IsLobbyHost()) return;

    try
    {
      Debug.Log("Start Game");

      int myGun = 0;
      string myName = _playerName;
      Player me = _joinedLobby.Players.Find(p => p.Id == AuthenticationService.Instance.PlayerId);

      if (me != null)
      {
        myName = me.Data[_keyPlayerName].Value;
        myGun = int.Parse(me.Data[_keyGunId].Value);
      }
      
      PlayerLocalData localData = PlayerLocalData.Instance;
      localData.SetPlayerData(myGun, myName, _joinedLobby.Players.Count);

      int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
      string mapSceneName = System.IO.Path.GetFileNameWithoutExtension(
        SceneUtility.GetScenePathByBuildIndex(currentSceneIndex + 1)
      );
      
      string hostAddress = GetLocalIPAddress();
      ushort port = 7777;

      Lobby lobby = await LobbyService.Instance.UpdateLobbyAsync(_joinedLobby.Id, new UpdateLobbyOptions
      {
        Data = new Dictionary<string, DataObject>
        {
          { _keyStartGameHostAddress, new DataObject(DataObject.VisibilityOptions.Member, hostAddress) },
          { _keyStartGamePort, new DataObject(DataObject.VisibilityOptions.Member, port.ToString()) },
          { _keyMap, new DataObject(DataObject.VisibilityOptions.Public, mapSceneName) }
        }
      });

      _joinedLobby = lobby;

      Cons.Print($"Loading map: {mapSceneName}", ColorConsole.Yellow);
      
      LoadSceneWithFishNet(mapSceneName, port);
    }
    catch (LobbyServiceException e)
    {
      Debug.Log(e);
    }
  }
  
  void OnClientConnectionState(FishNet.Connection.NetworkConnection conn, 
    FishNet.Transporting.RemoteConnectionStateArgs args)
  {
    Debug.Log($"[Server] Remote state: {args.ConnectionState} | ConnId: {args.ConnectionId}");
    
    if (args.ConnectionState == FishNet.Transporting.RemoteConnectionState.Started)
    {
      StopAllCoroutines();
      _networkManager.ServerManager.OnRemoteConnectionState -= OnClientConnectionState;

      int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        
      string mapSceneName = System.IO.Path.GetFileNameWithoutExtension(
        SceneUtility.GetScenePathByBuildIndex(currentSceneIndex + 1)
      );
        
      SceneLoadData sld = new SceneLoadData(mapSceneName);
      sld.ReplaceScenes = ReplaceOption.All;
      InstanceFinder.SceneManager.LoadGlobalScenes(sld);
    }
  }
  
  void LoadSceneWithFishNet(string sceneName, ushort port)
  {
    var transport = _networkManager.TransportManager.Transport as FishNet.Transporting.Tugboat.Tugboat;
    
    if (transport != null)
    {
      transport.SetPort(port);
    }

    _networkManager.ServerManager.StartConnection();
    _networkManager.ClientManager.StartConnection();

    _networkManager.ServerManager.OnRemoteConnectionState += OnClientConnectionState;
    
    StartCoroutine(FallbackLoad(sceneName));
  }
  
  IEnumerator FallbackLoad(string sceneName)
  {
    yield return new WaitForSeconds(3f);

    Debug.Log("Fallback loading scene (no clients?)");

    SceneLoadData sld = new SceneLoadData(sceneName);
    sld.ReplaceScenes = ReplaceOption.All;
    sld.Options.AllowStacking = false;

    InstanceFinder.SceneManager.LoadGlobalScenes(sld);
  }
  
  string GetLocalIPAddress()
  {
    var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
    foreach (var ip in host.AddressList)
    {
      if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
      {
        return ip.ToString();
      }
    }
        
    return "127.0.0.1";
  }


  bool IsLobbyHost()
  {
    if (_joinedLobby != null)
      return _joinedLobby.HostId == AuthenticationService.Instance.PlayerId;

    return false;
  }

  public async void SetGun(int newGunId)
  {
    try
    {
      await LobbyService.Instance.UpdatePlayerAsync(
        _joinedLobby.Id,
        AuthenticationService.Instance.PlayerId,
        new UpdatePlayerOptions
        {
          Data = new Dictionary<string, PlayerDataObject>
          {
            { _keyGunId, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, newGunId.ToString()) }
          }
        });
      
      OnSetGun?.Invoke(newGunId);
    }
    catch (LobbyServiceException e)
    {
      Debug.Log(e);
    }
  }

  public async void SetReady()
  {
    try
    {
      Player me = _joinedLobby.Players.Find(p => p.Id == AuthenticationService.Instance.PlayerId);

      if (me == null)
      {
        Debug.LogError("Local player not found in lobby");
        return;
      }

      string current  = me.Data.ContainsKey("IsReady") ? me.Data["IsReady"].Value : "0";
      string newState = current == "0" ? "1" : "0";

      int id = _joinedLobby.Players.IndexOf(me);
      OnSetLocalReadyPlayer?.Invoke(newState == "1", id);
      
      await LobbyService.Instance.UpdatePlayerAsync(
        _joinedLobby.Id,
        AuthenticationService.Instance.PlayerId,
        new UpdatePlayerOptions
        {
          Data = new Dictionary<string, PlayerDataObject>
          {
            { "IsReady", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, newState) }
          }
        });
      
      Debug.Log("IsReady: " + newState);
    }
    catch (LobbyServiceException e)
    {
      Debug.Log(e);
    }
  }

  bool CheckAllPlayerReady()
  {
    if (_joinedLobby == null) return false;
    if (_joinedLobby.Players == null || _joinedLobby.Players.Count == 0) return false;

    foreach (var player in _joinedLobby.Players)
    {
      if (player.Data == null ||
          !player.Data.ContainsKey("IsReady") ||
          player.Data["IsReady"].Value != "1")
      {
        return false;
      }
    }

    return true;
  }
}