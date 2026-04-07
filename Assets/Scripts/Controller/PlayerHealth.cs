using System;
using System.Threading.Tasks;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.InputSystem;

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
	[SerializeField] private float _timeToRespawn = 5;
	[SerializeField, Range(0f, 1f)] private float _critikStep = 0.5f;
	[SerializeField] private PlayerInput _playerInputAction;
	[SerializeField] private PlayerAnimation _playerAnimation;

	[Header("Healing")] [SerializeField] private float _selfHealingTime;
	[SerializeField] private float _healThrowingTimeThreshold = 1;
	
	private bool _initialized = false;
	private bool _isCritik = false;
	private float _targetHealthFill;
	private Vector3 _startPos;
	
	private float _healKeyDownTime;
	private float _healConsoPercent;
	
	//Action
	public Action<float> OnUpdateHealth;
	public Action OnStartWarning;
	public Action<bool> OnKOPlayer;
	public Action OnTakeDamage;
	
	public Action<float> OnSelfHealing;
	
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
		ListenToEvent<AddHealthToPlayer>(AddHealth);
	}

	public override void OnStartClient()
	{
		_currentHealth.OnChange += OnHealthChange;
		_isDead.OnChange += OnDeadChange;
		_respawnTimer.OnChange += OnRespawnTimerChange;
		
		if (IsOwner)
			_startPos = transform.position;
	}

	private void OnEnable()
	{
		_playerInputAction.actions["Heal"].performed += HealKeyPerformed;
		_playerInputAction.actions["Heal"].canceled += HealKeyCanceled;
	}

	private void OnDisable()
	{
		_playerInputAction.actions["Heal"].performed -= HealKeyPerformed;
		_playerInputAction.actions["Heal"].canceled -= HealKeyCanceled;
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

	void HealKeyPerformed(InputAction.CallbackContext ctx)
	{
		_healKeyDownTime = Time.time;
	}
	async void HealKeyCanceled(InputAction.CallbackContext ctx)
	{
		if (Time.time - _healKeyDownTime > _healThrowingTimeThreshold) //Throwing heal
		{
			
		}
		else //Self-healing
		{
			OnSelfHealing?.Invoke(_selfHealingTime);
		}
		
		_healKeyDownTime = float.MaxValue;
	}

	[Server]
	void TakeDamage(PlayerTakeDamageEvent data)
	{
		if (data.p_playerN.ObjectId != NetworkObject.ObjectId) return;
		
		if (IsDead) return;
		
		float newHealth = _currentHealth.Value - data.p_value;

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
	void AddHealth(AddHealthToPlayer data)
	{
		if (_isDead.Value || data.p_playerId != OwnerId) return;

		float newHealth = (_currentHealth.Value + data.p_value) > _healthBase ? _healthBase : _currentHealth.Value + data.p_value;
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
		InvokeEvent(new OnPlayerDeathEvent { p_playerN = playerN });
	}
	
	[ObserversRpc]
	private void NotifyRespawnRpc(NetworkObject playerN)
	{
		InvokeEvent(new OnPlayerRespawnEvent { p_playerN = playerN });
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

public struct AddHealthToPlayer
{
	public int p_playerId; 
	public float p_value;
}

public struct PlayerTakeDamageEvent
{
	public NetworkObject p_playerN;
	public float p_value;
}

public struct OnPlayerDeathEvent
{
	public NetworkObject p_playerN;
}

public struct OnPlayerRespawnEvent
{
	public NetworkObject p_playerN;
}
