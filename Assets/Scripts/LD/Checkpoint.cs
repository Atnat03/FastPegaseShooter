using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Transform p_spawnPointTransform;
    [HideInInspector] public CheckpointManager p_checkpointManager;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            p_checkpointManager.RegisterCheckpoint(this, other.transform.GetRootTransform().gameObject);
        }
    }
}