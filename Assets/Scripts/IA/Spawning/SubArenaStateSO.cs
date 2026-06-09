using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SubArenaStateSO", menuName = "Scriptable Objects/AI/Spawning/SubArenaStateSO")]
public class SubArenaStateSO : ScriptableObject
{
    public int p_budgetPerSecond;
    public Color p_color;
    public Sprite p_icon;
    public string p_name;
    
    public List<MobSpawnSO> p_spawnMobs;
}
