using System;
using System.Collections.Generic;
using FishNet;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Network.Lobby
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private Transform _parentList;
        [SerializeField] private LobbyInfoUI _lobbyInfoPrefab;
        [SerializeField] private GameObject _parent;
        
        [Header("Create Game")]
        [SerializeField] private GameObject _createGamePannel;
        [SerializeField] private Button _createGameButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private TMP_InputField _inputFieldPartyName;
        
        private LobbyManager _lobbyManager;

        private void Awake()
        {
            _createGameButton.onClick.AddListener(() => ActivateCreateGameUI(true));
            _backButton.onClick.AddListener(() => ActivateCreateGameUI(false));
        }

        private void Start()
        {
            ActivateCreateGameUI(false);
        }

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

            foreach (string lobbyData in list)
            {
                LobbyInfoUI lobbyInfo = Instantiate(_lobbyInfoPrefab, _parentList);
                lobbyInfo.Initialize(lobbyData, () => _lobbyManager.JoinGame(lobbyData));
            }
        }

        public void DesactivateUI()
        {
            _parent.SetActive(false);
        }

        #region Create Game
        
        public void ActivateCreateGameUI(bool state) => _createGamePannel.SetActive(state);

        #endregion
    }
}