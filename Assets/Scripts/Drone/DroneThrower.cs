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
	
	[Header("Throw")]
	[SerializeField] private Drone _dronePrefab;
	[SerializeField] private Transform _spawnPoint;

	private float _currentChargeTime = 0f;
	private bool _isCharging = false;
	private bool _isCanceled = false;

	private readonly SyncVar<bool> _canThrow = new(false);

	public bool _hasDrone = false;
	
	private DroneBullet _currentDroneInTerrain = null;
	
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
		
		_isCharging = false;
		
		ThrowDroneServerRpc();

		_hasDrone = false;
		OnThrowing?.Invoke();
	}

	[ServerRpc]
	void ThrowDroneServerRpc()
	{
		if (_currentDroneInTerrain != null)
		{
			InstanceFinder.ServerManager.Despawn(_currentDroneInTerrain.gameObject);
		}
		
		Cons.Print("Drone Lancé !!", ColorConsole.Blue, ConsoleStyle.Bold);
    
		/*
		InstanceFinder.ServerManager.Spawn(drone.gameObject);
		_currentDroneInTerrain = drone;

		drone.SetDrone(_dronePrefab, Owner, _playerEnergy);
    
		drone.GetComponent<Rigidbody>().AddForce(_spawnPoint.forward * force, ForceMode.Impulse);*/
	}

	[TargetRpc]
	public void GiveDroneBackTargetRpc(NetworkConnection target)
	{
		Cons.Print("GiveDroneBackTargetRpc " + target.ClientId, ColorConsole.Pink);
		_hasDrone = true;
		OnGetDrone?.Invoke();
	}
	
	public float range = 20f;
	public float aimAngle = 0.95f; 
	public LayerMask targetLayer;
	public Camera _camera;
	public GameObject _uiTarget;
	
	Transform GetTarget()
	{
		Collider[] targets = Physics.OverlapSphere(
			_camera.transform.position,
			range,
			targetLayer
		);

		Transform bestTarget = null;
		float bestScore = aimAngle;

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

	private Transform target = null;
	
	private void Update()
	{
		if (!IsOwner) return;
		
		target = GetTarget();

		if (target)
		{
			Cons.Print("Player trouvé", ColorConsole.Cyan);
			_uiTarget.SetActive(true);
			
			Vector3 screenPos = _camera.WorldToScreenPoint(target.position + Vector3.up);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				_uiTarget.transform.parent as RectTransform,
				screenPos,
				_camera,
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
