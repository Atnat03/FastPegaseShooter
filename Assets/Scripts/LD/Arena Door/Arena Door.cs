using System;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class ArenaDoor : NetworkBehaviour
{
    [SerializeField] private List<SpawnZone> areasToClear = new List<SpawnZone>();

    public Action p_onDoorOpening;

    private void Awake()
    {
        foreach (SpawnZone area in areasToClear)
        {
            //can only be received on server because initialised on server
            area.p_onSpawnZoneComplete += OnSpawnZoneComplete;
        }
    }

    private void OnSpawnZoneComplete(SpawnZone area)
    {
        areasToClear.Remove(area);
        area.p_onSpawnZoneComplete -= OnSpawnZoneComplete;

        if (areasToClear.Count == 0)
        {
            OnDoorOpeningObserverRPC();
        }
    }

    [ObserversRpc]
    public void OnDoorOpeningObserverRPC()
    {
        p_onDoorOpening?.Invoke();
    }
}
