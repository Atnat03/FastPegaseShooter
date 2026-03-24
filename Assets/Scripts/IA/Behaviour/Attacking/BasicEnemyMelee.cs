using CustomConsole.Runtime.Logger;
using FishNet;
using FishNet.Object;
using UnityEngine;

public class BasicEnemyMelee : EnemyAttackingModule
{
    [SerializeField] private float _maxPlayerDistance = 1.5f;
    
    public override void OnNetworkTick()
    {
        base.OnNetworkTick();
        if (_waitedTimeSinceAttack >= _attackDelay && CanAttack(out int playerObjectId))
        {
            _waitedTimeSinceAttack = 0;
            
            if (InstanceFinder.ClientManager.Objects.Spawned.TryGetValue(_targetingModule.p_targetId, out NetworkObject player))
            {
                //Empty event for now
                EventBusInitialiser.instance.Bus.InvokeEvent(new EnemyMeleeAttack());
                EventBusInitialiser.instance.Bus.InvokeEvent(new PlayerTakeDamageEvent
                {
                    playerN = player,
                    value = _damage
                });
                OnHitPlayer?.Invoke(player.ObjectId, _damage);
            }
        }
    }

    protected override bool CanAttack(out int playerObjectId)
    {
        playerObjectId = 0;
        if (GetTargetSqrDistance() > _maxPlayerDistance * _maxPlayerDistance)
        {
            return false;
        }
            
        //only condition is to be close enough from the player
        return true;
    }
    
    private void OnDrawGizmos()
    {
        if(!Application.isPlaying || !IsServerInitialized) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _maxPlayerDistance);
        Gizmos.DrawSphere(_targetingModule.GetTargetPosition(), 0.1f);
    }
}

public struct EnemyMeleeAttack
{
    
}
