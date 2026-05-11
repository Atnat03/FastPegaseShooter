using UnityEngine;

[AddComponentMenu("EnemyBehaviour/Target/StepTargetModule")]
public class StepTargetModule : ScoreTargetModule
{
    [SerializeField] private float _fakeTargetSwitchingDistance;

    private bool _reachedFakeTarget;
    private int _fakeTargetIndex = -1;

    protected override void OnPlayerPositionUpdate(PlayerPositionUpdateEvent PPUE)
    {
        base.OnPlayerPositionUpdate(PPUE);
        if (HasTarget() && _fakeTargetIndex < 0)
        {
            Debug.LogWarning("test");
            _fakeTargetIndex = p_playerVisualBridge.PlayerPositionCaster.GetTargetIndex();
        }
    }

    public override void OnNetworkTick()
    {
        base.OnNetworkTick();
        if (HasTarget() && !_reachedFakeTarget && (transform.position - GetTargetPosition()).sqrMagnitude <=
            _fakeTargetSwitchingDistance * _fakeTargetSwitchingDistance)
        {
            _reachedFakeTarget = true;
        }
    }

    public override Vector3 GetTargetPosition()
    {
        if (_reachedFakeTarget)
        {
            return p_playerVisualBridge.FPSController.transform.position;
        }

        return p_playerVisualBridge.PlayerPositionCaster.GetFakeTargetPosition(_fakeTargetIndex);
    }
    public override float GetTargetSqrDistance(Vector3 position)
    {
        float dist = (p_playerVisualBridge.FPSController.transform.position - position).sqrMagnitude;
        return dist;
    }
}