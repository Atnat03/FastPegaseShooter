using System;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MyPrint;
using Unity.Services.Authentication.PlayerAccounts;
using UnityEngine;

public class DroneThrower : NetworkBusListener
{
	#region Properties

	#endregion
	
	#region Variables
	
	[SerializeField] private ArmBridgeAnimation _bridgeAnimation;
	[SerializeField] private GunSwitching _gunSwitching;
	[SerializeField] private PlayerCapacity _playerCapacity;
	
	[Header("Throw")]
	[SerializeField] private Drone _dronePrefab;
	[SerializeField] private Transform _spawnPoint;

	[Header("Detection Bro")]
	[SerializeField] private float _range = 50f;
	[SerializeField] private float _aimAngle = 0.95f; 
	[SerializeField] private LayerMask _targetLayer;
	[SerializeField] private Camera _camera;
	[SerializeField] private GameObject _uiTarget;
	private Transform _target = null;
	
	private float _currentChargeTime = 0f;
	private bool _isCharging = false;
	private bool _isCanceled = false;

	private readonly SyncVar<bool> _canThrow = new(false);
	
	private Drone _currentDroneInTerrain = null;

	public bool p_unlockCapa = true;
	
	//Actions
	public Action OnThrowing;
	public Action OnGetDrone;
	
	#endregion
	
	#region Fonctions

	public override void OnStartNetwork()
	{
		if (IsServerInitialized)
		{
			OnGetDrone?.Invoke();
		}
	}

	public void TryThrowDrone()
	{
		if (_isCanceled) return;
		if (_target == null) return;
		if (!_playerCapacity.CanDrone) return;
		if (!p_unlockCapa) return;
		
		InvokeEvent(new OnUseCapacity
		{
			p_capacityData = Capacity.Drone
		});
		
		InvokeEvent(new OnDroneUsed_TUTO());
		
		_isCharging = false;
		
		NetworkObject targetNetObj = _target.GetComponent<NetworkObject>()
		                             ?? _target.GetComponentInParent<NetworkObject>();
		
		if (_bridgeAnimation != null)
		{
			_bridgeAnimation.StartThrowDrone(targetNetObj);
			_gunSwitching.IGunMain.TryCancelShooting();
			_gunSwitching.ISurchargeMain.StopReload();
		}
		else
		{
			ThrowDroneServerRpc(targetNetObj);
		}

		OnThrowing?.Invoke();
	}
	
	[ServerRpc]
	public void ThrowDroneServerRpc(NetworkObject targetNetObj)
	{
		if (_currentDroneInTerrain != null)
		{
			InstanceFinder.ServerManager.Despawn(_currentDroneInTerrain.gameObject);
		}
		
		_currentDroneInTerrain = Instantiate(_dronePrefab, _spawnPoint.position, Quaternion.identity);
		InstanceFinder.ServerManager.Spawn(_currentDroneInTerrain.gameObject);
		
		_currentDroneInTerrain.SetTarget(targetNetObj.transform, _gunSwitching.IsPositive);
	}

	[TargetRpc]
	public void GiveDroneBackTargetRpc(NetworkConnection target)
	{
		OnGetDrone?.Invoke();
	}
	
	Transform GetTarget()
	{
		Collider[] targets = Physics.OverlapSphere(
			_camera.transform.position,
			_range,
			_targetLayer
		);

		Transform bestTarget = null;
		float bestScore = _aimAngle;

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
			_uiTarget.SetActive(true);
			
			Vector3 screenPos = _camera.WorldToScreenPoint(_target.position + Vector3.up);
			Canvas canvas = _uiTarget.GetComponentInParent<Canvas>();

			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				canvas.transform as RectTransform,
				screenPos,
				canvas.worldCamera,
				out Vector2 localPos
			);

			_uiTarget.GetComponent<RectTransform>().localPosition = localPos;
		}
		else
		{
			_uiTarget.SetActive(false);
		}
	}

	#endregion
}
