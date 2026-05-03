using System;
using System.Collections.Generic;
using Controller;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MyPrint;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BorneGunSelection : NetworkBusListener
{
	#region Properties

	#endregion


	#region Variables

	[Header("Zone")]
	[SerializeField] private Vector3 _zoneSize;
	[SerializeField] private Transform _zoneMesh;
	[SerializeField] private bool _showGIZMOS = true;
	private List<PlayerVisuelBridge> _playerList = new List<PlayerVisuelBridge>();
	private BoxCollider _collider;
	private readonly SyncVar<int> _numberPlayer = new SyncVar<int>(0);
	private readonly SyncVar<bool> _canOpenSelect = new SyncVar<bool>(false);
	
	#endregion


	#region Fonctions

	private void Awake()
	{
		_collider = GetComponent<BoxCollider>();
		_collider.size = _zoneSize;
		_zoneMesh.localScale = _zoneSize;
	}
	
	public override void OnStartServer()
	{
		_numberPlayer.OnChange += OnNumberPlayerChange;
	}

	public override void OnStartClient()
	{
		ListenToEvent<OnPlayerInteract>(PlayerInteract);
	}

	private void PlayerInteract(OnPlayerInteract data)
	{
		if (_canOpenSelect.Value)
		{
			if(IsServerInitialized)
			{
				AllPlayerInZoneObserversRpc();
			}
			else
			{
				AllPlayerInZoneServerRpc();
			}
		}
	}

	private void OnNumberPlayerChange(int prev, int next, bool asServer)
	{
		if (asServer)
		{
			if (next == InstanceFinder.ServerManager.Clients.Count)
			{

				_canOpenSelect.Value = true;
			}
			else
			{
				_canOpenSelect.Value = false;
			}
			CanInteractToOpenObserversRpc(_canOpenSelect.Value);
		}
	}

	[ServerRpc]
	private void AllPlayerInZoneServerRpc()
	{
		AllPlayerInZoneObserversRpc();
	}

	[ObserversRpc]
	void AllPlayerInZoneObserversRpc()
	{
		Cons.Print("All player in zone", ColorConsole.Cyan);
		InvokeEvent(new OnAllPlayerAtBorne());
	}

	[ObserversRpc]
	void CanInteractToOpenObserversRpc(bool isOpen)
	{
		InvokeEvent(new OnAllPlayerCanSelectGun{p_open = isOpen});
	}
	
	public void OnTriggerEnter(Collider other)
	{
		if (!IsServerInitialized) return;
		
		if (other.TryGetComponent(out PlayerVisuelBridge player))
		{
			_playerList.Add(player);
			_numberPlayer.Value++;
		}
	}
	
	public void OnTriggerExit(Collider other)
	{
		if (!IsServerInitialized) return;
		
		if (other.TryGetComponent(out PlayerVisuelBridge player))
		{
			if(_playerList.Contains(player))
			{
				_playerList.Remove(player);
				_numberPlayer.Value--;
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (_showGIZMOS)
		{
			Gizmos.color = Color.cornflowerBlue;
			Gizmos.DrawWireCube(transform.position, _zoneSize);
		}
	}

	#endregion
}

public struct OnAllPlayerAtBorne
{ }

public struct OnAllPlayerCanSelectGun
{
	public bool p_open;
}