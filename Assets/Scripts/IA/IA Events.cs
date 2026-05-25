using System;
using UnityEngine;

#region Shooting
    public struct EnemyShootingEvent
    {
        public bool p_useGravity;
        public BulletTypes p_bulletType;
        
        public Vector3 p_startPos;
        public Vector3 p_generalDirection;
        public int p_bulletAmount;
        public float p_shootingSpreadAngle;

        public EnemyAttackModule p_enemyAttackModule;

        public float p_bulletSpeed;
        public int p_bulletDamage;
        public float p_bulletSize;
        public float p_bulletMaxAliveTime;

        public EnemyShootingEvent(Vector3 startPos, Vector3 dir, float bSpeed, int bDamage, float bSize, BulletTypes bulletType, float bLifeTime, EnemyAttackModule attackModule, bool useGravity, int bAmount = 1, float spreadAngle = 0)
        {
            p_startPos = startPos;
            p_generalDirection = dir;

            p_useGravity = useGravity;
            p_bulletType = bulletType;
        
            p_bulletAmount = bAmount;
            p_shootingSpreadAngle = spreadAngle;
        
            p_bulletSpeed = bSpeed;
            p_bulletDamage = bDamage;
            p_bulletSize = bSize;
            p_bulletMaxAliveTime = bLifeTime;
            p_enemyAttackModule = attackModule;
        }
    }
    public struct BulletDestructionEvent
    {
        public int p_bulletId;
    }
#endregion


public struct EnemyMeleeAttackEvent
{
    
}

public struct OnDapEvent
{
    
}

/*public struct EnemyDyingEvent
{
    public Guid p_gridReaderId;
    public int p_enemySpawnCost;
    public EnemyCore p_enemyCore;

    public EnemyDyingEvent(Guid id, int cost, EnemyCore core)
    {
        p_gridReaderId = id;
        p_enemySpawnCost = cost;
        p_enemyCore = core;
    }
}*/

