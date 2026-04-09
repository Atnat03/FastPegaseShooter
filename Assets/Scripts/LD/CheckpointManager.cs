using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBusListener
{
    [SerializeField] private List<Checkpoint> _checkpoints = new List<Checkpoint>();
    Dictionary<GameObject, int> playerCheckpoints = new Dictionary<GameObject, int>();

    void Awake()
    {
        ListenToEvent<OnPlayerDeathEvent>(RespawnPlayer);
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
        data.p_playerN.transform.position = _checkpoints[playerCheckpoints[data.p_playerN.gameObject]].p_spawnPointTransform.position;
    }
}