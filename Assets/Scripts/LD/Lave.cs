using System;
using System.Collections.Generic;
using FishNet.Object;
using MyPrint;
using UnityEngine;

public class Lave : MonoBehaviour
{ 
    public void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerVisuelBridge player))
        {
            Cons.Print("LAVE");
            player.PlayerHealth.RequestTakeDamageServerRpc(100000);
        }
    }
}