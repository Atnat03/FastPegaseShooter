using System;
using FishNet.Example.Scened;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPing : NetworkBehaviour
{
	#region Variables

	[SerializeField] private Camera _cam;
	[SerializeField] private PlayerInput _playerInput;
	
	[Header("Ping")]
	[SerializeField] private Ping _pingNormalPrefab;
	[SerializeField] private Ping _pingEnemyPrefab;
	[SerializeField] private float _cooldownLifePing = 5f;
	[SerializeField] private float _timerBetweenPing = 2f;
	
	float _elapsedTime = 0f;
	private bool _canPing = true;

	private GameObject _currentPing;
	
	public Action<bool> OnPinging;

	#endregion
	
	#region Fonctions
	
	private void Update()
	{
		if (_elapsedTime > 0)
		{
			_elapsedTime -= Time.deltaTime;
			_canPing = false;

			if (_elapsedTime <= 0)
			{
				_canPing = true;
			}
		}
	}

	private void AddPing(InputAction.CallbackContext obj)
	{
		if (!IsOwner) return;
		if (!_canPing) return;
		
		if (Physics.Raycast(_cam.transform.position, _cam.transform.forward, out RaycastHit hit, 10000, ~LayerMask.NameToLayer("NotWallridable") ,QueryTriggerInteraction.Ignore))
		{
			_elapsedTime = _timerBetweenPing;

			bool isNormal = !hit.collider.TryGetComponent(out EnemyCore e);

			if (IsServerInitialized)
				AddPingObserverRpc(hit.point + hit.normal * 0.1f, isNormal);
			else
			{
				AddPingServerRpc(hit.point + hit.normal * 0.1f, isNormal);
			}
		}
	}

	[ServerRpc]
	void AddPingServerRpc(Vector3 point, bool isNormal)
	{
		AddPingObserverRpc(point, isNormal);
	}


	[ObserversRpc]
	void AddPingObserverRpc(Vector3 pos, bool isNormal)
	{
		Ping prefab = isNormal ? _pingNormalPrefab : _pingEnemyPrefab;
		
		Ping ping = Instantiate(prefab, pos, Quaternion.identity).GetComponent<Ping>();

		if (Camera.main != null)
			ping.SetTarget(Camera.main.transform);

		OnPinging?.Invoke(isNormal);
		
		Destroy(ping.gameObject, _cooldownLifePing);
	}
	
	private void OnEnable()
	{
		_playerInput.actions["Ping"].performed += AddPing;
	}

	private void On()
	{
		_playerInput.actions["Ping"].performed -= AddPing;
	}

	#endregion
}
