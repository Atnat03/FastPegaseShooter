using System;
using FishNet;
using FishNet.Object;
using MyPrint;
using UnityEngine;

public class DroneView : NetworkBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private DroneEffectParent _droneEffect;
	[SerializeField] private Drone _drone;
	[SerializeField] private MeshRenderer _meshRenderer;
	[SerializeField] private Material[] _materials;
	
	[Header("ApplysEffect")]
	[SerializeField] private ParticleSystem _applyEffect;
	[SerializeField] private Material[] _materialsParticles;

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
	
	private void SetUpColor(bool isPositive)
	{
		_meshRenderer.material = isPositive ? _materials[0] : _materials[1];
		
		ParticleSystemRenderer psRenderer = _applyEffect.GetComponent<ParticleSystemRenderer>();
		psRenderer.material = isPositive ? _materialsParticles[0] : _materialsParticles[1];
		
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
