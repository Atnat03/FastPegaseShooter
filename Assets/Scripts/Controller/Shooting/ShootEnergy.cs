using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MyPrint;
using UnityEngine;

public struct OnResetEnergizedEvent{}

public class ShootEnergy : NetworkBusListener
{
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
	private bool _resetLock = false;
	
	//Actions
	public Action<int> CantThrowEnergy;
	public Action<bool, Vector3> OnDetectBro;
	public Action<bool, Vector3> OnLaserActivate;
	public Action<bool> OnTPSLaserActivate;

	#endregion
	
	#region Fonctions

	public override void OnStartNetwork()
	{
		_isAiming.OnChange += OnAimingChange;
    
		ListenToEvent<OnResetEnergizedEvent>(_ =>
		{
			_resetLock = true;
			SetAimingState(false);
			if (base.Owner.IsLocalClient)
				ResetEnergizedServerRpc();
        
			if (_unlockCoroutine != null) StopCoroutine(_unlockCoroutine);
			_unlockCoroutine = StartCoroutine(UnlockAfterDelay());
		});
    
		ListenToEvent<OnPlayerRespawnEvent>((_) =>
		{
			if (_unlockCoroutine != null) StopCoroutine(_unlockCoroutine);
			_resetLock = false;
		});
	}

	private Coroutine _unlockCoroutine;

	IEnumerator UnlockAfterDelay()
	{
		yield return new WaitForSeconds(0.5f);
		_resetLock = false;
		_unlockCoroutine = null;
	}

	public override void OnStopNetwork()
	{
		_isAiming.OnChange -= OnAimingChange;
	}

	private void OnDisable()
	{
		OnDetectBro?.Invoke(false, Vector3.zero);
	}

	[ServerRpc]
	private void ResetEnergizedServerRpc()
	{
		_isAiming.Value = false;
		SendEnergyStateObserverRpc(_targetNetObj != null ? _targetNetObj.OwnerId : -1, false);
		_targetNetObj = null;
	}

	public void TryShoot()
	{
		if (_resetLock) return;
		
		if (Time.time < _nextFireTime)
		{
			return;
		}
		
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
		
		if (_resetLock)
		{
			OnDetectBro?.Invoke(false, Vector3.zero);
			return;
		}
		
		_target = GetTarget();
		
		if (_target)
		{
			Vector3 screenPos = _camera.WorldToScreenPoint(_target.position);
			
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
		if (state)
			_targetNetObj = targetNetObj;
    
		_isAiming.Value = state;
	}
	
	private void OnAimingChange(bool prev, bool next, bool asServer)
	{
		if (asServer)
		{
			if (_targetNetObj != null)
				SendEnergyStateObserverRpc(_targetNetObj.OwnerId, next);
        
			if (!next)
				_targetNetObj = null;
        
			return;
		}

		if (IsOwner)
		{
			if (next && _target != null)
				OnLaserActivate?.Invoke(true, _target.position);
			else
				OnLaserActivate?.Invoke(false, Vector3.zero);
		}

		if (!IsOwner)
		{
			if (next)
				OnTPSLaserActivate?.Invoke(true); 
			else
				OnTPSLaserActivate?.Invoke(false);
		}
	}

	[ObserversRpc]
	private void SendEnergyStateObserverRpc(int targetOwnerId, bool state)
	{
		InvokeEvent(new OnPlayerGetEnergized
		{
			p_ownerId = targetOwnerId,
			p_shooterOwnerId = OwnerId,
			p_state = state
		});
	}
	
	#endregion
}

public struct OnPlayerGetEnergized
{
	public int p_ownerId;
	public int p_shooterOwnerId;
	public bool p_state;
}
