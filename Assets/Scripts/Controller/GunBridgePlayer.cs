using System;
using System.Collections;
using FishNet.Object;
using UnityEngine;

namespace Controller
{
    public class GunBridgePlayer : NetworkBehaviour
    {
        public int GetCurrentMainIndex => _gunSwitching.CurrentMainGunIndex;
        
        [SerializeField] private GunSwitching _gunSwitching;

        private EventBus _bus;
        
        public override void OnStartClient()
        {
            base.OnStartClient();

            _bus = EventBusInitialiser.instance.Bus;

            if (IsOwner)
            {
                _bus.Subscribe((SwapingGunEvent data) => SwapingGun(data));
            }

            int startIndex = OwnerId % 2;
            _gunSwitching.Initialize(startIndex);
        }


        public void TryShootWithCurrentGun()
        { 
            _gunSwitching.CurrentGun.TryFire();
        }
        
        public void SwitchGunType()
        {
            _gunSwitching.SwitchGunType();
        }

        [ServerRpc]
        public void RequestSwapingGunServerRpc(NetworkObject playerNet, int gunIndex)
        {
            CallSwapGunEvent data = new CallSwapGunEvent
            {
                player = playerNet,
                gunIndex = gunIndex
            };
    
            _bus.InvokeEvent(data);
        }
        
        
        private void SwapingGun(SwapingGunEvent data)
        {
            StartCoroutine(WaitBeforeSwapCoroutine(data));
            Debug.Log($"[{OwnerId}] Swapped to index: {data.gunIndex}");
        }

        IEnumerator WaitBeforeSwapCoroutine(SwapingGunEvent data)
        {
            _gunSwitching.DesactivateAllMainGun();
            
            yield return new WaitForSeconds(data.timeToSwap);
            
            _gunSwitching.ChangeCurrentGun_Main(data.gunIndex);
        }
    }

    public struct CallSwapGunEvent
    {
        public NetworkObject player;
        public int gunIndex;
    }
    
    public struct SwapingGunEvent
    {
        public int gunIndex;
        public float timeToSwap;
    }
}