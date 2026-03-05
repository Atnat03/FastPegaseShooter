using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour
{
	#region Properties

	public float CurrentHealth => _currentHealth.Value;
	public bool IsDead => _isDead.Value;
	
	#endregion


	#region Variables

	private readonly SyncVar<float> _currentHealth =  new SyncVar<float>();
	private readonly SyncVar<bool> _isDead =  new SyncVar<bool>(false);
	private readonly SyncVar<float> _respawnTimer =  new SyncVar<float>(0);
	[SerializeField] private float _healthBase = 100;
	[SerializeField] private PlayerAnimation _playerAnimation;
	[SerializeField] private float _timeToRespawn = 5;
	private Vector3 _respawnPosition;
	private Quaternion _respawnRotation;

	[Header("UI")]
	[SerializeField] private Image _healthBar;
	[SerializeField] private Image _deathImage;
	
	private EventBus _bus;

	#endregion


	#region Fonctions
	
	public override void OnStartServer()
	{
		_currentHealth.Value = _healthBase;
		
		_bus = EventBusInitialiser.instance.Bus;
		_bus.Subscribe((PlayerTakeDamageEvent data) => TakeDamage(data));
	}

	public override void OnStartClient()
	{
		_currentHealth.OnChange += OnHealthChange;
		_isDead.OnChange += OnDeadChange;
		_respawnTimer.OnChange += OnRespawnTimerChange;
		
		_respawnPosition = transform.position;
		_respawnRotation = transform.rotation;
		
		_deathImage.gameObject.SetActive(false);
	}

	private void Update()
	{
		if (_isDead.Value)
		{
			if (_respawnTimer.Value > 0)
			{
				_respawnTimer.Value -= Time.deltaTime;;
			}
			else
			{
				Respawn();
			}
		}
	}

	[Server]
	void TakeDamage(PlayerTakeDamageEvent data)
	{
		if (data.playerN.ObjectId != NetworkObject.ObjectId) return;	
		
		if (_isDead.Value) return;
		
		float newHealth = _currentHealth.Value - data.damage;

		if (newHealth <= 0)
		{
			Death();
		}
		else
		{
			_currentHealth.Value = newHealth;
		}
	}

	private void Death()
	{
		_currentHealth.Value = 0;
		_isDead.Value = true;
		_respawnTimer.Value = _timeToRespawn;
		
		_bus.InvokeEvent(new OnPlayerDeathEvent
		{
			playerN = NetworkObject
		});
	}

	private void Respawn()
	{
		Debug.Log("Respawn");
		
		_respawnTimer.Value = 0;
		_isDead.Value = false;
		_currentHealth.Value = _healthBase;
		
		transform.position = _respawnPosition;
		transform.rotation = _respawnRotation;
		
		_bus.InvokeEvent(new OnPlayerRespawnEvent
		{
			playerN = NetworkObject
		});
	}
	
	private void OnHealthChange(float prev, float next, bool asServer)
	{
		if (!IsOwner) return;
		
		_healthBar.fillAmount = next / _healthBase;
	}
	
	private void OnDeadChange(bool prev, bool next, bool asServer)
	{
		_playerAnimation.SetDeadAnim(next);
		_deathImage.gameObject.SetActive(next);
	}
	
	private void OnRespawnTimerChange(float prev, float next, bool asServer)
	{ }

	#endregion

	public void Interact()
	{
		Debug.Log("Interact");
		Respawn();
	}
}

public struct PlayerTakeDamageEvent
{
	public NetworkObject playerN;
	public float damage;
}

public struct OnPlayerDeathEvent
{
	public NetworkObject playerN;
}

public struct OnPlayerRespawnEvent
{
	public NetworkObject playerN;
}
