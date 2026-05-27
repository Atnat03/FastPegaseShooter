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

        private InputAction _shootAction;
        
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
            if (_shootAction.WasReleasedThisFrame()) CancelShooting();

            if (_shootAction.IsPressed())
            {
                if (!IsOwner) return;
                if (_playerHealth.IsDead) return;
                if (!_canShoot) return;

                if (_bridgePlayer != null)
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
        
        private void ChangeToMainGun(InputAction.CallbackContext obj)
        {
            if (!IsOwner) return;
            if (_playerHealth.IsDead) return;
            
            _bridgePlayer.TryChangeMain(true);
        }
        
        private void ChangeToEnergyGun(InputAction.CallbackContext obj)
        {
            if (!IsOwner) return;
            if (_playerHealth.IsDead) return;
            
            _bridgePlayer.TryChangeMain(false);
        }
        
        private void ChangeGunScroll(InputAction.CallbackContext obj)
        {
            if (!IsOwner) return;
            if (_playerHealth.IsDead) return;

            float scroll = obj.ReadValue<float>();
            
            if (scroll > 0)
            {
                _bridgePlayer.TryChangeMain(false);
            }
            else if (scroll < 0)
            {
                _bridgePlayer.TryChangeMain(true);
            }
        }
        
        void OnEnable()
        {
            _shootAction = _playerInputAction.actions["Shoot"];
            
            _playerInputAction.actions["Charge"].performed += ShootCharged;
            
            _playerInputAction.actions["Reload"].performed += Reloading;
            
            _playerInputAction.actions["ThrowDrone"].performed += TryThrowDrone;
            
            _playerInputAction.actions["ChangeToMainGun"].performed += ChangeToMainGun;
            _playerInputAction.actions["ChangeToEnergyGun"].performed += ChangeToEnergyGun;
            
            _playerInputAction.actions["ChangeGunScroll"].performed += ChangeGunScroll;
            
            //Stop Shoot
            _playerHealth.OnUpdateHealth += StopShooting;
            
            //Interact
            _playerInputAction.actions["Grapple"].performed += Interact;
        }

        void OnDisable()
        {
            _playerInputAction.actions["Charge"].performed -= ShootCharged;
            
            _playerInputAction.actions["Reload"].performed -= Reloading;
            
            _playerInputAction.actions["ThrowDrone"].performed -= TryThrowDrone;
            
            _playerInputAction.actions["ChangeToMainGun"].performed -= ChangeToMainGun;
            _playerInputAction.actions["ChangeToEnergyGun"].performed -= ChangeToEnergyGun;
            
            _playerInputAction.actions["ChangeGunScroll"].performed -= ChangeGunScroll;
            
            //Stop Shoot
            _playerHealth.OnUpdateHealth -= StopShooting;
            
            //Interact
            _playerInputAction.actions["Grapple"].performed -= Interact;
        }

        #endregion
    }
    
    public struct OnPlayerInteract{}
}