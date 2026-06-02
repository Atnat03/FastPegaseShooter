using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScoreTargetModuleSO", menuName = "Scriptable Objects/AI/Entity/Target/ScoreTargetModuleSO")]
public class ScoreTargetModuleSO : ScriptableObject
{
    [Header("Aggro")]
    public int p_aggroPointWhenInDetectZone;
    public int p_aggroPointPerDamageTaken;
    public int p_aggroPointPerSecond;
    public int p_aggroPointPerDamageDealt;
    public List<int> p_aggroPointsThreshold = new List<int>(){0,100,200};
    
    [Header("Zones")]
    public float p_detectionZoneRadius;
    public float p_aggroZoneRadius;
}
