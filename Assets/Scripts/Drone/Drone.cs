using System;
using System.Collections;
using System.Text.RegularExpressions;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MyPrint;
using UnityEngine;

public struct DroneActivatedEvent
{
	public int p_playerId;
	public bool p_isActivate;
	public float p_ratioBar;
}


public class Drone : NetworkBusListener
{
	#region Properties

	public NetworkConnection IdThrower => _idThrower.Value;
	
	#endregion

	#region Variables

	[Header("Vol Stationnaire")] 
	[SerializeField] private float _speedFloat = 2f;
	[SerializeField] private float _amplitudeFloat = 0.5f;	
	[SerializeField] private Transform _pales;
	[SerializeField] private float _speedPaleRotation = 360f;

	[Header("Activated")]
	private readonly SyncVar<bool> _IsActivated = new (false);
	private readonly SyncVar<NetworkConnection> _idThrower = new();
	private NetworkConnection _activatorConnection;

	[Header("Follow")] 
	[SerializeField] private float _speedNoActivated = 2f;
	[SerializeField] private float _speed = 5f;
	[SerializeField] private float _heightOffset = 2f;
	private float _orbitAngle;
	private Transform _target;
	private float elapsedTimeUpdateSearch = 0;
	private float timeUpdateSearch = 0.2f;
	
	[Header("Life")]
	[SerializeField] private float _durationLife = 10f;
	private DroneEffectParent _effect;
	
	//Actions
	public Action OnIdThrowerChange;
	
	#endregion

	#region Fonctions
	
	private void Awake()
	{
		_effect = GetComponent<DroneEffectParent>();
	}

	public override void OnStartNetwork()
	{
		_idThrower.OnChange += OnThrowChange;
	}
	
	private void OnThrowChange(NetworkConnection prev, NetworkConnection next, bool asServer)
	{
		OnIdThrowerChange?.Invoke();
	}

	private void Update()
	{
		if (!IsServerInitialized) return;
		
		if (!_IsActivated.Value)
		{
			FollowTarget(_speedNoActivated);
			
			if(Vector3.Distance(transform.position, _target.position + Vector3.up * _heightOffset) < 1f)
				Activated();

			return;
		}

		FollowTarget(_speed);
	}
	
	private void Die()
	{
		if (!IsServerInitialized) return;

		_effect.ApplyDeathEffect();

		if (_activatorConnection != null && _activatorConnection.IsValid)
		{
			PlayerVisuelBridge player = _activatorConnection.FirstObject.GetComponentInChildren<PlayerVisuelBridge>();
			if (player != null)
			{
				
				DroneThrower thrower = player.PlayerDroneView.DroneThrower;

				if (thrower != null)
				{
					thrower.GiveDroneBackTargetRpc(_activatorConnection);
				}
			}
		}

		InstanceFinder.ServerManager.Despawn(gameObject);
	}

	private void LateUpdate()
	{
		_pales.Rotate(Vector3.up * _speedPaleRotation * Time.deltaTime);
	}

	void Activated()
	{
		_IsActivated.Value = true;
		
		_effect?.Activated();

		StartCoroutine(Living());
	}

	IEnumerator Living()
	{
		yield return new WaitForSeconds(_durationLife);
		
		Die();
	}

	public void SetTarget(Transform target)
	{
		_target = target;
		_activatorConnection = _target.root.GetComponent<NetworkObject>().Owner;
	}
	
	private void FollowTarget(float speed)
	{
		if (_target == null) return;
		
		Vector3 desiredPosition = _target.position + Vector3.up * _heightOffset;
		
		transform.position = Vector3.Lerp(transform.position, desiredPosition, speed * Time.deltaTime);
	}
	
	#endregion

}