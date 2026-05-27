using System;
using System.Collections;
using MyPrint;
using UnityEngine;

public class PlayerEnergizedState : NetworkBusListener
{
	[Header("References")] 
	[SerializeField] private GunSwitching _gunSwitching;
    
	[Header("Settings")] 
	[SerializeField] private float _damageFactor = 1.5f;
	[SerializeField] private float _percentagePerSecond = 10f;
	[SerializeField, Tooltip("0 = Tir chargé / 1 = Drone / 2 = Heal")] 
	private float[] _reloadCapacityValue;

	[Header("Test")] 
	[SerializeField] private bool _percentageFreeze = false;
	[SerializeField] private float _percentagePerSecondDecrease = 0.5f;
    
	public Action<bool> OnEnergized;

	private Coroutine _addPercentageCoroutineIncrease;
	private Coroutine _addPercentageCoroutineDecrease;

	public override void OnStartNetwork()
	{
		ListenToEvent<OnPlayerGetEnergized>(SetEnergizedPlayer);
	}
    
	private void SetEnergizedPlayer(OnPlayerGetEnergized data)
	{
		if (!IsOwner) return;
		if (data.p_ownerId != OwnerId) return;

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
		if (_addPercentageCoroutineIncrease == null)
			return;

		if (_addPercentageCoroutineDecrease != null)
			return;
    
		StopCoroutine(_addPercentageCoroutineIncrease);
		_addPercentageCoroutineIncrease = null;
    
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
					p_percentageValue = _percentagePerSecond * Time.deltaTime * ratio
				});
			}
			yield return null;
		}
	}
}