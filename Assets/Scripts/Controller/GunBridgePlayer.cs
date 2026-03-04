using System;
using System.Collections;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Controller
{
    public class GunBridgePlayer : NetworkBehaviour
    {
        public int GetCurrentMainIndex => _gunSwitching.CurrentMainGunIndex;
        //public int GetCurrentAmmo => _gunSwitching.CurrentMainSurchargeGun.GetCurrentAmmo();
        
        [SerializeField] private GunSwitching _gunSwitching;
        
        private readonly SyncVar<bool> _wantToSwitch = new SyncVar<bool>();
        private bool _localWantToSwitch = false;
        
        private EventBus _bus;
        
        public override void OnStartClient()
        {
            base.OnStartClient();

            _bus = EventBusInitialiser.instance.Bus;

            if (IsOwner)
            {
                _bus.Subscribe((SwapingGunEvent data) => SwapingGun(data));
                _bus.Subscribe((EndTimerSwapEvent data) => EndTimerSwap(data));
                
                _wantToSwitch.OnChange += (prev, next, asServer) => _localWantToSwitch = next;
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
            if (_localWantToSwitch) return;
            
            _gunSwitching.SwitchGunType();
        }

        [ServerRpc]
        public void RequestSwapingGunServerRpc(NetworkObject playerNet, int gunIndex)
        {
            if (!_gunSwitching.IsMainGun) return;
            
            _wantToSwitch.Value = true;
            
            CallSwapGunEvent data = new CallSwapGunEvent
            {
                player = playerNet,
                gunIndex = gunIndex
            };
    
            _bus.InvokeEvent(data);
        }

        private void SwapingGun(SwapingGunEvent data)
        {
            _wantToSwitch.Value = false;
            StartCoroutine(WaitBeforeSwapCoroutine(data));
            Debug.Log($"[{OwnerId}] Swapped to index: {data.gunIndex}");
        }

        IEnumerator WaitBeforeSwapCoroutine(SwapingGunEvent data)
        {
            _gunSwitching.DesactivateAllMainGun();
            
            yield return new WaitForSeconds(data.timeToSwap);
            
            _gunSwitching.ChangeCurrentGun_Main(data.gunIndex);
            //_gunSwitching.CurrentMainSurchargeGun.SetAmmo(data.currentAmmo);
            
            //_gunSwitching.CurrentMainSurchargeGun.SetSurchargeStat(true, 2, 2);
        }

        private void EndTimerSwap(EndTimerSwapEvent data)
        {
            if (data.player == NetworkObject) return;

            ResetWantToSwitchServerRpc();
        }
        
        [ServerRpc]
        private void ResetWantToSwitchServerRpc()
        {
            _wantToSwitch.Value = false;
        }
    }

    //Demande de swap
    public struct CallSwapGunEvent
    {
        public NetworkObject player;
        public int gunIndex;
        public int currentAmmo;
    }
    
    //Swap accepté et envoyé au joueux
    public struct SwapingGunEvent
    {
        public int gunIndex;
        public float timeToSwap;
        public int currentAmmo;
    }

    //Event de fin de demande de swap
    public struct EndTimerSwapEvent
    {
        public NetworkObject player;
    }
}