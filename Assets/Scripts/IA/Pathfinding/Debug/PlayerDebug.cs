using System;
using CustomConsole.Runtime.Logger;
using UnityEngine;

public class PlayerDebug : MonoBehaviour
{
    //##########
    //Script Broken by removal of "PlayerPosRequestEvent" struct
    //##########
    /*public void Start()
    {
        EventBusInitialiser.instance.Bus.Subscribe(
            (PlayerPosRequestEvent PPRE) =>
            {
                PPRE.positionListener.OnPlayerMoving(transform.position);
            });
    }*/
    private void FixedUpdate()
    {
        CustomLogger.CCErrorLog($"The script {typeof(PlayerDebug)} attached to {transform.name} isn't valid anymore. No behaviour remaining");
    }
}

//##########
//Struct Broken by removal of "IPlayerPositionListener" interface
//##########
/*public struct PlayerPosRequestEvent
{
    public IPlayerPositionListener positionListener;
}*/
