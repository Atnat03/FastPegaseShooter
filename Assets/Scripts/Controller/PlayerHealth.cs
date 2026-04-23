using System;
using System.Threading.Tasks;
using CustomConsole.Runtime.Logger;
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

	[Header("Healing")]
	[SerializeField] private PlayerEnergy _playerEnergy;
	public Transform p_healThrowPoint;
	public Transform p_healThrowDirection;
	[SerializeField] private float _healthToGive = 30;
	public float p_healThrowRadius = 3;
	[SerializeField] private float _healThrowCost = 10;
	[SerializeField] private LayerMask _throwHitLayerMask;
	[SerializeField] private LayerMask _throwHealLayerMask;
	
	private bool _initialized = false;
	private bool _isCritik = false;
	private float _targetHealthFill;
	private Vector3 _startPos;

	private bool _isHealKeyDown;
	[HideInInspector] public Vector3 p_healThrowLandingPos;
	private bool _canThrowHeal = true;
	
	//Action
	public Action<float> OnUpdateHealth;	
	public Action OnStartWarning;
	public Action<bool> OnKOPlayer;
	public Action OnTakeDamage;
	
	public Action OnThrowingVisualActivation;
	public Action OnThrowing;
	public Action<Vector3> OnHealThrowLanding;
	
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
			if (_isHealKeyDown)
			{
				if(Physics.Raycast(p_healThrowPoint.position, p_healThrowDirection.forward, out RaycastHit hit, 999f, _throwHitLayerMask, QueryTriggerInteraction.Ignore))
					p_healThrowLandingPos = hit.point;
				else
					p_healThrowLandingPos = p_healThrowPoint.position;
				
				//Debug.DrawLine(p_healThrowPoint.position, p_healThrowLandingPos, Color.red, 2);
			}
		}
	}
	
	public void CancelHealThrowing() => _canThrowHeal = false;

	void HealKeyPerformed(InputAction.CallbackContext ctx)
	{
		if(!IsOwner)return;

		if (_playerEnergy.CurrentEnergy < _healThrowCost)
		{
			CustomLogger.ImportantLog($"Energy amount : {_playerEnergy.CurrentEnergy}");
			return;
		}

		OnThrowingVisualActivation?.Invoke();
		_isHealKeyDown = true;
	}
	void HealKeyCanceled(InputAction.CallbackContext ctx)
	{
		if(!(IsOwner || _isHealKeyDown))return;
		
		if (_playerEnergy.CurrentEnergy < _healThrowCost)
		{
			return;
		}
		
		if(_canThrowHeal && p_healThrowLandingPos != p_healThrowPoint.position)
		{
			OnThrowing?.Invoke();
			
			ThrowHealServerRpc(p_healThrowLandingPos, _healthToGive, Owner);
		}
		
		_isHealKeyDown = false;
		_canThrowHeal = true;
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
		OnUpdateHealth?.Invoke(1f);
	}

	[ServerRpc(RequireOwnership = false)]
	void ThrowHealServerRpc(Vector3 landingPos, float lifeToAdd, NetworkConnection throwerConnection)
	{
		InvokeEvent(new ModifyEnergyEvent
		{
			p_player = throwerConnection,
			p_value = -_healThrowCost
		});
		Collider[] colliders = Physics.OverlapSphere(landingPos, p_healThrowRadius, _throwHealLayerMask);
		foreach (Collider collider in colliders)
		{
			if (collider != null && collider.TryGetComponent(out PlayerVisuelBridge visualBridge))
			{
				visualBridge.PlayerHealth.AddHealth(new AddHealthToPlayer
				{
					p_delay = 0,
					p_playerId = visualBridge.OwnerId,
					p_value = lifeToAdd
				});
			}
		}

		ShowHealThrowObserverRpc(landingPos);
	}

	[Server]
	async void AddHealth(AddHealthToPlayer data)
	{
		if (_isDead.Value || data.p_playerId != OwnerId) return;
		if(data.p_delay != 0) await Task.Delay(Mathf.RoundToInt(data.p_delay * 1000));

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

	[ObserversRpc]
	private void ShowHealThrowObserverRpc(Vector3 landingPos)
	{
		OnHealThrowLanding?.Invoke(landingPos);
	}
	
	private void OnHealthChange(float prev, float next, bool asServer)
	{
		if (!IsOwner) return;
    
		CustomLogger.ImportantLog("healChange");
		_targetHealthFill = next / _healthBase;
		
		OnUpdateHealth?.Invoke(_targetHealthFill);

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
	public float p_delay;
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
