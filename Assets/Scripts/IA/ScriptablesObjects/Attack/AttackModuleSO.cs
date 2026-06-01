using UnityEngine;

[CreateAssetMenu(fileName = "AttackModuleSO", menuName = "Scriptable Objects/AI/Entity/Attack/AttackModuleSO")]
public class AttackModuleSO : ScriptableObject
{
    public int p_damage = 10;
    public float p_attackDelay = 2f;
    public BulletTypes p_bulletType;
    public bool p_projectileUseGravity = false;
    public float p_maxPlayerDistance = 10f;
}
