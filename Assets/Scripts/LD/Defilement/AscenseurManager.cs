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
    
    
    private List<Ascenseur> _pool = new();

    public void LaunchElevator()
    {
        float launchTime = Time.time; // référence commune

        for (int i = 0; i < _partsList.Length; i++)
        {
            Ascenseur a = _partsList[i].GetComponent<Ascenseur>();
            _pool.Add(a);
            a?.StartDescente(_spawnPoint.position, _endPoint.position, _durationTraveling, launchTime);
        }
    }
}

public struct OnAscenseurStart
{ }