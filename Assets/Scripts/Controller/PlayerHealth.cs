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
	public Transform p_healThrowPoint;
	public Transform p_healThrowDirection;
	[SerializeField] private float _healthToGive = 30;
	[SerializeField] private float _selfHealingTime = 2;
	[SerializeField] private float _healThrowingTimeThreshold = 1;
	public float p_healThrowRadius = 3;
	[SerializeField] private LayerMask _throwHitLayerMask;
	[SerializeField] private LayerMask _throwHealLayerMask;
	[SerializeField] private float _healingCooldown = 5;
	
	private bool _initialized = false;
	private bool _isCritik = false;
	private float _targetHealthFill;
	private Vector3 _startPos;
	
	private float _healKeyDownTime;
	private float _healActivationTime = float.MinValue;
	private bool _throwActivated;
	[HideInInspector] public Vector3 p_healThrowLandingPos;
	private bool _canThrowHeal = true;
	
	//Action
	public Action<float> OnUpdateHealth;
	public Action OnStartWarning;
	public Action<bool> OnKOPlayer;
	public Action OnTakeDamage;
	
	public Action<float> OnSelfHealing;
	public Action OnThrowingActivation;
	public Action OnThrowing;
	public Action<Vector3> OnHealThrowLanding;
	public Action<float> OnUpdateCooldown;
	
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
			
			float remainingCooldown = _healingCooldown - (Time.time - _healActivationTime);
			if (remainingCooldown > 0)
			{
				OnUpdateCooldown?.Invoke(remainingCooldown / _healingCooldown);
			}
			else
			{
				OnUpdateCooldown?.Invoke(0);
			}
			
			if (Time.time - _healKeyDownTime > _healThrowingTimeThreshold)
			{
				if (!_throwActivated)
				{
					OnThrowingActivation?.Invoke();
					_throwActivated = true;
				}
				
				if(Physics.Raycast(p_healThrowPoint.position, p_healThrowDirection.forward, out RaycastHit hit, 999f, _throwHitLayerMask))
					p_healThrowLandingPos = hit.point;
				else
					p_healThrowLandingPos = p_healThrowPoint.position;
				
				Debug.DrawLine(p_healThrowPoint.position, p_healThrowLandingPos, Color.red, 2);
			}
		}
	}
	
	public void CancelHealThrowing() => _canThrowHeal = false;

	void HealKeyPerformed(InputAction.CallbackContext ctx)
	{
		if(!IsOwner || Time.time - _healActivationTime < _healingCooldown)return;
		
		_healKeyDownTime = Time.time;
	}
	async void HealKeyCanceled(InputAction.CallbackContext ctx)
	{
		if(!IsOwner || Time.time - _healActivationTime < _healingCooldown)return;
		
		if (Time.time - _healKeyDownTime > _healThrowingTimeThreshold) //Throwing heal
		{
			if(_canThrowHeal && p_healThrowLandingPos != p_healThrowPoint.position)
			{
				OnThrowing?.Invoke();
				
				ThrowHealServerRpc(p_healThrowLandingPos, _healthToGive);
				_healActivationTime = Time.time;
			}
		}
		else //Self-healing
		{
			OnSelfHealing?.Invoke(_selfHealingTime);
			AddHealthServerRpc(new AddHealthToPlayer
			{
				p_playerId = OwnerId,
				p_value = _healthToGive,
				p_delay = _selfHealingTime
			});
			_healActivationTime = Time.time;
		}
		
		_healActivationTime = Time.time;
		_throwActivated = false;
		_healKeyDownTime = float.MaxValue;
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
	}

	[ServerRpc(RequireOwnership = false)]
	void ThrowHealServerRpc(Vector3 landingPos, float lifeToAdd)
	{
		Collider[] colliders = Physics.OverlapSphere(landingPos, p_healThrowRadius, _throwHealLayerMask);
		foreach (Collider collider in colliders)
		{
			CustomLogger.HighlightLog(collider.gameObject.name);
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

	[ServerRpc(RequireOwnership = false)]
	public void AddHealthServerRpc(AddHealthToPlayer data)
	{
		AddHealth(data);
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
