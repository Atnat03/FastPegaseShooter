using System;
using FishNet.Object;
using UnityEngine;

public class DroneView : NetworkBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private DroneEffectParent _drone;
	
	[Header("As to Activated")]
	[SerializeField] private Transform _arrow;
	[SerializeField] private GameObject _activated;
	
	[Header("ApplysEffect")]
	[SerializeField] private ParticleSystem _applyEffect;

	private RaycastHit hit;
	
	#endregion
	
	#region Fonctions

	private void OnEnable()
	{
		_drone.OnActivatedDrone += OnActivatedDrone;
		_drone.OnUpdateEffect += UpdatePosition;
	}
	private void OnActivatedDrone(float radius)
	{
		_arrow.gameObject.SetActive(false);
		_activated.gameObject.SetActive(false);
		
		_applyEffect.gameObject.SetActive(true);
		_applyEffect.transform.localScale = Vector3.one * 4f * radius;
	}
	
	private void UpdatePosition()
	{
		if (Physics.Raycast(transform.position, Vector3.down, out hit, 1000, LayerMask.NameToLayer("Ground"), QueryTriggerInteraction.Ignore))
		{
			_applyEffect.transform.position = hit.point;
		}
	}
	
	#endregion
	
	private void OnDisable()
	{
		_drone.OnActivatedDrone -= OnActivatedDrone;
		_drone.OnUpdateEffect -= UpdatePosition;
	}

}
