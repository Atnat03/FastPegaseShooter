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
	[SerializeField] private GameObject _pingPrefab;
	[SerializeField] private float _cooldownLifePing = 5f;
	[SerializeField] private float _timerBetweenPing = 2f;
	float _elapsedTime = 0f;
	private bool _canPing = true;

	private GameObject _currentPing;

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
		
		if (Physics.Raycast(_cam.transform.position, _cam.transform.forward, out RaycastHit hit))
		{
			_elapsedTime = _timerBetweenPing;

			if (IsServerInitialized)
				AddPingObserverRpc(hit.point);
			else
			{
				AddPingServerRpc(hit.point);
			}
		}
	}

	[ServerRpc]
	void AddPingServerRpc(Vector3 point)
	{
		AddPingObserverRpc(point);
	}


	[ObserversRpc]
	void AddPingObserverRpc(Vector3 pos)
	{
		Destroy(Instantiate(_pingPrefab, pos, Quaternion.identity), _cooldownLifePing);
	}
	
	#endregion

	#region Fonctions

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
