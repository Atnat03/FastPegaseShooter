using System;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MyPrint;
using Unity.Services.Authentication.PlayerAccounts;
using UnityEngine;

public class DroneThrower : NetworkBehaviour
{
	#region Properties

	#endregion


	#region Variables
	
	[SerializeField] private ArmBridgeAnimation _bridgeAnimation;
	[SerializeField] private PlayerEnergy _playerEnergy;
	[SerializeField] private GunSwitching _gunSwitching;
	
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

	public bool _hasDrone = false;
	
	private Drone _currentDroneInTerrain = null;
	
	//Actions
	public Action OnThrowing;
	public Action OnGetDrone;
	
	#endregion


	#region Fonctions

	public override void OnStartNetwork()
	{
		if (IsServerInitialized)
		{
			_hasDrone = true;
			OnGetDrone?.Invoke();
		}
	}

	public void TryThrowDrone()
	{
		if (!_hasDrone) return;
		if (_isCanceled) return;
		if (_target == null) return;
		
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

		_hasDrone = false;
		OnThrowing?.Invoke();
	}

	[ServerRpc]
	public void ThrowDroneServerRpc(NetworkObject targetNetObj)
	{
		if (_currentDroneInTerrain != null)
		{
			InstanceFinder.ServerManager.Despawn(_currentDroneInTerrain.gameObject);
		}
		
		Cons.Print("Drone Lancé !!", ColorConsole.Blue, ConsoleStyle.Bold);
		
		_currentDroneInTerrain = Instantiate(_dronePrefab, _spawnPoint.position, Quaternion.identity);
		InstanceFinder.ServerManager.Spawn(_currentDroneInTerrain.gameObject);
		
		_currentDroneInTerrain.SetTarget(targetNetObj.transform);
	}

	[TargetRpc]
	public void GiveDroneBackTargetRpc(NetworkConnection target)
	{
		Cons.Print("GiveDroneBackTargetRpc " + target.ClientId, ColorConsole.Pink);
		_hasDrone = true;
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

		if (!_hasDrone)
		{
			_uiTarget.SetActive(false);
			return;
		}
		
		_target = GetTarget();

		if (_target)
		{
			Cons.Print("Player trouvé", ColorConsole.Cyan);
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
			Cons.Print("Player pas en range", ColorConsole.Cyan);
			_uiTarget.SetActive(false);
		}
	}

	#endregion
}
