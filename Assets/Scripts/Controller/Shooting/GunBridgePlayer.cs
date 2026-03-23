using System;
using System.Collections;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Managers;
using UnityEngine;

namespace Controller
{
    public class GunBridgePlayer : NetworkBehaviour
    {
        public int GetCurrentMainIndex => _gunSwitching.CurrentMainGunIndex;
        public int GetCurrentAmmo => CurrentMainSurchargeGun.GetCurrentAmmo();
        
        public IGun CurrentGun => _gunSwitching.IsMainGun ? _gunSwitching.CurrentMainGun.GetComponent<IGun>() : _gunSwitching.CurrentSecondaryGun.GetComponent<IGun>();

        public ISurcharge CurrentMainSurchargeGun => _gunSwitching.CurrentMainGun.GetComponent<ISurcharge>();
        
        [SerializeField] private GunSwitching _gunSwitching;
        [SerializeField] private GunSurcharge _gunSurcharge;
        
        private readonly SyncVar<bool> _wantToSwitch = new SyncVar<bool>();
        private bool _localWantToSwitch = false;
        
        private Material _gunMaterial;
        
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
            else
            {
                SetLayerRecursively(gameObject, LayerMask.NameToLayer("Default"));
            }
            
            int startIndex = OwnerId % 2;
            _gunSwitching.Initialize(startIndex);
        }

        public void TryShootWithCurrentGun()
        { 
            CurrentGun.TryFire();
            Debug.Log("Shoot 2");
        }

        public void TryCancelShooting()
        {
            CurrentGun.TryCancelShooting();
        }

        public void TryReload()
        {
            CurrentGun.TryReload();
        }
        
        public void SwitchGunType()
        {
            if (_localWantToSwitch) return;
            
            _gunSwitching.SwitchGunType();
        }

        [ServerRpc]
        public void RequestSwapingGunServerRpc(NetworkObject playerNet, int gunIndex,int currentAmmo)
        {
            if (!_gunSwitching.IsMainGun) return;
            
            _wantToSwitch.Value = true;
            
            CallSwapGunEvent data = new CallSwapGunEvent
            {
                player = playerNet,
                gunIndex = gunIndex,
                currentAmmo = currentAmmo,
            };
    
            _bus.InvokeEvent(data);
        }

        private void SwapingGun(SwapingGunEvent data)
        {
            _wantToSwitch.Value = false;
            StartCoroutine(WaitBeforeSwapCoroutine(data));
        }

        IEnumerator WaitBeforeSwapCoroutine(SwapingGunEvent data)
        {
            CurrentMainSurchargeGun.StopReload();
            
            _gunMaterial = CurrentMainSurchargeGun.ModelGun.material;
            _gunSurcharge.SetColorImage(data.color);
            
            int ammoToApply = data.currentAmmo;

            _gunMaterial.SetFloat("_Dissolving", 0);

            float duration = data.timeToSwap / 2;
            float elapsedTime = 0;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                _gunMaterial.SetFloat("_Dissolving", elapsedTime / duration);
                
                yield return null;
            }

            _gunSwitching.ChangeCurrentGun_Main(data.gunIndex);

            _gunMaterial = CurrentMainSurchargeGun.ModelGun.material;
            
            _gunMaterial.SetFloat("_Dissolving", 1);
            
            elapsedTime = duration;
            
            while (elapsedTime >0)
            {
                elapsedTime -= Time.deltaTime;
                _gunMaterial.SetFloat("_Dissolving", elapsedTime / duration);
                
                yield return null;
            }
            
            _gunMaterial.SetFloat("_Dissolving", 0);
            
            _gunSurcharge.SetOverloadStats(true, 
                data.dataSurcharge.overloadDuration, 
                data.dataSurcharge.damageMultiplier, 
                data.dataSurcharge.cadenceMultiplier,
                ammoToApply);
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
        
        void SetLayerRecursively(GameObject obj, int newLayer)
        {
            obj.layer = newLayer;

            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
    }

    //Demande de swap
    public struct CallSwapGunEvent
    {
        public NetworkObject player;
        public int gunIndex;
        public int currentAmmo;
        public Color colorSwap;
    }
    
    //Swap accepté et envoyé au joueux
    public struct SwapingGunEvent
    {
        public SurchargeData dataSurcharge;
        public int gunIndex;
        public float timeToSwap;
        public int currentAmmo;
        public Color color;
    }

    //Event de fin de demande de swap
    public struct EndTimerSwapEvent
    {
        public NetworkObject player;
    }
}