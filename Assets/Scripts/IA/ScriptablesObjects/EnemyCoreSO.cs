using UnityEngine;

[CreateAssetMenu(fileName = "EnemyCoreSO", menuName = "Scriptable Objects/AI/Entity/EnemyCoreSO")]
public class EnemyCoreSO : ScriptableObject
{
    public float p_spawningTime;
    public float p_deathTime;
    
    public bool p_dropXpOrb = false;
    public float p_baseEnergyDropValue = 5;
    
    public ChargeType p_pinataType = ChargeType.None;
    public float p_pinataEnergyDropValue = 10;
}
