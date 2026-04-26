using System;
using System.Collections;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Managers;
using MyPrint;
using UnityEngine;

namespace Controller
{
    public class GunBridgePlayer : NetworkBusListener
    {
        #region Properties
        public int GetCurrentMainIndex => _gunSwitching.CurrentMainGunIndex;
        public int GetCurrentAmmo => CurrentMainSurchargeGun.GetCurrentAmmo();
        private IGun CurrentGun => _gunSwitching.IGunMain;
        public ISurcharge CurrentMainSurchargeGun => _gunSwitching.ISurchargeMain;
        
        #endregion
        
        [SerializeField] private GunSwitching _gunSwitching;
        [SerializeField] private GunSurcharge _gunSurcharge;
        [SerializeField] private GrenadeThrower _grenadeThrower;
        
        private readonly SyncVar<bool> _wantToSwitch = new SyncVar<bool>();
        
        private bool _localWantToSwitch = false;
        
        private Material _gunMaterial;
        
        private bool _isInitialized = false;
        
        public override void OnStartClient()
        {
            base.OnStartClient();

            if (IsOwner)
            {
                ListenToEvent<SwapingGunEvent>(SwapingGun);
                ListenToEvent<EndTimerSwapEvent>(EndTimerSwap);
                _wantToSwitch.OnChange += (prev, next, asServer) => _localWantToSwitch = next;
            }
            else
            {
                SetLayerRecursively(gameObject, LayerMask.NameToLayer("Default"));
            }

            _gunSwitching.OnStartSwitchGun += StopReloadGun;
        }

        public void InitializeWithGunId(int gunId)
        {
            _grenadeThrower.Initialize(gunId);
            _gunSwitching.Initialize(gunId);
            _isInitialized = true;
        }

        public void TryShootWithCurrentGun()
        {
            if (_gunSwitching == null || _gunSwitching.IGunMain == null)
                return;

            if (_gunSwitching.IsSwitching) return;
            if (!_isInitialized) return;
            
            CurrentGun.TryFire();
        }

        public void TryCancelShooting()
        {
            if (_gunSwitching.IsSwitching) return;
            if (!_isInitialized) return;
            
            CurrentGun.TryCancelShooting();
        }
        
        public void TryChargeWithCurrentGun()
        {             
            if (_gunSwitching.IsSwitching) return;
            if (!_isInitialized) return;

            TryCancelShooting();
            CurrentGun.TryCharging();
        }

        public void TryShootChargeShooting()
        {
            if (_gunSwitching.IsSwitching) return;
            if (!_isInitialized) return;
            
            CurrentGun.TryShootCharged();
        }

        public void TryReload()
        {
            if (_gunSwitching.IsSwitching) return;
            if (!_isInitialized) return;
            
            CurrentGun.TryReload();
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
    
            InvokeEvent(data);
        }

        private void SwapingGun(SwapingGunEvent data)
        {
            ResetWantToSwitchServerRpc();
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

            _gunSwitching.ChangeCurrentGun_Main_ServerRpc(data.gunIndex);
            _grenadeThrower.ChangeMagneticChargeServerRpc();

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
        
        private void StopReloadGun()
        {
            CurrentMainSurchargeGun.StopReload();
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