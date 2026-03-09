using System;
using FishNet.Object;
using GunDecorator;
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

        private bool shootingInputPressed;

        #endregion

        #region Fonctions

        void Update()
        {
            if(_playerInputAction.actions["Shoot"].WasReleasedThisFrame())CancelShooting();
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

        private void Reloading(InputAction.CallbackContext obj)
        {
            if (!IsOwner) return;
            if (_playerHealth.IsDead) return;

            if (_bridgePlayer != null)
            {
                _bridgePlayer.TryReload();
            }
        }

        private void SwitchGunType(InputAction.CallbackContext obj)
        {
            if (!IsOwner) return;
            if (_playerHealth.IsDead) return;

            _bridgePlayer.SwitchGunType();
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

        void OnEnable()
        {
            _playerInputAction.actions["Shoot"].performed += Shooting;
            _playerInputAction.actions["SwitchGunType"].performed += SwitchGunType;
            _playerInputAction.actions["SwapGun"].performed += RequestSwapingGun;
            _playerInputAction.actions["Reload"].performed += Reloading;
        }

        void OnDisable()
        {
            _playerInputAction.actions["Shoot"].performed -= Shooting;
            _playerInputAction.actions["SwitchGunType"].performed -= SwitchGunType;
            _playerInputAction.actions["SwapGun"].performed -= RequestSwapingGun;
            _playerInputAction.actions["Reload"].performed -= Reloading;
        }

        #endregion
    }
}