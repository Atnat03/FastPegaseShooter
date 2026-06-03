using UnityEngine;

[CreateAssetMenu(fileName = "PredictiveShootSO", menuName = "Scriptable Objects/AI/Entity/Attack/PredictiveShootSO")]
public class PredictiveShootingAttackModuleSO : ScriptableObject
{
    public float p_bulletSize = 0.2f;
    public float p_bulletSpeed = 1;
    public float p_maxBulletLifeTime = 10f;

    public int p_bulletAmount = 1;
    public float p_shootingSpreadAngle = 0;
}
