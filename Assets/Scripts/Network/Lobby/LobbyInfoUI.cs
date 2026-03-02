using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Network.Lobby
{
    public class LobbyInfoUI : MonoBehaviour
    {
        public int IP => _ip;
        
        [SerializeField] private TextMeshProUGUI _lobbyTitle;
        [SerializeField] private Button _joinButton;
        private int _ip;
        
        public void Initialize(string lobbyTitle, Action joinAction)
        {
            _lobbyTitle.SetText(lobbyTitle);
            _joinButton.onClick.AddListener(() => joinAction());
        }
    }
}