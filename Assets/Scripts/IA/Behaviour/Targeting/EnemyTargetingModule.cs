using System;
using CustomConsole.Runtime.Logger;
using FishNet;
using UnityEngine;

public abstract class EnemyTargetingModule : EnemyBehaviourModule
{
    public int p_targetId { get; private set; }
    public Vector3 p_lastTargetPosition =  Vector3.negativeInfinity;
    protected abstract bool IsNewTargetValid(PlayerPositionUpdateEvent PPUE);

    public override void OnStartServer()
    {
        base.OnStartServer();
        ListenToEvent((PlayerPositionUpdateEvent PPUE) =>
        {
            if (IsNewTargetValid(PPUE))
            {
                p_targetId = PPUE.p_networkObjectId;
                OnNewTargetPosition(PPUE);
            }
        });
    }
    protected virtual void OnNewTargetPosition(PlayerPositionUpdateEvent PPUE)
    {
        p_lastTargetPosition = PPUE.p_playerPosition;
    }
    protected bool IsTargetPlayer(int playerId)  => playerId == p_targetId;
    public virtual Vector3 GetTargetPosition() => InstanceFinder.ClientManager.Objects.Spawned[p_targetId].transform.position;
    public virtual bool IsMyTarget(int objectId)
    {
        return objectId == p_targetId;
    }
}
