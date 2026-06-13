using FishNet;
using FishNet.Object;
using UnityEngine;

public class BorneSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject _bornePrefab;
    [SerializeField] private Transform _spawnPoint;

    public override void OnStartServer()
    {
        base.OnStartServer();
        SpawnBorne();
    }

    private void SpawnBorne()
    {
        Vector3 position = _spawnPoint != null ? _spawnPoint.position : Vector3.zero;
        Quaternion rotation = _spawnPoint != null ? _spawnPoint.rotation : Quaternion.identity;

        GameObject borne = Instantiate(_bornePrefab, position, rotation);
        InstanceFinder.ServerManager.Spawn(borne);
    }
}