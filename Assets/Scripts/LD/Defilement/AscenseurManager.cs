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

    /*[Header("Events")] 
    [SerializeField] private Transform _doorReference;*/
    
    private List<Ascenseur> _pool = new();

    public void LaunchElevator()
    {
        int count = _partsList.Length;

        for (int i = 0; i < count; i++)
        {
            Ascenseur a = _partsList[i].GetComponent<Ascenseur>();
            _pool.Add(a);
            
            a?.StartDescente(_spawnPoint.position, _endPoint.position, _durationTraveling);
        }
    }
}

public struct OnAscenseurStart
{ }