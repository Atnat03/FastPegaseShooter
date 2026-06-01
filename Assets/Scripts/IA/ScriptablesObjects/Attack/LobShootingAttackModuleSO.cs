using UnityEngine;

[CreateAssetMenu(fileName = "LobShootSO", menuName = "Scriptable Objects/AI/Entity/Attack/LobShootSO")]
public class LobShootingAttackModuleSO : ScriptableObject
{
    [Header("Bullets")]
    public float p_bulletSize = 0.2f;
    public float p_bulletSpeed = 1;
    public float p_maxBulletLifeTime = 10f;

    [Header("Shooting")]
    [Range(5,85)] public float p_shootingAngle = 30;
    public Vector3 p_shootingOffset = Vector3.up * 0.5f;
    public float p_splashSize = 3;
    public float p_splashDuration = 30;
    public float p_splashDamageDelay = 1;

    public static readonly float _g = -Physics.gravity.y;
}
