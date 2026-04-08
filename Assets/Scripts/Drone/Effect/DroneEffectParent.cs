using System;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MyPrint;
using UnityEngine;


public abstract class DroneEffectParent : NetworkBusListener
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private float _radius = 2f;
	[SerializeField] private float _applyEffectTimer = 0.5f;
	private float _elapsedTimeApplyEffect = 0;
	
	private readonly SyncVar<bool> _isActivated = new(false);
	protected List<PlayerVisuelBridge> _playerUnderEffect = new ();
	
	//Actions
	public Action<float> OnActivatedDrone;
	public Action OnUpdateEffect;
	public Action<NetworkConnection> OnEnableDrone;
	
	#endregion


	#region Fonctions

	private void OnEnable()
	{
		_isActivated.OnChange += OnActivatedChange;
	}

	public override void OnStartNetwork()
	{
		UpdateStartVisuelObserverRpc();
	}

	[ObserversRpc]
	void UpdateStartVisuelObserverRpc()
	{
		OnEnableDrone?.Invoke(Owner);
	}

	private void OnActivatedChange(bool prev, bool next, bool asServer)
	{
		if(next)
		{
			OnActivatedDrone?.Invoke(_radius);
		}
	}

	void Update()
	{
		if (!IsServerInitialized) return;
		
		_elapsedTimeApplyEffect -= Time.deltaTime;
		
		if(_elapsedTimeApplyEffect <= 0)
		{
			_elapsedTimeApplyEffect = _applyEffectTimer;
			ApplyEffect();
		}
	}

	protected virtual void ApplyEffect()
	{
		UpdateViewObserversRpc();
	}

	[ObserversRpc]
	private void UpdateViewObserversRpc()
	{
		OnUpdateEffect?.Invoke();
	}

	public virtual void ApplyDeathEffect()
	{
		UpdateDeathEffect();
	}

	[ObserversRpc]
	private void UpdateDeathEffect()
	{
		foreach (PlayerVisuelBridge p in _playerUnderEffect)
		{
			StopApplicateEffect(p);
		}
	}


	protected virtual void StopApplicateEffect(PlayerVisuelBridge playerVisuelBridge)
	{
		Cons.Print("StopApplicateEffect base ", ColorConsole.Orange);

		SetUnderDroneTargetRpc(playerVisuelBridge.Owner, false);
	}


	public void OnTriggerStay(Collider other)
	{
		if (!_isActivated.Value) return;
		
		if(!IsServerInitialized)
			return;

		if (other.TryGetComponent(out PlayerVisuelBridge player))
		{
			PlayerVisuelBridge g = player;
			
			if(!_playerUnderEffect.Contains(g))
			{
				SetUnderDroneTargetRpc(player.Owner, true);
				_playerUnderEffect.Add(g);
			}
		}
	}
	
	public void OnTriggerExit(Collider other)
	{
		if (!_isActivated.Value) return;
		
		if(!IsServerInitialized)
			return;

		if (other.TryGetComponent(out PlayerVisuelBridge player))
		{
			PlayerVisuelBridge g = player;
			
			if(_playerUnderEffect.Contains(g))
			{
				_playerUnderEffect.Remove(g);
				SetUnderDroneTargetRpc(player.Owner, false);
			}
		}
	}
	
	[TargetRpc]
	private void SetUnderDroneTargetRpc(NetworkConnection target, bool state)
	{
		Cons.Print("SetUnderDroneTargetRpc ", ColorConsole.Orange);
		PlayerVisuelBridge players = target.FirstObject.GetComponentInChildren<PlayerVisuelBridge>();
		if (players.IsOwner)
		{
			players.PlayerDroneView.SetInfoUnderDrone(state);
			players.PlayerGun.IGunMain.SetFireRate(-1);
		}
	}
	
	public void Activated()
	{
		GetComponent<SphereCollider>().radius = _radius;
		_isActivated.Value = true;
	}
	
	#endregion
}
