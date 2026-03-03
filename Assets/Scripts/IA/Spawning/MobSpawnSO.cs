using UnityEngine;

[CreateAssetMenu(fileName = "MobSpawnSO", menuName = "Scriptable Objects/MobSpawnSO")]
public class MobSpawnSO : ScriptableObject
{
    public GameObject p_prefab;
    public int p_cost;
}
