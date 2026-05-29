using System;
using MyPrint;
using UnityEngine;

namespace Tuto
{
    [RequireComponent(typeof(Collider))]
    public class TriggerBoxBridge : MonoBehaviour
    {
        public int bridgeIndex;

        public Action<PlayerSide> OnPlayerEntered;
        public Action<PlayerSide> OnPlayerExited;

        public bool IsRedInside  { get; private set; }
        public bool IsBlueInside { get; private set; }

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerVisuelBridge player))
            {
                Cons.Print("In");
                
                PlayerSide side = player.PlayerGun.IsPositive ? PlayerSide.Red : PlayerSide.Blue; 
                if (side == PlayerSide.Red)  
                    IsRedInside  = true;
                if (side == PlayerSide.Blue)
                    IsBlueInside = true;
                
                OnPlayerEntered?.Invoke(side);
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out PlayerVisuelBridge player))
            {
                Cons.Print("In");
                
                PlayerSide side = player.PlayerGun.IsPositive ? PlayerSide.Red : PlayerSide.Blue;
                if (side == PlayerSide.Red)  
                    IsRedInside  = false;
                if (side == PlayerSide.Blue) 
                    IsBlueInside = false;
                
                OnPlayerExited?.Invoke(side);
            }
        }
    }
}