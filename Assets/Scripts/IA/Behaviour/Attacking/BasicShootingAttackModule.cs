using System;
using FishNet;
using UnityEngine;
using FishNet.Object;
using UnityEngine.Serialization;

[AddComponentMenu("EnemyBehaviour/Attack/BasicShootAttackModule")]
public class BasicShootingAttackModule : EnemyAttackModule
{
    [SerializeField] private float _bulletSize = 0.2f;
    [SerializeField] private float _bulletSpeed = 1;
    [SerializeField] private float _maxBulletLifeTime = 10f;
    
    

    public override void OnNetworkTick(float tickDelta)
    {
        base.OnNetworkTick(tickDelta);
        if (_waitedTimeSinceAttack >= _attackModuleSO.p_attackDelay)
        {
            Vector3 delta = _targetModule.GetTargetPosition() - transform.position;
            float length = delta.magnitude;
            Vector3 dir = delta / length;

            Vector3 shootingPos = transform.position + Vector3.up * 0.5f;
            
            if(!CanAttack(shootingPos, dir)) return;
            _waitedTimeSinceAttack = 0;
            
            InvokeEvent(new EnemyShootingEvent(
                shootingPos, 
                dir, 
                _bulletSpeed, 
                _attackModuleSO.p_damage, 
                _bulletSize,
                _attackModuleSO.p_bulletType,
                _maxBulletLifeTime, 
                this,
                _attackModuleSO.p_projectileUseGravity));
        }
    }

    protected override bool CanAttack(Vector3 shootingPos, Vector3 projectileDir)
    {
        if (GetTargetSqrDistance() > _attackModuleSO.p_maxPlayerDistance * _attackModuleSO.p_maxPlayerDistance)
        {
            return false;
        }

        Debug.DrawLine(shootingPos,shootingPos + projectileDir * _attackModuleSO.p_maxPlayerDistance, Color.red, _attackModuleSO.p_attackDelay);
        
        if (Physics.Raycast(shootingPos, projectileDir, out RaycastHit hit, _attackModuleSO.p_maxPlayerDistance, LayerMask.GetMask("Owner", "Other"), QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }
}
