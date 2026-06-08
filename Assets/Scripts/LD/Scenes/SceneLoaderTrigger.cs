using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LD.Scenes
{
    public class SceneLoaderTrigger : MonoBusListener
    {
        [SerializeField] private GameObject _door;

        #region Variables

        [SerializeField] private SceneField[] _sceneToLoad;
        [SerializeField] private SceneField[] _sceneToUnload;
        private int playerCount = 0;
        private List<PlayerVisuelBridge> alreadyCountedPlayers = new List<PlayerVisuelBridge>();
        [SerializeField] bool need2Players = false;

        #endregion


        #region Fonctions
        
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerVisuelBridge player))
            {
                if (!alreadyCountedPlayers.Contains(player))
                {
                    playerCount++;
                    alreadyCountedPlayers.Add(player);
                    if (playerCount >= 2 || !need2Players)
                    {
                        InvokeEvent(new OnSceneLoadTrigger());
                        _door.SetActive(!_door.activeSelf);
                        SceneManaging.LoadScene(_sceneToLoad);
                        SceneManaging.UnloadScene(_sceneToUnload);
                    }
                }
            }
        }

        #endregion
    }
}

public struct OnSceneLoadTrigger
{
}