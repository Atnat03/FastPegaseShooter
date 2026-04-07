using System;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MyPrint;
using Unity.VisualScripting;
using UnityEngine;


public class DroneEffectFireRate : DroneEffectParent
{
	#region Properties

	#endregion


	#region Variables
	
	[Header("Multiplicateur")]
	[SerializeField] private float _fireRateMultiplicator = 2f;

	
	#endregion


	#region Fonctions

	protected override void ApplyEffect()
	{
		foreach (PlayerVisuelBridge player in _playerUnderEffect)
		{
			Cons.Print(_playerUnderEffect.Count.ToString(), ColorConsole.Red);
			player.PlayerGun.IGunMain.SetFireRate(_fireRateMultiplicator);
		}
	}

	public override void ApplyDeathEffect()
	{
		foreach (PlayerVisuelBridge player in _playerUnderEffect)
		{
			player.PlayerGun.IGunMain.SetFireRate(-1);
		}
	}
	
	#endregion
}
