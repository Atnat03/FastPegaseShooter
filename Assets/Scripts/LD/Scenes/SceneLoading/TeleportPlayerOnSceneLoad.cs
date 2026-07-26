using System.Collections.Generic;
using UnityEngine;

public class TeleportPlayerOnSceneLoad : MonoBusListener
{
    [SerializeField] private List<Transform> _spawnPositions = new List<Transform>();
    private void Start()
    {
        List<Vector3> spawnPositions = new List<Vector3>();
        foreach (Transform t in _spawnPositions)
            spawnPositions.Add(t.position);
        
        InvokeEvent(new OnPlayerSpawnTPEvent(spawnPositions));
    }
}

public struct OnPlayerSpawnTPEvent
{
    public List<Vector3> p_spawnPositions;

    public OnPlayerSpawnTPEvent(List<Vector3> spawnPositions)
    {
        p_spawnPositions = spawnPositions;
    }
}