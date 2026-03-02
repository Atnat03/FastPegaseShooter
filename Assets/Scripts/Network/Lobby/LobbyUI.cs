using System;
using System.Collections.Generic;
using FishNet;
using UnityEngine;

namespace Network.Lobby
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private Transform _parentList;
        [SerializeField] private LobbyInfoUI _lobbyInfoPrefab;
        [SerializeField] private GameObject _parent;
        
        private LobbyManager _lobbyManager;

        public void SetLobbyManager(LobbyManager lobbyManager)
        {
            _lobbyManager = lobbyManager;
        }
        
        public void UpdateUIList(List<string> list)
        {
            print("update list " +  list.Count);
            
            foreach (Transform child in _parentList)
            {
                Destroy(child.gameObject);
            }

            foreach (string lobbyTitle in list)
            {
                LobbyInfoUI lobbyInfo = Instantiate(_lobbyInfoPrefab, _parentList);
                lobbyInfo.Initialize(lobbyTitle, () => _lobbyManager.JoinGame(lobbyTitle));
            }
        }

        public void DesactivateUI()
        {
            _parent.SetActive(false);
        }
    }
}