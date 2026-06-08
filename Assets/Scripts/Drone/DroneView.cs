using System;
using FishNet;
using FishNet.Object;
using MyPrint;
using UnityEngine;

public class DroneView : NetworkBehaviour
{
	#region Variables

	[SerializeField] private DroneEffectParent _droneEffect;
	[SerializeField] private Drone _drone;
	[SerializeField] private GameObject[] _models;
	
	[Header("ApplysEffect")]
	[SerializeField] private ParticleSystem[] _applyEffects;
	private ParticleSystem _applyEffectAssigned;

	private RaycastHit hit;
	
	#endregion
	
	#region Fonctions

	public override void OnStartClient()
	{
		SetUpColor(_drone.IsPositive);
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
		_applyEffectAssigned.gameObject.SetActive(true);
		_applyEffectAssigned.transform.localScale = Vector3.one * 4f * radius;
	}
	
	private void UpdatePosition()
	{
		if (Physics.Raycast(transform.position, Vector3.down, out hit, 1000, LayerMask.NameToLayer("Ground"), QueryTriggerInteraction.Ignore))
		{
			_applyEffectAssigned.transform.position = hit.point;
		}
	}
	
	private void SetUpColor(bool isPositive)
	{
		_models[0].gameObject.SetActive(isPositive);
		_models[1].gameObject.SetActive(!isPositive);
		
		/*
		ParticleSystemRenderer psRenderer = _applyEffectAssigned.GetComponent<ParticleSystemRenderer>();
		psRenderer.material = isPositive ? _materialsParticles[0] : _materialsParticles[1];
		*/
		
		_applyEffectAssigned = isPositive ? _applyEffects[0] : _applyEffects[1];
		
		Cons.Print("Set up drone color : " + isPositive);
	}
	
	#endregion
	
	private void OnEnable()
	{
		_droneEffect.OnActivatedDrone += OnActivatedDrone;
		_droneEffect.OnUpdateEffect += UpdatePosition;
		_drone.OnIdThrowerChange += UpdateVisibility;

		_drone.OnSetUpColor += SetUpColor;
	}
	
	private void OnDisable()
	{
		_droneEffect.OnActivatedDrone -= OnActivatedDrone;
		_droneEffect.OnUpdateEffect -= UpdatePosition;
		_drone.OnIdThrowerChange -= UpdateVisibility;
		
		_drone.OnSetUpColor -= SetUpColor;
	}
}
