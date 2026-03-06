using System;
using System.Collections;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using ScriptableObjectsDefinitions;
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
	private bool _initialized = false;
	
	[Header("UI")]
	[SerializeField] private Image _healthBar;
	[SerializeField] private Image _deathImage;
	private float _targetHealthFill;
	
	[SerializeField] private SoundsDataSO _soundsData;
	private AudioSource _audioSource;
	
	private EventBus _bus;

	#endregion


	#region Fonctions
	
	
	public override void OnStartServer()
	{
		if (!_initialized)
		{
			_currentHealth.Value = _healthBase;
			_initialized = true;
		}
		
		_bus = EventBusInitialiser.instance.Bus;
		_bus.Subscribe((PlayerTakeDamageEvent data) => TakeDamage(data));
		_bus.Subscribe((AddHealthFromBarEvent data) => AddHealth(data));
	}

	public override void OnStartClient()
	{
		_bus = EventBusInitialiser.instance.Bus;
		
		_currentHealth.OnChange += OnHealthChange;
		_isDead.OnChange += OnDeadChange;
		_respawnTimer.OnChange += OnRespawnTimerChange;
		
		if (IsOwner)
		{
			_respawnPosition = transform.position;
			_respawnRotation = transform.rotation;
		}
		
		_audioSource = GetComponent<AudioSource>();
		
		_deathImage.gameObject.SetActive(false);
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
			_healthBar.fillAmount = Mathf.Lerp(_healthBar.fillAmount, _targetHealthFill, Time.deltaTime * 25);
		}
	}

	[Server]
	void TakeDamage(PlayerTakeDamageEvent data)
	{
		if (data.playerN.ObjectId != NetworkObject.ObjectId) return;
		
		if (IsDead) return;
		
		float newHealth = _currentHealth.Value - data.value;

		PlayHurtSoundObserverRpc();

		if (newHealth <= 0)
		{
			Death();
		}
		else
		{
			_currentHealth.Value = newHealth;
		}
	}

	[ObserversRpc]
	private void PlayHurtSoundObserverRpc()
	{
		AudioClip clip = SoundManager.GetAudioClip(_soundsData, "Hurt");
		SoundManager.PlaySound(clip, _audioSource);
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
		
		transform.position = _respawnPosition;
		transform.rotation = _respawnRotation;

		NotifyRespawnRpc(NetworkObject);
	}
	
	[ObserversRpc]
	private void NotifyDeathRpc(NetworkObject playerN)
	{
		_bus.InvokeEvent(new OnPlayerDeathEvent { playerN = playerN });
	}
	
	[ObserversRpc]
	private void NotifyRespawnRpc(NetworkObject playerN)
	{
		_bus.InvokeEvent(new OnPlayerRespawnEvent { playerN = playerN });
	}
	
	private void OnHealthChange(float prev, float next, bool asServer)
	{
		if (!IsOwner) return;
		
		_targetHealthFill = next / _healthBase;
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
