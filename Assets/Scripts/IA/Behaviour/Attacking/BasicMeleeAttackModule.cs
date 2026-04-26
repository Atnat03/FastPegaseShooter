using CustomConsole.Runtime.Logger;
using FishNet;
using FishNet.Object;
using UnityEngine;

[AddComponentMenu("EnemyBehaviour/Attack/BasicMeleeAttackModule")]
public class BasicMeleeAttackModule : EnemyAttackModule
{
    public override void OnNetworkTick()
    {
        base.OnNetworkTick();
        if (_waitedTimeSinceAttack >= _attackDelay && CanAttack(Vector3.zero, Vector3.zero))//can attack do not use projectile direction
        {
            _waitedTimeSinceAttack = 0;
            
            if (InstanceFinder.ClientManager.Objects.Spawned.TryGetValue(_targetModule.p_targetId, out NetworkObject player))
            {
                //Empty event for now
                InvokeEvent(new EnemyMeleeAttack());
                InvokeEvent(new PlayerTakeDamageEvent
                {
                    p_playerN = player,
                    p_value = _damage
                });
                p_onHitPlayer?.Invoke(player.ObjectId, _damage);
            }
        }
    }

    protected override bool CanAttack(Vector3 shootingPos, Vector3 projectileDir)
    {
        if (GetTargetSqrDistance() > _maxPlayerDistance * _maxPlayerDistance)
        {
            return false;
        }
            
        //only condition is to be close enough from the player
        return true;
    }
    
    /*private void OnDrawGizmos()
    {
        if(!Application.isPlaying || !IsServerInitialized) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _maxPlayerDistance);
        Gizmos.DrawSphere(_targetingModule.GetTargetPosition(), 0.1f);
    }*/
}

public struct EnemyMeleeAttack
{
    
}
