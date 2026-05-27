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


public struct OnEnemyDieEvent
{
    public EnemyCore p_enemy;
    public float p_energyToDropInOrb;

    public OnEnemyDieEvent(EnemyCore core, float energyToDropInOrb)
    {
        p_enemy = core;
        p_energyToDropInOrb = energyToDropInOrb;
    }
}

#region Sub Arena
    public struct OnDapEvent
    {
        
    }

    public struct OnSubArenaUpdateEvent
    {
        public Guid p_arenaID;
        public float p_overCrowdingPercent;
        public SubArenaStateSO p_state;
        public string p_arenaName;

        public OnSubArenaUpdateEvent(Guid arenaID, float overCrowdingPercent, string arenaName, SubArenaStateSO state)
        {
            p_arenaID = arenaID;
            p_state = state;
            p_overCrowdingPercent = overCrowdingPercent;
            p_arenaName = arenaName;
        }
    }
    public struct OnCorrosionEvent
    {
        public int p_corrosionDamage;

        public OnCorrosionEvent(int damages)
        {
            p_corrosionDamage = damages;
        }
    }
#endregion


