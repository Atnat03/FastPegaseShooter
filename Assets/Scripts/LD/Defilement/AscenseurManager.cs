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
    
    public override void OnStartClient()
    {
        ListenToEvent<OnAscenseurStart>(CreatePool);
    }

    public override void OnStartNetwork()
    {
        FillPool();
    }

    private void FillPool()
    {
        Ascenseur first = _partsList[0].GetComponent<Ascenseur>();
        first.OnThresholdReached += HandleThreshold;
        first.gameObject.SetActive(true);
        _pool.Add(first);

        for (int i = 0; i < _pool.Count - 1; i++)
        {
            Ascenseur a = _partsList[i].GetComponent<Ascenseur>();
            a.OnThresholdReached += HandleThreshold;
            a.gameObject.SetActive(false);
            _pool.Add(a);
        }
    }

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
    private void RequestActivateAscenseurServerRpc() => ActivateNextAscenseurObserverRpc();
}

public struct OnAscenseurStart
{ }