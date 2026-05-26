using System;
using System.Collections;
using MyPrint;
using NUnit.Framework;
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
	public Capacity p_capacityData;
	public float p_percentageValue;
}

public struct OnUseCapacity
{
	public Capacity p_capacityData;
}


public class PlayerCapacity : MonoBusListener
{
	#region Properties

	public bool CanChargedShoot => _tirChargeCapaData.p_currentNumberCapacity > 0 && _canChargedShoot;
	public bool CanDrone => _droneCapaData.p_currentNumberCapacity > 0 && _canDrone;
	public bool CanHeal => _healCapaData.p_currentNumberCapacity > 0 && _canHeal;
	
	#endregion

	#region Variables

	[Header("Tir chargé")] 
	[SerializeField] private CapacityData _tirChargeCapaData;
	
	[Header("Drone")]
	[SerializeField] private CapacityData _droneCapaData;
	
	[Header("Heal")]
	[SerializeField] private CapacityData _healCapaData;

	public bool _canChargedShoot = true;
	public bool _canDrone = true;
	public bool _canHeal = true;
	
	//Actions
	public Action<CapacityData> OnUpdateCapacity;
	
	#endregion

	#region Fonctions

	private void OnEnable()
	{
		ListenToEvent<OnUseCapacity>(UseCapacity);
		ListenToEvent<OnAddPercentageCapactity>(AddPercentage);
	}

	void UseCapacity(OnUseCapacity data)
	{
		switch (data.p_capacityData)
		{
			case Capacity.ChargedShoot:
				UseACapa(ref _tirChargeCapaData);
				break;
			case Capacity.Drone:
				UseACapa(ref _droneCapaData);
				break;
			case Capacity.Heal:
				UseACapa(ref _healCapaData);
				break;
		}
	}

	void AddPercentage(OnAddPercentageCapactity data)
	{
		switch (data.p_capacityData)
		{
			case Capacity.ChargedShoot:
				CheckPercentageCapa(ref _tirChargeCapaData, data.p_percentageValue);
				break;
			
			case Capacity.Drone:
				CheckPercentageCapa(ref _droneCapaData, data.p_percentageValue);
				break;
			
			case Capacity.Heal:
				CheckPercentageCapa(ref _healCapaData, data.p_percentageValue);
				break;
		}
	}

	void CheckPercentageCapa(ref CapacityData data, float valuePercentage)
	{
		if (data.p_currentNumberCapacity <= data.p_maxNumberCapacity)
		{
			data.p_currentPercentageCapacity += valuePercentage;
			
			if (data.p_currentPercentageCapacity >= 100)
			{
				data.p_currentPercentageCapacity = 0;
				data.p_currentNumberCapacity += 1;
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

	#endregion
}
