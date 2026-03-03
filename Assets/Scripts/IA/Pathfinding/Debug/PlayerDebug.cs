using System;
using UnityEngine;

public class PlayerDebug : MonoBehaviour
{
    public void Start()
    {
        EventBusInitialiser.instance.Bus.Subscribe(
            (PlayerPosRequestEvent PPRE) =>
            {
                PPRE.positionListener.OnPlayerMoving(transform.position);
            });
    }
}

public struct PlayerPosRequestEvent
{
    public IPlayerPositionListener positionListener;
}
