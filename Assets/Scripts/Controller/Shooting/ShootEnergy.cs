using System;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MyPrint;
using UnityEngine;

public class ShootEnergy : NetworkBusListener
{
	#region Properties

	#endregion
	
	#region Variables

	[Header("References")] 
	[SerializeField] private PlayerEnergy _playerEnergy;

	[Header("Settings")] 
	[SerializeField] private float _value;
	[SerializeField] private float _fireRate = 0.3f;
	
	[Header("Detection Bro")]
	[SerializeField] private float _range = 50f;
	[SerializeField] private float _aimAngle = 0.95f; 
	[SerializeField] private float _aimAngleShoot = 0.9f; 
	[SerializeField] private LayerMask _targetLayer;
	[SerializeField] private Camera _camera;
	private Transform _target = null;
	private NetworkObject _targetNetObj;
	
	private readonly SyncVar<bool> _isAiming = new  SyncVar<bool>(false);
	private readonly SyncVar<bool> _laserActive = new SyncVar<bool>(false);
	private readonly SyncVar<Vector3> _laserTargetPos = new SyncVar<Vector3>();

	private float _nextFireTime = 0f;

	//Actions
	public Action<int> CantThrowEnergy;
	public Action<bool, Vector3> OnDetectBro;
	public Action<bool, Vector3> OnLaserActivate;

	#endregion
	
	#region Fonctions

	public override void OnStartNetwork()
	{
		_isAiming.OnChange += OnAimingChange;
	}

	public override void OnStopNetwork()
	{
		_isAiming.OnChange -= OnAimingChange;
	}

	private void OnDisable()
	{
		OnDetectBro?.Invoke(false, Vector3.zero);
	}
	
	public void TryShoot()
	{
		if (Time.time < _nextFireTime) return;
		
		if (_playerEnergy.CurrentEnergy <= 0)
		{
			CantThrowEnergy?.Invoke(0);
			SetAimingState(false);
			return;
		}
		
		if (_target == null)
		{
			CantThrowEnergy?.Invoke(1);
			SetAimingState(false);
			return;
		}
		
		SetAimingState(true);
		
		_nextFireTime = Time.time + _fireRate;
		
		ConsumeEnergyServerRpc();
	}
	
	[ServerRpc]
	private void ConsumeEnergyServerRpc()
	{
		InvokeEvent(new ConsumeEnergyEvent
		{
			p_player = Owner,
			p_value = -_value
		});
	}
	
	
	public void TryCancelShoot()
	{
		SetAimingState(false);
	}
	
	Transform GetTarget()
	{
		Collider[] targets = Physics.OverlapSphere(
			_camera.transform.position,
			_range,
			_targetLayer
		);

		Transform bestTarget = null;
		float bestScore = _isAiming.Value ? _aimAngleShoot : _aimAngle;

		foreach (Collider col in targets)
		{
			Vector3 dir = (col.transform.position - _camera.transform.position).normalized;

			float dot = Vector3.Dot(_camera.transform.forward, dir);

			if (dot > bestScore)
			{
				bestScore = dot;
				bestTarget = col.transform;
			}
		}

		return bestTarget;
	}
	
	private void Update()
	{
		if (!IsOwner) return;
		
		_target = GetTarget();

		if (_target)
		{
			Vector3 screenPos = _camera.WorldToScreenPoint(_target.position + Vector3.up);
			
			if (!_isAiming.Value)
			{
				InvokeEvent(new OnLaserFired_TUTO());
				OnDetectBro?.Invoke(true, screenPos);
			}
		}
		else
		{
			OnDetectBro?.Invoke(false, Vector3.zero);
			SetAimingState(false);
		}
	}

	void SetAimingState(bool state)
	{
		NetworkObject targetNetObj = _target?.root.GetComponent<NetworkObject>();
		
		SetAimingServerRpc(state, targetNetObj);
	}

	[ServerRpc]
	private void SetAimingServerRpc(bool state, NetworkObject targetNetObj)
	{
		_targetNetObj = targetNetObj;
		_isAiming.Value = state;
	}
	
	private void OnAimingChange(bool prev, bool next, bool asServer)
	{
		if (asServer)
		{
			if (_targetNetObj != null)
				SendEnergyStateObserverRpc(_targetNetObj.OwnerId, next);
			return;
		}

		if (!IsOwner) return;

		if (next && _target != null)
		{
			OnLaserActivate?.Invoke(true, _target.position);
		}
		else
		{
			OnLaserActivate?.Invoke(false, Vector3.zero);
		}
	}

	[ObserversRpc]
	private void SendEnergyStateObserverRpc(int targetOwnerId, bool state)
	{
		InvokeEvent(new OnPlayerGetEnergized
		{
			p_ownerId = targetOwnerId,
			p_state = state
		});
	}
	
	#endregion
}

public struct OnPlayerGetEnergized
{
	public int p_ownerId;
	public bool p_state;
}
