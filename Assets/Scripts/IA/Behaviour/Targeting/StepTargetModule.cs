using FishNet.Connection;
using UnityEngine;

[AddComponentMenu("EnemyBehaviour/Target/StepTargetModule")]
public class StepTargetModule : ScoreTargetModule
{
    [SerializeField] private StepTargetModuleSO _stepTargetModuleSO;

    private bool _reachedFakeTarget;
    private int _fakeTargetIndex = -1;
    private float _timeToFakeTargetTargeting = 0;

    public override void OnNetworkTick(float tickDelta)
    {
        base.OnNetworkTick(tickDelta);
        
        if(!_stepTargetModuleSO.p_doSwitchTargeting) return;
        
        //only usefull for switching target
        if (HasTarget())
        {
            if(!_reachedFakeTarget && (transform.position - GetTargetPosition()).sqrMagnitude <=
               _stepTargetModuleSO.p_fakeTargetSwitchingDistance * _stepTargetModuleSO.p_fakeTargetSwitchingDistance)
            {
                _reachedFakeTarget = true;
                p_onTargetPositionUpdate?.Invoke();
                _fakeTargetIndex = p_playerVisualBridge.PlayerPositionCaster.GetTargetIndex();
                
                _timeToFakeTargetTargeting = Random.Range(_stepTargetModuleSO.p_rangeTimeForDirectTargeting.x,
                    _stepTargetModuleSO.p_rangeTimeForDirectTargeting.y);
            }

            if (_reachedFakeTarget && 
                //if both X and Y are greater or equal to 0
                _stepTargetModuleSO.p_rangeTimeForDirectTargeting is { x: >= 0, y: >= 0 })
            {
                _timeToFakeTargetTargeting -= tickDelta;
                if (_timeToFakeTargetTargeting <= 0)
                {
                    _reachedFakeTarget = false;
                    p_onTargetPositionUpdate?.Invoke();
                }
            }
        }
    }

    public override Vector3 GetTargetPosition()
    {
        if (p_playerVisualBridge == null) return transform.position;
        
        if (_stepTargetModuleSO.p_doSwitchTargeting && _reachedFakeTarget)
        {
            return p_playerVisualBridge.FPSController.transform.position;
        }
        
        
        if (_fakeTargetIndex < 0)
            _fakeTargetIndex = p_playerVisualBridge.PlayerPositionCaster.GetTargetIndex();

        return p_playerVisualBridge.PlayerPositionCaster.GetFakeTargetPosition(_fakeTargetIndex);
    }
    public override float GetTargetSqrDistance(Vector3 position)
    {
        if (p_playerVisualBridge == null || p_playerVisualBridge.FPSController == null)
            return float.MaxValue;
        
        float dist = (p_playerVisualBridge.FPSController.transform.position - position).sqrMagnitude;
        return dist;
    }
    
    public override bool HasTarget()
    {
        if (p_playerVisualBridge == null) return false;
        if (p_playerVisualBridge.FPSController == null) return false;
        return base.HasTarget();
    }
}