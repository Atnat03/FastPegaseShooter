using UnityEngine;
using FishNet.Object;


public class BasicEnemyShooting : NetworkBehaviour
{
    
}

public struct EnemyShootingEvent
{
    public Vector3 p_startPos;
    public Vector3 p_direction;
    public float p_speed;
}
