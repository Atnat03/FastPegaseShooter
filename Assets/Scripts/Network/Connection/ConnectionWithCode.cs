using System;
using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Net;
using System.Net.Sockets;

public class ConnectionWithCode : MonoBehaviour
{
    NetworkManager _networkManager;

    [Header("UI")]
    [SerializeField] private Button _hostConnectButton;
    [SerializeField] private Button _clientConnectButton;
    [SerializeField] private TMP_InputField _codeInputField;
    [SerializeField] private TextMeshProUGUI _codeTextUI;
    [SerializeField] private GameObject _connectedUI;
    [SerializeField] private GameObject _gameCodeUI;
    
    [Header("Wrong code")]
    [SerializeField] private GameObject _wrongCode;
    [SerializeField] private Transform _wrongCodeParent;

    private void Awake()
    {
        _networkManager = InstanceFinder.NetworkManager;
        _gameCodeUI.SetActive(false);
    }

    #region HOST

    public void CreateHostGame()
    {
        if (_networkManager == null)
            return;

        if (_networkManager.IsServerStarted || _networkManager.IsClientStarted)
            return;

        InstanceFinder.TransportManager.Transport.SetClientAddress("127.0.0.1");

        _networkManager.ServerManager.StartConnection();
        _networkManager.ClientManager.StartConnection();

        string code = GetConnectionCode();
//        Debug.Log("📒 Code de la partie : " + code);

        _codeTextUI.text = code;

        _connectedUI.SetActive(false);
        _gameCodeUI.SetActive(true);
    }

    #endregion

    #region CLIENT

    public void ClientConnectGame()
    {
        if (_networkManager == null)
            return;

        if (_networkManager.IsClientStarted)
            return;

        string addressToUse;

#if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPlaying)
        {
            if (Unity.Multiplayer.PlayMode.CurrentPlayer.ReadOnlyTags().Contains("Virtual"))
            {
                addressToUse = "127.0.0.1";
            }
            else
            {
                addressToUse = GetIPFromCode();
            }
        }
        else
#endif
        {
            addressToUse = GetIPFromCode();
        }
        
        InstanceFinder.TransportManager.Transport.SetClientAddress(addressToUse);
        _networkManager.ClientManager.StartConnection();
    }

    #endregion

    #region IP SYSTEM

    private string GetIPFromCode()
    {
        if (string.IsNullOrEmpty(_codeInputField.text))
        {
            Debug.LogWarning("Code invalide.");
            return "127.0.0.1";
        }

        string baseIP = GetBaseIP();
        int code = int.Parse(_codeInputField.text);

        int third = code / 256;
        int fourth = code % 256;

        return baseIP + third + "." + fourth;
    }

    private string GetBaseIP()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
            {
                string[] parts = ip.ToString().Split('.');
                return parts[0] + "." + parts[1] + ".";
            }
        }

        return "10.51.";
    }

    public string GetConnectionCode()
    {
        string localIP = GetLocalIP();

        string[] parts = localIP.Split('.');
        int third = int.Parse(parts[2]);
        int fourth = int.Parse(parts[3]);

        int code = third * 256 + fourth;
        return code.ToString();
    }

    private string GetLocalIP()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                return ip.ToString();
        }

        return "127.0.0.1";
    }
    
    private void OnEnable()
    {
        _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
    }

    private void OnDisable()
    {
        _networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
    }
    
    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            Destroy(Instantiate(_wrongCode, _wrongCodeParent), 2f);
        }
        
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            _connectedUI.SetActive(false);
            _gameCodeUI.SetActive(true);
        }
    }

    #endregion
}