using System;
using FishNet.Object;
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

public struct PlayerPosRequestEvent : INetworkEvent
{
    public IPlayerPositionListener positionListener;
    public NetworkObject player { get; set; }
}
