using System;
using System.Collections;
using System.Threading.Tasks;
using FishNet;
using FishNet.Managing.Scened;
using Unity.VisualScripting;
using UnityEngine;

public class LoadingScreen : MonoBusListener
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private float _minimumLoadingTime = 1;

    private bool fromConnection = true;
    
    void Awake()
    {
        if (fromConnection)
        {
            StartCoroutine(ConnectionLoading());
        }
    }

    IEnumerator ConnectionLoading()
    {
        loadingScreen.SetActive(true);
        yield return new WaitForSeconds(2f);
        loadingScreen.SetActive(false);

        fromConnection = false;
    }
    
    private void OnEnable()
    {
        InstanceFinder.SceneManager.OnLoadStart += OnLoadStart;
        InstanceFinder.SceneManager.OnLoadEnd += OnLoadEnd;
    }

    private void OnDisable()
    {
        InstanceFinder.SceneManager.OnLoadStart -= OnLoadStart;
        InstanceFinder.SceneManager.OnLoadEnd -= OnLoadEnd;
    }

    private void OnLoadStart(SceneLoadStartEventArgs args)
    {
        loadingScreen.SetActive(true);
    }

    private void OnLoadEnd(SceneLoadEndEventArgs args)
    {
        StartCoroutine(HideLoading());
    }

    IEnumerator HideLoading()
    {
        yield return new WaitForSeconds(_minimumLoadingTime);
        loadingScreen.SetActive(false);
    }
}
