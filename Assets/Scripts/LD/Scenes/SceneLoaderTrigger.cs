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

        #endregion


        #region Fonctions

        void Start()
        {
            ListenToEvent<OnDapEvent>(OpenDoor);
        }

        void OpenDoor(OnDapEvent evt)
        {
            if (!_door) return;
            if (_door.GetComponent<Animation>()) _door.GetComponent<Animation>().Play();
            else _door.SetActive(false);
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerVisuelBridge player))
            {
                if (!alreadyCountedPlayers.Contains(player))
                {
                    playerCount++;
                    alreadyCountedPlayers.Add(player);
                    if (playerCount >= 2)
                    {
                        InvokeEvent(new OnSceneLoadTrigger());
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