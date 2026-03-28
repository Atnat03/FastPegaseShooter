using System;
using FishNet;
using UnityEngine;
using FishNet.Object;


public class BasicShootingAttackModule : EnemyAttackingModule
{
    [SerializeField] private float _maxPlayerDistance = 10f;
    [SerializeField] private float _bulletSize = 0.2f;
    [SerializeField] private float _ammoSpeed;
    [SerializeField] private float _maxAmmoLifeTime = 10f;
    
    

    public override void OnNetworkTick()
    {
        base.OnNetworkTick();
        if (_waitedTimeSinceAttack >= _attackDelay && CanAttack())
        {
            _waitedTimeSinceAttack = 0;

            Vector3 delta = _targetingModule.GetTargetPosition() - transform.position;
            float length = delta.magnitude;
            Vector3 dir = delta / length;
            
            InvokeEvent(new EnemyShootingEvent
            {
                p_startPos = transform.position + dir * 0.1f + Vector3.up * 0.5f,
                p_direction = dir,
                p_speed = _ammoSpeed,
                p_damage = _damage,
                p_aliveTime = _maxAmmoLifeTime,
                p_bulletSize = _bulletSize,
                p_enemyAttackingModule = this
            });
        }
    }

    protected override bool CanAttack()
    {
        if (GetTargetSqrDistance() > _maxPlayerDistance * _maxPlayerDistance)
        {
            return false;
        }


        Vector3 delta = _targetingModule.GetTargetPosition() - transform.position;
        float length = delta.magnitude;
        Vector3 dir = delta / length;

        Vector3 origin = transform.position + dir * 0.1f + Vector3.up * 0.5f;
        Debug.DrawLine(origin,origin + dir * length, Color.red, _attackDelay);
        if (Physics.Raycast(origin, dir, out RaycastHit hit, length, LayerMask.GetMask("Owner", "Other"), QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }


    private void OnDrawGizmos()
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
    }
}

public struct EnemyShootingEvent
{
    public Vector3 p_startPos;
    public Vector3 p_direction;
    public float p_speed;
    public int p_damage;
    public float p_aliveTime;
    public float p_bulletSize;
    public EnemyAttackingModule p_enemyAttackingModule;
}
