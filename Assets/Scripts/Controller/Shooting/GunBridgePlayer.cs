using System;
using System.Collections;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using GunDecorator;
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
        [SerializeField] private PlayerCapacity _playerCapacity;
        
        private bool _isInitialized = false;

        public bool p_unlockSwapEnergyLaser = true;
        public bool p_unlockChargedShoot = true;
        
        public void InitializeWithGunId(int gunId)
        {
            _gunSwitching.Initialize(gunId);
            _isInitialized = true;
        }

        public void TryShootWithCurrentGun()
        {
            if (!_gunSwitching.IsMainGun)
            {
                _gunSwitching.ShootEnergy.TryShoot();
                return;
            }

            if (_gunSwitching.IsSwitching) return;
            if (!_isInitialized) return;
            if(CurrentGun.IsChargeShooting())return;
            
            CurrentGun.TryFire();
        }

        public void TryCancelShooting()
        {
            if (!_gunSwitching.IsMainGun)
            {
                _gunSwitching.ShootEnergy.TryCancelShoot();
                return;
            }
            
            if (_gunSwitching.IsSwitching) return;
            if (!_isInitialized) return;
            
            CurrentGun.TryCancelShooting();
        }
        
        public void TryShootChargeShooting()
        {
            if (_gunSwitching.IsSwitching) return;
            if (!_isInitialized) return;
            if (!_playerCapacity.CanChargedShoot) return;
            if (!p_unlockChargedShoot)return;
            
            InvokeEvent(new OnUseCapacity
            {
                p_capacityData = Capacity.ChargedShoot
            });

            InvokeEvent(new OnDataLog
            {
                entityName = transform.GetRootTransform().gameObject.name,
                EntityID = ObjectId,
                weapon = gameObject.name,
                skillUsed = "ChargedShoot",
                ArenaID = -1,
            });
            
            CurrentGun.TryShootCharged();
        }

        public void TryReload()
        {
            if (_gunSwitching.IsSwitching) return;
            if (!_isInitialized) return;
            if(CurrentGun.IsChargeShooting())return;
            
            CurrentGun.TryReload();
        }

        public void TryChangeMain(bool isMain)
        {
            if (!p_unlockSwapEnergyLaser)
                return;
            
            InvokeEvent(new OnFireModeChanged_TUTO());
            
            _gunSwitching.ChangeGunServerRpc(isMain);
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