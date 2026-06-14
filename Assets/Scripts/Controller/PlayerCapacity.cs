using System;
using System.Collections;
using MyPrint;
using NUnit.Framework;
using Tuto;
using UnityEngine;

public enum Capacity
{
	ChargedShoot, Drone, Heal
}

[Serializable]
public struct CapacityData
{
	public Capacity p_capacity;
	public int p_currentNumberCapacity;
	public int p_maxNumberCapacity;
	public float p_currentPercentageCapacity;
	public float p_ghostCooldown;
}

public struct OnAddPercentageCapactity
{
	public int p_targetOwnerId;
	public Capacity p_capacityData;
	public float p_percentageValue;
}

public struct OnUseCapacity
{
	public Capacity p_capacityData;
}

public class PlayerCapacity : NetworkBusListener
{
	#region Properties

	public bool CanChargedShoot => _tirChargeCapaData.p_currentNumberCapacity > 0 && _canChargedShoot;
	public bool CanDrone => _droneCapaData.p_currentNumberCapacity > 0 && _canDrone;
	public bool CanHeal => _healCapaData.p_currentNumberCapacity > 0 && _canHeal;
	
	#endregion

	#region Variables

	[SerializeField] private GunSwitching _gun;
	
	[Header("Tir chargé")] 
	[SerializeField] private CapacityData _tirChargeCapaData;
	
	[Header("Drone")]
	[SerializeField] private CapacityData _droneCapaData;
	
	[Header("Heal")]
	[SerializeField] private CapacityData _healCapaData;

	public bool _canChargedShoot = true;
	public bool _canDrone = true;
	public bool _canHeal = true;

	private PlayerTuto _playerTuto;
	
	//Actions
	public Action<CapacityData> OnUpdateCapacity;
	public Action<CapacityData> OnUseCapacity;
	
	#endregion

	#region Fonctions

	private void Awake()
	{
		ListenToEvent<OnUseCapacity>(UseCapacity);
		ListenToEvent<OnAddPercentageCapactity>(AddPercentage);
		ListenToEvent<OnCapacityUnlocked>(OnUnlocked);

		_playerTuto = GetComponent<PlayerTuto>();
	}

	void UseCapacity(OnUseCapacity data)
	{
		switch (data.p_capacityData)
		{
			case Capacity.ChargedShoot:
				UseACapa(ref _tirChargeCapaData);
				InvokeEvent(new OnUseChargedShoot_Dialogue{isPositive = _gun.IsPositive});
				break;
			case Capacity.Drone:
				UseACapa(ref _droneCapaData);
				InvokeEvent(new OnUseDrone_Dialogue{isPositive = _gun.IsPositive});
				break;
			case Capacity.Heal:
				UseACapa(ref _healCapaData);
				InvokeEvent(new OnUseHeal_Dialogue{isPositive = _gun.IsPositive});
				break;
		}
	}

	void AddPercentage(OnAddPercentageCapactity data)
	{
		if (data.p_targetOwnerId != OwnerId) return;
		
		switch (data.p_capacityData)
		{
			case Capacity.ChargedShoot:
				CheckPercentageCapa(ref _tirChargeCapaData, data.p_percentageValue, _playerTuto.IsCapaUnlock(Capacity_TUTO.ChargedShoot));
				break;
			
			case Capacity.Drone:
				CheckPercentageCapa(ref _droneCapaData, data.p_percentageValue, _playerTuto.IsCapaUnlock(Capacity_TUTO.Drone));
				break;
			
			case Capacity.Heal:
				CheckPercentageCapa(ref _healCapaData, data.p_percentageValue, _playerTuto.IsCapaUnlock(Capacity_TUTO.Heal));
				break;
		}
	}

	void CheckPercentageCapa(ref CapacityData data, float valuePercentage, bool isUnlocked)
	{
		if (!isUnlocked) return;
		
		if (data.p_currentNumberCapacity <= data.p_maxNumberCapacity)
		{
			data.p_currentPercentageCapacity += valuePercentage;
			
			if (data.p_currentPercentageCapacity >= 100)
			{
				data.p_currentPercentageCapacity = 0;
				data.p_currentNumberCapacity += 1;
				
				InvokeEvent(new PlayUISound{keySound = "GetCapa"});
			}
		}
		else
		{
			data.p_currentPercentageCapacity = 100;
		}
		
		data.p_currentPercentageCapacity = Mathf.Clamp(data.p_currentPercentageCapacity, 0, 100);
		
		OnUpdateCapacity?.Invoke(data);
	}

	void UseACapa(ref CapacityData data)
	{
		data.p_currentNumberCapacity
			= Mathf.Clamp(
				data.p_currentNumberCapacity - 1,
				0,
				data.p_maxNumberCapacity);
		
		OnUpdateCapacity?.Invoke(data);

		OnUseCapacity?.Invoke(data);
		
		StartCoroutine(CooldownCapa(data));
	}

	IEnumerator CooldownCapa(CapacityData data)
	{
		SetCanUseCapa(data, false);
		
		yield return new WaitForSeconds(data.p_ghostCooldown);
		
		SetCanUseCapa(data, true);
	}

	void SetCanUseCapa(CapacityData data, bool state)
	{
		switch (data.p_capacity)
		{
			case Capacity.ChargedShoot:
				_canChargedShoot = state;
				break;
			case Capacity.Drone:
				_canDrone = state;
				break;
			case Capacity.Heal:
				_canHeal = state;
				break;
		}
	}
	
	void OnUnlocked(OnCapacityUnlocked data)
	{
		switch (data.capacity)
		{
			case Capacity_TUTO.ChargedShoot:
				OnUpdateCapacity?.Invoke(_tirChargeCapaData);
				break;
			case Capacity_TUTO.Drone:
				OnUpdateCapacity?.Invoke(_droneCapaData);
				break;
			case Capacity_TUTO.Heal:
				OnUpdateCapacity?.Invoke(_healCapaData);
				break;
		}
	}

	public void SetStartChargeCapacities(int numberCharge)
	{
		_tirChargeCapaData.p_currentNumberCapacity = numberCharge;
		_droneCapaData.p_currentNumberCapacity = numberCharge;
		_healCapaData.p_currentNumberCapacity = numberCharge;
		
		OnUseCapacity?.Invoke(_tirChargeCapaData);
		OnUseCapacity?.Invoke(_droneCapaData);
		OnUseCapacity?.Invoke(_healCapaData);
	}
	
	#endregion
}

public struct OnCapacityUnlocked
{
	public Capacity_TUTO capacity;
}
