using System;
using UnityEngine;

public class LoadingScreen : MonoBusListener
{
    [SerializeField] private GameObject loadingScreen;

    private void Awake()
    {
        ListenToEvent<OnSceneLoadingEvent>((OSLE) =>
        {
            loadingScreen.SetActive(true);
        });
        ListenToEvent<OnPlayerSpawnTPEvent>((OPSTPE) =>
        {
            loadingScreen.SetActive(false);
        });
    }
}
