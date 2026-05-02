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
			player.PlayerGun.IGunMain.SetFireRate(_fireRateMultiplicator);
			player.PlayerGun.IGunMain.SetInfiniteAmmo(true);
			ApplyFireRateObserverRpc(player.Owner, _fireRateMultiplicator, player);
		}
	}

	protected override void StopApplicateEffect(PlayerVisuelBridge playerVisuelBridge)
	{
		base.StopApplicateEffect(playerVisuelBridge);
		
		playerVisuelBridge.PlayerGun.IGunMain.SetFireRate(-1);
		playerVisuelBridge.PlayerGun.IGunMain.SetInfiniteAmmo(false);

		ResetFireRateObserverRpc(playerVisuelBridge.Owner, playerVisuelBridge);
	}
	
	[ObserversRpc]
	private void ApplyFireRateObserverRpc(NetworkConnection target, float multiplier, PlayerVisuelBridge player)
	{
		if (target != LocalConnection) return;
    
		player.PlayerGun.IGunMain.SetFireRate(multiplier);
		player.PlayerGun.IGunMain.SetInfiniteAmmo(true);

	}

	public override void ApplyDeathEffect()
	{
		foreach (PlayerVisuelBridge player in _playerUnderEffect)
		{
			StopApplicateEffect(player);
		}
	}
	
	[ObserversRpc]
	private void ResetFireRateObserverRpc(NetworkConnection target, PlayerVisuelBridge player)
	{
		player.PlayerGun.IGunMain.SetFireRate(-1);
		player.PlayerGun.IGunMain.SetInfiniteAmmo(false);
	}
	
	#endregion
}
