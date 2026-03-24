using FishNet;
using UnityEngine;

public class FluidTargetingModule : EnemyTargetingModule
{
    protected override bool IsNewTargetValid(PlayerPositionUpdateEvent PPUE)
    {
        return IsTargetPlayer(PPUE.p_playerId) || IsPlayerCloser(PPUE.p_playerPosition);
    }
    bool IsPlayerCloser(Vector3 playerPosition) => (transform.position - playerPosition).sqrMagnitude < (transform.position - p_lastTargetPosition).sqrMagnitude;

}
