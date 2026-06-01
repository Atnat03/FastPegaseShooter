using UnityEngine;

[CreateAssetMenu(fileName = "MobSpawnSO", menuName = "Scriptable Objects/AI/Spawning/MobSpawnSO")]
public class MobSpawnSO : ScriptableObject
{
    public GameObject p_prefab;
    public float p_spawnProba;
    public int p_cost;
}
