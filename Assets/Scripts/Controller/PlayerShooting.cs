using System;
using FishNet.Object;
using GunDecorator;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Controller
{
	public class PlayerShooting : NetworkBehaviour
	{
		#region Variables

		private EventBus _bus;
		
		[SerializeField] private PlayerInput _playerInputAction;
		[SerializeField] private GunSwitching _gunSwitching;
		[SerializeField] private GunBridgePlayer _bridgePlayer;
		
		#endregion
		
		#region Fonctions

		public override void OnStartClient()
		{
			base.OnStartClient();
			
			_bus = EventBusInitialiser.instance.Bus;

			_gunSwitching.Initialize();
		}
		
		private void Shooting(InputAction.CallbackContext obj)
		{
			if (!IsOwner) return;
			ShootSoundServerRpc();
		}

		[ServerRpc]
		private void ShootSoundServerRpc()
		{
			ShootSoundObserverRpc();
		}

		[ObserversRpc]
		private void ShootSoundObserverRpc()
		{
			_bus.InvokeEvent(new S_Shoot {
				data = GetComponent<TestSound>()._soundsData,
				player = NetworkObject,
			});
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
