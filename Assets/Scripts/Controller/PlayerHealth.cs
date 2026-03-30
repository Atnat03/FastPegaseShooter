using System;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerHealth : NetworkBusListener
{
	#region Properties

	public float CurrentHealth => _currentHealth.Value;
	public bool IsDead => _isDead.Value;

	public bool IsCritik => _isCritik;
	
	#endregion


	#region Variables

	private readonly SyncVar<float> _currentHealth =  new SyncVar<float>();
	private readonly SyncVar<bool> _isDead =  new SyncVar<bool>(false);
	private readonly SyncVar<float> _respawnTimer =  new SyncVar<float>(0);
	[SerializeField] private float _healthBase = 100;
	[SerializeField] private PlayerAnimation _playerAnimation;
	[SerializeField] private float _timeToRespawn = 5;
	[SerializeField, Range(0f, 1f)] private float _critikStep = 0.5f;
	private bool _initialized = false;
	bool _isCritik = false;
	
	private float _targetHealthFill;


	private Vector3 _startPos;
	
	//Action
	public Action<float> OnUpdateHealth;
	public Action OnStartWarning;
	public Action<bool> OnKOPlayer;
	public Action OnTakeDamage;
	
	#endregion


	#region Fonctions
	
	public override void OnStartServer()
	{
		if (!_initialized)
		{
			_currentHealth.Value = _healthBase;
			_initialized = true;
		}

		ListenToEvent<PlayerTakeDamageEvent>(TakeDamage);
		ListenToEvent<AddHealthFromBarEvent>(AddHealth); 
	}

	public override void OnStartClient()
	{
		_currentHealth.OnChange += OnHealthChange;
		_isDead.OnChange += OnDeadChange;
		_respawnTimer.OnChange += OnRespawnTimerChange;
		
		if (IsOwner)
			_startPos = transform.position;
	}

	private void Update()
	{
		if (IsServerInitialized)
		{
			if (_isDead.Value)
			{
				if (_respawnTimer.Value > 0)
					_respawnTimer.Value -= Time.deltaTime;
				else
					RespawnObserverRpc();
			}
		}
    
		if (IsOwner)
		{
			OnUpdateHealth?.Invoke(_targetHealthFill);
		}
	}


	[Server]
	void TakeDamage(PlayerTakeDamageEvent data)
	{
		if (data.playerN.ObjectId != NetworkObject.ObjectId) return;
		
		if (IsDead) return;
		
		float newHealth = _currentHealth.Value - data.value;

		ApplyVolumeDamagedEffectTargetRpc(Owner);
		
		if (newHealth <= 0)
		{
			Death();
		}
		else
		{
			_currentHealth.Value = newHealth;
		}
	}

	[TargetRpc]
	private void ApplyVolumeDamagedEffectTargetRpc(NetworkConnection target)
	{
		OnTakeDamage?.Invoke();
		
	}


	[Server]
	void AddHealth(AddHealthFromBarEvent data)
	{
		if (_isDead.Value) return;

		float newHealth = (_currentHealth.Value + data.value) > _healthBase ? _healthBase : _currentHealth.Value + data.value;
		_currentHealth.Value = newHealth;
	}

	private void Death()
	{
		_currentHealth.Value = 0;
		_isDead.Value = true;
		_respawnTimer.Value = _timeToRespawn;

		NotifyDeathRpc(NetworkObject);
	}
	
	[ObserversRpc]
	private void RespawnObserverRpc()
	{
		Debug.Log("Respawn");
		
		_respawnTimer.Value = 0;
		_isDead.Value = false;
		_currentHealth.Value = _healthBase;

		transform.position = _startPos;

		NotifyRespawnRpc(NetworkObject);
	}
	
	[ObserversRpc]
	private void NotifyDeathRpc(NetworkObject playerN)
	{
		InvokeEvent(new OnPlayerDeathEvent { playerN = playerN });
	}
	
	[ObserversRpc]
	private void NotifyRespawnRpc(NetworkObject playerN)
	{
		InvokeEvent(new OnPlayerRespawnEvent { playerN = playerN });
	}
	
	private void OnHealthChange(float prev, float next, bool asServer)
	{
		if (!IsOwner) return;
    
		_targetHealthFill = next / _healthBase;

		if (_targetHealthFill <= _critikStep)
		{
			_isCritik = true;
			OnStartWarning?.Invoke();
		}
		else
		{
			_isCritik = false;
		}
	}

	
	private void OnDeadChange(bool prev, bool next, bool asServer)
	{
		_playerAnimation.SetDeadAnim(next);
		OnKOPlayer?.Invoke(next);
	}
	
	private void OnRespawnTimerChange(float prev, float next, bool asServer)
	{ }

	#endregion

	public void Interact()
	{
		Debug.Log("Interact");
		RespawnObserverRpc();
	}
}

public struct PlayerTakeDamageEvent
{
	public NetworkObject playerN;
	public float value;
}

public struct OnPlayerDeathEvent
{
	public NetworkObject playerN;
}

public struct OnPlayerRespawnEvent
{
	public NetworkObject playerN;
}
