using FishNet;
using UnityEngine;

public class FluidTargetModule : EnemyTargetModule
{
    protected override bool IsNewTargetValid(PlayerPositionUpdateEvent PPUE)
    {
        return IsMyTarget(PPUE.p_playerId) || IsPlayerCloser(PPUE.p_playerPosition);
    }
    bool IsPlayerCloser(Vector3 playerPosition) => (transform.position - playerPosition).sqrMagnitude < (transform.position - p_lastTargetPosition).sqrMagnitude;

}
