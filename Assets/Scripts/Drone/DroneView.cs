using System;
using FishNet;
using FishNet.Object;
using UnityEngine;

public class DroneView : NetworkBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private DroneEffectParent _droneEffect;
	[SerializeField] private Drone _drone;
	
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
		_droneEffect.OnActivatedDrone += OnActivatedDrone;
		_droneEffect.OnUpdateEffect += UpdatePosition;
		_drone.OnIdThrowerChange += UpdateVisibility;
	}
	
	public override void OnStartClient()
	{
		UpdateVisibility();
	}
	
	private void UpdateVisibility()
	{
		if (!IsClientInitialized) return;

		var localConnection = InstanceFinder.ClientManager.Connection;

		bool isThrower = _drone.IdThrower == localConnection;

		_arrow.gameObject.SetActive(!isThrower);
		_activated.gameObject.SetActive(!isThrower);
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
		_droneEffect.OnActivatedDrone -= OnActivatedDrone;
		_droneEffect.OnUpdateEffect -= UpdatePosition;
		_drone.OnIdThrowerChange -= UpdateVisibility;
	}
}
