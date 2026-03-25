using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    [SerializeField] private List<Checkpoint> _checkpoints = new List<Checkpoint>();
    Dictionary<GameObject, int> playerCheckpoints = new Dictionary<GameObject, int>();
    private EventBus _bus;

    void Awake()
    {
        _bus = EventBusInitialiser.instance.Bus;
        _bus.Subscribe((OnPlayerDeathEvent data) => RespawnPlayer(data));
    }

    void Start()
    {
        foreach (Checkpoint checkpoint in _checkpoints)
        {
            checkpoint.p_checkpointManager = this;
        }
    }

    public void RegisterCheckpoint(Checkpoint checkpoint, GameObject player)
    {
        int ID = _checkpoints.IndexOf(checkpoint);
        if (!playerCheckpoints.TryAdd(player, ID))
        {
            if (playerCheckpoints[player] < ID)
            {
                playerCheckpoints[player] = ID;
            }
        }
        Debug.Log($"Registered player {player.gameObject.name} with ID {ID}");
    }

    private void RespawnPlayer(OnPlayerDeathEvent data)
    {
        data.playerN.transform.position = _checkpoints[playerCheckpoints[data.playerN.gameObject]].p_spawnPointTransform.position;
    }
}