using System;
using CustomConsole.Runtime.Logger;
using FishNet;
using FishNet.Object;
using UnityEngine;

//[AddComponentMenu("EnemyBehaviour/Target")]
public abstract class EnemyTargetModule : EnemyBehaviourModule
{
    private int _targetId;
    public PlayerVisuelBridge p_playerVisualBridge {get; private set;}
    public int p_targetId
    {
        get => _targetId;
        set
        {
            _targetId = value;

            NetworkObject obj = GetTargetNetworkObject();
            if(obj == null) return;
            
            p_playerVisualBridge = obj.GetComponentInChildren<PlayerVisuelBridge>();
            p_onTargetPositionForceUpdate?.Invoke();
        }
    }

    [HideInInspector] public Vector3 p_lastTargetPosition =  Vector3.negativeInfinity;
    public Action p_onTargetPositionForceUpdate;
    protected virtual bool IsNewTargetValid(PlayerPositionUpdateEvent PPUE) => true;

    public override void OnStartServer()
    {
        base.OnStartServer();
        p_targetId = -1;
        ListenToEvent((PlayerPositionUpdateEvent PPUE) => OnPlayerPositionUpdate(PPUE));
    }

    protected virtual void OnPlayerPositionUpdate(PlayerPositionUpdateEvent PPUE)
    {
        if (IsNewTargetValid(PPUE))
        {
            p_targetId = PPUE.p_networkObjectId;
            p_lastTargetPosition = PPUE.p_playerPosition;
        }
    }
    public virtual Vector3 GetTargetPosition()
    {
        NetworkObject networkObject = GetTargetNetworkObject();
        if(networkObject)
            return networkObject.transform.position;
        
        return Vector3.positiveInfinity; 
    }
    NetworkObject GetTargetNetworkObject()
    {
        if(InstanceFinder.ClientManager.Objects.Spawned.TryGetValue(p_targetId, out var networkObject))
            return networkObject;
        
        return null;
    }

    public virtual bool HasTarget() => true;
    public virtual bool IsMyTarget(int objectId)
    {
        return objectId == p_targetId;
    }
    public virtual float GetTargetSqrDistance(Vector3 position)
    {
        float dist = (GetTargetPosition() - position).sqrMagnitude;
        return dist;
    }
}
