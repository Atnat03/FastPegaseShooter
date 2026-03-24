using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Transform p_spawnPointTransform;
    [HideInInspector] public CheckpointManager p_checkpointManager;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            p_checkpointManager.RegisterCheckpoint(this, GetRootParent(other.transform).gameObject);
        }
    }

    Transform GetRootParent(Transform t)
    {
        if (t.parent == null) return t;
        return GetRootParent(t.parent);
    }
}