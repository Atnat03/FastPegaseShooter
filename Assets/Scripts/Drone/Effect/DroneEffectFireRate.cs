using System;
using System.Collections.Generic;
using FishNet.Connection;
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
		base.ApplyEffect();
		
		foreach (PlayerVisuelBridge player in _playerUnderEffect)
		{
			Cons.Print(_playerUnderEffect.Count.ToString(), ColorConsole.Red);
			player.PlayerGun.IGunMain.SetFireRate(_fireRateMultiplicator);
			ApplyFireRateObserverRpc(player.Owner, _fireRateMultiplicator, player);
		}
	}

	protected override void StopApplicateEffect(PlayerVisuelBridge playerVisuelBridge)
	{
		playerVisuelBridge.PlayerGun.IGunMain.SetFireRate(-1);
	}
	
	[ObserversRpc]
	private void ApplyFireRateObserverRpc(NetworkConnection target, float multiplier, PlayerVisuelBridge player)
	{
		if (target != LocalConnection) return;
    
		player.PlayerGun.IGunMain.SetFireRate(multiplier);
	}

	public override void ApplyDeathEffect()
	{
		foreach (PlayerVisuelBridge player in _playerUnderEffect)
		{
			ApplyFireRateObserverRpc(player.Owner, -1, player);
		}
	}
	
	#endregion
}
