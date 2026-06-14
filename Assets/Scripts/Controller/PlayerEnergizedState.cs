using System;
using System.Collections;
using FishNet.Object;
using MyPrint;
using UnityEngine;

public class PlayerEnergizedState : NetworkBusListener
{
	[Header("References")] 
	[SerializeField] private GunSwitching _gunSwitching;
    
	[Header("Settings")] 
	[SerializeField] private float _damageFactor = 1.5f;
	[SerializeField, Tooltip("0 = Tir chargé / 1 = Drone / 2 = Heal")] public float[] _reloadCapacityValue;

	[Header("Test")] 
	[SerializeField] public bool _percentageFreeze = false;
	[SerializeField] private float _percentagePerSecondDecrease = 0.5f;
    
	public Action<bool> OnEnergized;

	private Coroutine _addPercentageCoroutineIncrease;
	private Coroutine _addPercentageCoroutineDecrease;

	public override void OnStartNetwork()
	{
		ListenToEvent<OnPlayerGetEnergized>(SetEnergizedPlayer);
		ListenToEvent<OnResetEnergizedEvent>(OnReset);
	}
	
	private void OnReset(OnResetEnergizedEvent data)
	{
		StopAddingPercentage();
		OnEnergized?.Invoke(false);
		_gunSwitching.CurrentMainGun.SetDamage(1);
	}
	
	private void SetEnergizedPlayer(OnPlayerGetEnergized data)
	{
		if (OwnerId == data.p_shooterOwnerId) return;
		if (data.p_ownerId != -1 && data.p_ownerId != OwnerId) return;
		
		_gunSwitching.CurrentMainGun.SetDamage(data.p_state ? _damageFactor : 1);
		OnEnergized?.Invoke(data.p_state);

		if (data.p_state)
			StartAddingPercentage();
		else
			StopAddingPercentage();
	}

	void StartAddingPercentage()
	{
		if (_addPercentageCoroutineIncrease != null)
			return;

		if (_addPercentageCoroutineDecrease != null)
		{
			StopCoroutine(_addPercentageCoroutineDecrease);
			_addPercentageCoroutineDecrease = null;
		}
    
		_addPercentageCoroutineIncrease = StartCoroutine(AddPercentageLoop(1));
	}

	void StopAddingPercentage()
	{
		if (_addPercentageCoroutineIncrease != null)
		{
			StopCoroutine(_addPercentageCoroutineIncrease);
			_addPercentageCoroutineIncrease = null;
		}

		if (_addPercentageCoroutineDecrease != null)
		{
			StopCoroutine(_addPercentageCoroutineDecrease);
			_addPercentageCoroutineDecrease = null;
		}

		if (!_percentageFreeze)
		{
			_addPercentageCoroutineDecrease = StartCoroutine(AddPercentageLoop(-1 * _percentagePerSecondDecrease));
		}
	}

	IEnumerator AddPercentageLoop(float ratio)
	{
		while (true)
		{
			foreach (Capacity capa in Enum.GetValues(typeof(Capacity)))
			{
				InvokeEvent(new OnAddPercentageCapactity
				{
					p_capacityData = capa,
					p_percentageValue = _reloadCapacityValue[(int)capa] * Time.deltaTime * ratio
				});
				
				if (ratio > 0)
					AddDapPercentageServerRpc(Time.deltaTime);
			}
			yield return null;
		}
	}
	
	[ServerRpc]
	private void AddDapPercentageServerRpc(float deltaTime)
	{
		InvokeEvent(new OnAddDapPercentage { p_ratio = deltaTime });
	}
}