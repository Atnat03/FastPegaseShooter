using System;
using FishNet.Object;
using GunDecorator;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : NetworkBehaviour
{
	#region Variables

	[SerializeField] private GunController _currentGunInHand;
	[SerializeField] private PlayerInput _playerInputAction;
	[SerializeField] private GunSwitching _gunSwitching;
	
	#endregion
	
	#region Fonctions

	public override void OnStartClient()
	{
		base.OnStartClient();

		_gunSwitching.Initialize();
		
		_currentGunInHand = _gunSwitching.CurrentMainGun.GetComponent<GunController>();
	}
	
	private void Shooting(InputAction.CallbackContext obj)
	{
		if(_currentGunInHand != null && _gunSwitching.IsMainGun)
			_currentGunInHand.TryFire();
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
