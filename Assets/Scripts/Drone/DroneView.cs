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
	}
	
	private void OnActivatedDrone(float radius)
	{
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
