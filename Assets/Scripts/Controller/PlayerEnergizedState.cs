using System;
using MyPrint;
using UnityEngine;

public class PlayerEnergizedState : NetworkBusListener
{
	#region Properties

	#endregion


	#region Variables

	[Header("References")] 
	[SerializeField] private GunSwitching _gunSwitching;
	
	[Header("Settings")] 
	[SerializeField] private float _damageFactor = 1.5f;
	
	//Actions
	public Action<bool> OnEnergized;
	
	#endregion


	#region Fonctions

	public override void OnStartNetwork()
	{
		ListenToEvent<OnPlayerGetEnergized>(SetEnergizedPlayer);
	}
	
	private void SetEnergizedPlayer(OnPlayerGetEnergized data)
	{
		if (!IsOwner) return;
		if (data.p_ownerId != OwnerId) return; 

		Cons.Print("Set energized : " + data.p_state, ColorConsole.Cyan);
		
		_gunSwitching.CurrentMainGun.SetDamage(data.p_state ? _damageFactor : 1);
		OnEnergized?.Invoke(data.p_state);
	}
	
	#endregion
}
