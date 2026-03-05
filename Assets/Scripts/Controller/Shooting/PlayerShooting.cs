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

        #endregion

        #region Fonctions

        private void Shooting(InputAction.CallbackContext obj)
        {
            if (!IsOwner) return;

            if (_bridgePlayer != null)
            {
                _bridgePlayer.TryShootWithCurrentGun();
            }
        }

        private void Reloading(InputAction.CallbackContext obj)
        {
            if (!IsOwner) return;

            if (_bridgePlayer != null)
            {
                _bridgePlayer.TryReload();
            }
        }

        private void SwitchGunType(InputAction.CallbackContext obj)
        {
            if (!IsOwner) return;

            _bridgePlayer.SwitchGunType();
        }

        private void RequestSwapingGun(InputAction.CallbackContext obj)
        {
            if (!IsOwner) return;

            _bridgePlayer.RequestSwapingGunServerRpc(this, _bridgePlayer.GetCurrentMainIndex);
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