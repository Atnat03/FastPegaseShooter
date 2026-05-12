using System;
using System.Collections;
using FishNet.Object;
using MyPrint;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Controller
{
    public class PlayerInputs : NetworkBusListener
    {
        #region Variables

        [SerializeField] private PlayerInput _playerInputAction;
        [SerializeField] private GunBridgePlayer _bridgePlayer;
        [SerializeField] private PlayerHealth _playerHealth;
        [SerializeField] private GrenadeThrower _grenadeThrower;
        [SerializeField] private DroneThrower _droneThrower;
        
        private bool _canShoot = true;


        private bool shootingInputPressed;

        #endregion

        #region Fonctions

        private void Start()
        {
            ListenToEvent<OnPauseEvent>(data =>
            {
                _canShoot = !data.p_isPause;
            });
        }

        void Update()
        {
            if (_playerInputAction.actions["Shoot"].WasReleasedThisFrame()) CancelShooting();
        }

        private void Shooting(InputAction.CallbackContext obj)
        {
            if (!IsOwner) return;
            if (_playerHealth.IsDead) return;
            if (!_canShoot) return;
            
            if (_bridgePlayer != null)
                _bridgePlayer.TryShootWithCurrentGun();
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
        
        private void ShootCharged(InputAction.CallbackContext obj)
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
            if(!_canShoot) return;

            if (_bridgePlayer != null)
            {
                _bridgePlayer.TryReload();
            }
        }

        private void RequestSwapingGun(InputAction.CallbackContext obj)
        {
            if (!IsOwner) return;
            if (_playerHealth.IsDead) return;
            if(!_canShoot) return;

            _bridgePlayer.RequestSwapingGunServerRpc(
                this,
                _bridgePlayer.GetCurrentMainIndex,
                _bridgePlayer.GetCurrentAmmo);
        }

        private void TryThrowGrenade(InputAction.CallbackContext obj)
        {
            if (!IsOwner) return;
            if (_playerHealth.IsDead) return;
            if(!_canShoot) return;
    
            _grenadeThrower.TryThrowGrenade();
        }
        
        private void OnGrenadeThrown(ElementaryGrenade g)
        {
            if (_playerInputAction.actions["Shoot"].IsPressed())
            {
                _bridgePlayer.TryShootWithCurrentGun();
            }
        }
        
        private void TryThrowDrone(InputAction.CallbackContext obj)
        {
            if (!IsOwner) return;
            if (_playerHealth.IsDead) return;
            if(!_canShoot) return;
            
            _droneThrower.TryThrowDrone();
        }
        
        private void StopShooting(float duration)
        {
            if (duration <= 0) return;
            StartCoroutine(StopShootingWait(duration));
        }
        
        IEnumerator StopShootingWait(float duration)
        {
            _canShoot = false;
            
            yield return new WaitForSeconds(duration);
            _canShoot = true;
        }
        
        private void Interact(InputAction.CallbackContext obj)
        {
            if (!IsOwner) return;
            if (_playerHealth.IsDead) return;

            InvokeEvent(new OnPlayerInteract());
        }
        
        private void ChangeMagneticCharge(InputAction.CallbackContext obj)
        {
            if (!IsOwner) return;
            if (_playerHealth.IsDead) return;

            _bridgePlayer.TryChangeMagneticCharge();
        }

        void OnEnable()
        {
            _playerInputAction.actions["Shoot"].performed += Shooting;
            _playerInputAction.actions["Charge"].performed += ShootCharged;
            
            _playerInputAction.actions["SwapGun"].performed += RequestSwapingGun;
            _playerInputAction.actions["Reload"].performed += Reloading;
            
            _playerInputAction.actions["ThrowGrenade"].performed += TryThrowGrenade;
            _grenadeThrower.OnThrow += OnGrenadeThrown;
            
            _playerInputAction.actions["ThrowDrone"].performed += TryThrowDrone;
            
            //Stop Shoot
            _playerHealth.OnUpdateHealth += StopShooting;
            
            //Interact
            _playerInputAction.actions["Grapple"].performed += Interact;
            
            //Charges Magnetic
            _playerInputAction.actions["ChangeMagneticCharge"].performed += ChangeMagneticCharge;
        }

        void OnDisable()
        {
            _playerInputAction.actions["Shoot"].performed -= Shooting;
            _playerInputAction.actions["Charge"].performed -= ShootCharged;
            
            _playerInputAction.actions["SwapGun"].performed -= RequestSwapingGun;
            _playerInputAction.actions["Reload"].performed -= Reloading;
            
            _playerInputAction.actions["ThrowGrenade"].performed -= TryThrowGrenade;
            _grenadeThrower.OnThrow -= OnGrenadeThrown;
            
            _playerInputAction.actions["ThrowDrone"].performed -= TryThrowDrone;
            
            //Stop Shoot
            _playerHealth.OnUpdateHealth -= StopShooting;
            
            //Interact
            _playerInputAction.actions["Grapple"].performed -= Interact;
            
            //Charges Magnetic
            _playerInputAction.actions["ChangeMagneticCharge"].performed -= ChangeMagneticCharge;
        }

        #endregion
    }
    
    public struct OnPlayerInteract{}
}