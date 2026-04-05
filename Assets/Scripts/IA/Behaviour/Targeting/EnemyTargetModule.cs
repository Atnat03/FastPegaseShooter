using System;
using CustomConsole.Runtime.Logger;
using FishNet;
using UnityEngine;

//[AddComponentMenu("EnemyBehaviour/Target")]
public abstract class EnemyTargetModule : EnemyBehaviourModule
{
    public int p_targetId { get; protected set; }
    [HideInInspector] public Vector3 p_lastTargetPosition =  Vector3.negativeInfinity;
    protected virtual bool IsNewTargetValid(PlayerPositionUpdateEvent PPUE) => true;

    public override void OnStartServer()
    {
        base.OnStartServer();
        ListenToEvent((PlayerPositionUpdateEvent PPUE) => OnPlayerPositionUpdate(PPUE));
    }

    public virtual void OnNetworkTick() {}

    protected virtual void OnPlayerPositionUpdate(PlayerPositionUpdateEvent PPUE)
    {
        if (IsNewTargetValid(PPUE))
        {
            p_targetId = PPUE.p_networkObjectId;
            OnNewTargetPosition(PPUE);
        }
    }
    protected virtual void OnNewTargetPosition(PlayerPositionUpdateEvent PPUE)
    {
        p_lastTargetPosition = PPUE.p_playerPosition;
    }
    public virtual Vector3 GetTargetPosition()
    {
        if(InstanceFinder.ClientManager.Objects.Spawned.TryGetValue(p_targetId, out var value))
            return value.transform.position;
        
        return Vector3.positiveInfinity; 
    }

    public virtual bool HasTarget() => true;
    public virtual bool IsMyTarget(int objectId)
    {
        return objectId == p_targetId;
    }
    public float GetTargetSqrDistance(Vector3 position)
    {
        float dist = (GetTargetPosition() - position).sqrMagnitude;
        return dist;
    }
}
