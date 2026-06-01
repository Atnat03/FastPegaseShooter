using CustomConsole.Runtime.Logger;
using UnityEngine;

[AddComponentMenu("EnemyBehaviour/Target/StepTargetModule")]
public class StepTargetModule : ScoreTargetModule
{
    [SerializeField] private float _fakeTargetSwitchingDistance;
    
    [Tooltip("If this booléan is set to false, the enemy will only target te fake targets and never the player directly")]
    [SerializeField] private bool _doSwitchTargeting = false;
    [Tooltip("If one of the value is negative, the target module won't ever switch back to fake target")]
    [SerializeField] private Vector2 _rangeTimeForDirectTargeting = new Vector2(10, 30);

    private bool _reachedFakeTarget;
    private int _fakeTargetIndex = -1;
    private float _timeToFakeTargetTargeting = 0;

    public override void OnNetworkTick(float tickDelta)
    {
        base.OnNetworkTick(tickDelta);
        
        if(!_doSwitchTargeting) return;
        
        //only usefull for switching target
        if (HasTarget())
        {
            if(!_reachedFakeTarget && (transform.position - GetTargetPosition()).sqrMagnitude <=
                _fakeTargetSwitchingDistance * _fakeTargetSwitchingDistance)
            {
                _reachedFakeTarget = true;
                p_onTargetPositionUpdate?.Invoke();
                _fakeTargetIndex = p_playerVisualBridge.PlayerPositionCaster.GetTargetIndex();
                
                _timeToFakeTargetTargeting = Random.Range(_rangeTimeForDirectTargeting.x,
                    _rangeTimeForDirectTargeting.y);
            }

            if (_reachedFakeTarget && 
                //if both X and Y are greater or equal to 0
                _rangeTimeForDirectTargeting is { x: >= 0, y: >= 0 })
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
        if (_doSwitchTargeting && _reachedFakeTarget)
        {
            return p_playerVisualBridge.FPSController.transform.position;
        }
        
        
        if (_fakeTargetIndex < 0)
            _fakeTargetIndex = p_playerVisualBridge.PlayerPositionCaster.GetTargetIndex();

        return p_playerVisualBridge.PlayerPositionCaster.GetFakeTargetPosition(_fakeTargetIndex);
    }
    public override float GetTargetSqrDistance(Vector3 position)
    {
        float dist = (p_playerVisualBridge.FPSController.transform.position - position).sqrMagnitude;
        return dist;
    }
}