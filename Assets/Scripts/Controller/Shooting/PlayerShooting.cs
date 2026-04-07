using System;
using FishNet.Object;
using MyPrint;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Controller
{
    public class PlayerShooting : NetworkBehaviour
    {
        #region Variables

        [SerializeField] private PlayerInput _playerInputAction;
        [SerializeField] private GunBridgePlayer _bridgePlayer;
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private GrenadeThrower _grenadeThrower;
        [SerializeField] private DroneThrower _droneThrower;

        private bool shootingInputPressed;

        #endregion

        #region Fonctions

        void Update()
        {
            if (_playerInputAction.actions["Shoot"].WasReleasedThisFrame()) CancelShooting();
            if(_playerInputAction.actions["Charge"].WasReleasedThisFrame())ShootCharged();
        }
        

        private void Shooting(InputAction.CallbackContext obj)
        {
            if (!IsOwner) return;
            if (_playerHealth.IsDead) return;

            if (_bridgePlayer != null)
            {
                _bridgePlayer.TryShootWithCurrentGun();
            }
        }

        private void CancelShooting()
        {
            if (!IsOwner) return;
            if (_playerHealth.IsDead) return;

            if (_bridgePlayer != null)
            {
                _bridgePlayer.TryCancelShooting();
            }
        }
        
        private void Charging(InputAction.CallbackContext obj)
        {
            if (!IsOwner) return;
            if (_playerHealth.IsDead) return;
            
            if (_bridgePlayer != null)
            {
                _bridgePlayer.TryChargeWithCurrentGun();
            }
        }
        
        private void ShootCharged()
        {
            if (!IsOwner) return;
            if (_playerHealth.IsDead) return;
            
            if (_bridgePlayer != null)
            {
                _bridgePlayer.TryShootChargeShooting();
            }
        }

        private void Reloading(InputAction.CallbackContext obj)
        {
            if (!IsOwner) return;
            if (_playerHealth.IsDead) return;

            if (_bridgePlayer != null)
            {
                _bridgePlayer.TryReload();
            }
        }

        private void RequestSwapingGun(InputAction.CallbackContext obj)
        {
            if (!IsOwner) return;
            if (_playerHealth.IsDead) return;

            _bridgePlayer.RequestSwapingGunServerRpc(
                this,
                _bridgePlayer.GetCurrentMainIndex,
                _bridgePlayer.GetCurrentAmmo);
        }

        private void TryThrowGrenade(InputAction.CallbackContext obj)
        {
            if (!IsOwner) return;
            if (_playerHealth.IsDead) return;
            
            _grenadeThrower.TryThrowGrenade();
        }
        
        private void TryThrowDrone(InputAction.CallbackContext obj)
        {
            if (!IsOwner) return;
            if (_playerHealth.IsDead) return;
            
            Debug.Log("Throw drone input");
            
            _droneThrower.TryThrowDrone();
        }

        void OnEnable()
        {
            _playerInputAction.actions["Shoot"].performed += Shooting;
            _playerInputAction.actions["Charge"].performed += Charging;
            
            _playerInputAction.actions["SwapGun"].performed += RequestSwapingGun;
            _playerInputAction.actions["Reload"].performed += Reloading;
            
            _playerInputAction.actions["ThrowGrenade"].performed += TryThrowGrenade;
            _playerInputAction.actions["ThrowDrone"].performed += TryThrowDrone;
        }


        void OnDisable()
        {
            _playerInputAction.actions["Shoot"].performed -= Shooting;
            _playerInputAction.actions["Charge"].performed -= Charging;
            
            _playerInputAction.actions["SwapGun"].performed -= RequestSwapingGun;
            _playerInputAction.actions["Reload"].performed -= Reloading;
            
            _playerInputAction.actions["ThrowGrenade"].performed -= TryThrowGrenade;
            _playerInputAction.actions["ThrowDrone"].performed -= TryThrowDrone;
        }

        #endregion
    }
}