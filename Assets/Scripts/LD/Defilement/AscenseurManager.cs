using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class AscenseurManager : NetworkBusListener
{
    [Header("Prefabs")]
    [SerializeField] private GameObject[] _partsList;

    [Header("Settings")]
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private Transform _endPoint;
    [SerializeField] private float _durationTraveling;

    [Header("Events")] 
    [SerializeField] private float _timeBeforeActivatedAscenseurScroll = 1;
    
    private List<Ascenseur> _pool = new();
    
    /*public override void OnStartClient()
    {
        ListenToEvent<OnAscenseurStart>(CreatePool);
    }*/

    public override void OnStartNetwork()
    {
        Debug.Log("OnStartNetwork");
        FillPool();
    }

    private void FillPool()
    {
        for (int i = 0; i < _partsList.Length - 1; i++)
        {
            Ascenseur a = _partsList[i].GetComponent<Ascenseur>();
            //a.OnThresholdReached += HandleThreshold;
            //a.gameObject.SetActive(i == 0);
            _pool.Add(a);
            a.StartDescente(_spawnPoint.position, _endPoint.position, _durationTraveling);
        }
    }

    /*
    private void CreatePool(OnAscenseurStart data)
    {
        LaunchNext();
    }

    private void HandleThreshold()
    {
        ActivateNextAscenseurObserverRpc();
    }

    private void LaunchNext()
    {
        if (IsServerInitialized)
            ActivateNextAscenseurObserverRpc();
        else
            RequestActivateAscenseurServerRpc();
    }

    [ObserversRpc]
    private void ActivateNextAscenseurObserverRpc()
    {
        Ascenseur current = _pool[0];
        _pool.RemoveAt(0);
        _pool.Add(current);

        current.transform.position = _spawnPoint.position;
        current.StartDescente(_spawnPoint.position, _endPoint.position, _durationTraveling);
    }

    [ServerRpc]
    private void RequestActivateAscenseurServerRpc() => ActivateNextAscenseurObserverRpc();*/
}

public struct OnAscenseurStart
{ }