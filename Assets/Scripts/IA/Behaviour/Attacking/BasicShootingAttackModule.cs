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
        if (_waitedTimeSinceAttack >= _attackDelay)
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
                _damage, 
                _bulletSize,
                _bulletType,
                _maxBulletLifeTime, 
                this,
                _projectileUseGravity));
        }
    }

    protected override bool CanAttack(Vector3 shootingPos, Vector3 projectileDir)
    {
        if (GetTargetSqrDistance() > _maxPlayerDistance * _maxPlayerDistance)
        {
            return false;
        }

        Debug.DrawLine(shootingPos,shootingPos + projectileDir * _maxPlayerDistance, Color.red, _attackDelay);
        
        if (Physics.Raycast(shootingPos, projectileDir, out RaycastHit hit, _maxPlayerDistance, LayerMask.GetMask("Owner", "Other"), QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }


    /*private void OnDrawGizmos()
    {
        if(!Application.isPlaying || !IsServerInitialized) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _maxPlayerDistance);
        Gizmos.DrawSphere(_targetingModule.GetTargetPosition(), 0.1f);
        
        Vector3 delta = _targetingModule.GetTargetPosition() - transform.position;
        float length = delta.magnitude;
        Vector3 dir = delta / length;

        Vector3 origin = transform.position + dir * 0.1f + Vector3.up * 0.5f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, _bulletSize);
        Gizmos.DrawWireSphere(origin+dir*length, _bulletSize);
        Gizmos.DrawLine(origin, origin + dir * length);
    }*/
}
