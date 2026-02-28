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
	
	#endregion


	#region Fonctions
	
	private void Shooting(InputAction.CallbackContext obj)
	{
		if(_currentGunInHand != null)
			_currentGunInHand.TryFire();
	}
	
	void OnEnable()
	{
		_playerInputAction.actions["Shoot"].performed += Shooting;
	}
	
	void OnDisable()
	{
		_playerInputAction.actions["Shoot"].performed -= Shooting;
	}

	#endregion
}
