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
		[SerializeField] private GunSwitching _gunSwitching;
		[SerializeField] private GunBridgePlayer _bridgePlayer;
		
		#endregion
		
		#region Fonctions

		public override void OnStartClient()
		{
			base.OnStartClient();

			_gunSwitching.Initialize();
		}
		
		private void Shooting(InputAction.CallbackContext obj)
		{
			if(_bridgePlayer != null)
				_bridgePlayer.TryShootWithCurrentGun();
		}

		private void SwitchGunType(InputAction.CallbackContext obj)
		{
			_gunSwitching.SwitchGunType();
		}
		
		void OnEnable()
		{
			_playerInputAction.actions["Shoot"].performed += Shooting;
			_playerInputAction.actions["SwitchGunType"].performed += SwitchGunType;
		}

		void OnDisable()
		{
			_playerInputAction.actions["Shoot"].performed -= Shooting;
			_playerInputAction.actions["SwitchGunType"].performed -= SwitchGunType;
		}

		#endregion
	}
}
