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
        CustomLogger.HighlightLog("on networkTime attack 2");
        if (_waitedTimeSinceAttack >= _attackDelay && CanAttack())
        {
            CustomLogger.HighlightLog("on networkTime attack 3");
            _waitedTimeSinceAttack = 0;
            
            //Empty event for now
            if (InstanceFinder.ClientManager.Objects.Spawned.TryGetValue(_targetingModule.p_targetId, out NetworkObject player))
            {
                EventBusInitialiser.instance.Bus.InvokeEvent(new EnemyMeleeAttack());
                EventBusInitialiser.instance.Bus.InvokeEvent(new PlayerTakeDamageEvent
                {
                    playerN = player,
                    value = _damage
                });
            }
        }
    }

    protected override bool CanAttack()
    {
        if (GetTargetSqrDistance() > _maxPlayerDistance * _maxPlayerDistance)
        {
            CustomLogger.HighlightLog($"target distance : {GetTargetSqrDistance()}, max dist : {_maxPlayerDistance*_maxPlayerDistance}");
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
