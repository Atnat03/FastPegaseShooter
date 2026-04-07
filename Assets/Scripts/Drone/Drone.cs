using System;
using System.Text.RegularExpressions;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MyPrint;
using UnityEngine;

public struct DroneActivatedEvent
{
	public int p_playerId;
	public bool p_isActivate;
	public float p_ratioBar;
}


public class Drone : NetworkBusListener
{
	#region Properties

	#endregion


	#region Variables

	[Header("Vol Stationnaire")] 
	[SerializeField] private float _speedFloat = 2f;
	[SerializeField] private float _amplitudeFloat = 0.5f;	
	[SerializeField] private Transform _pales;
	[SerializeField] private float _speedPaleRotation = 360f;

	[Header("Activated")]
	[SerializeField] private float _timeToActivate = 1f;
	[SerializeField] private bool _activatedRangeDEBUG = false;
	[SerializeField] private float _activatedRange = 3f;
	private readonly SyncVar<float> _currentActivatedTime = new (0);
	private readonly SyncVar<bool> _IsActivated = new (false);
	private int _idThrower;
	private NetworkConnection _activatorConnection; // ✅ Stocker la connexion au lieu de l'ID

	[Header("Follow")] 
	[SerializeField] private float _speed = 5f;
	[SerializeField] private float _heightOffset = 2f;
	private float _orbitAngle;
	private Transform _target;
	private int idActivated;
	private Vector3 _targetPosition;
	private float elapsedTimeUpdateSearch = 0;
	private float timeUpdateSearch = 0.2f;
	
	[Header("Life")]
	[SerializeField] private float _lifeTime = 10f;
	private readonly SyncVar<float> _currentLifetime = new (0);
	private DroneEffectParent _effect;
	
	private Vector3 _startPosition;
		
	
	#endregion

	#region Fonctions
	
	private void Awake()
	{
		_startPosition = transform.position;
		GetComponent<SphereCollider>().radius = _activatedRange;
		_effect = GetComponent<DroneEffectParent>();
	}

	public override void OnStartNetwork()
	{
		_currentActivatedTime.OnChange += OnActivatedTimerChange;
		_currentLifetime.OnChange += OnTimeLifeChange;
	}

	private void OnTimeLifeChange(float prev, float next, bool asServer)
	{
		if (next <= 0)
		{
			if(asServer)
			{
				_effect.ApplyDeathEffect();
				InstanceFinder.ServerManager.Despawn(gameObject);
			}
			
			Cons.Print("Instantiate VFX drone death");

		}
	}

	private void OnActivatedTimerChange(float prev, float next, bool asServer)
	{
		// ✅ Calculer et envoyer au client concerné
		if (_activatorConnection == null || !_activatorConnection.IsValid) return;
		
		bool activated = false;
		float ratio = 0;
		
		if (next > 0)
		{
			activated = true;
			ratio = next / _timeToActivate;
		}
		
		// ✅ Envoyer au client via TargetRpc
		SendActivationUITargetRpc(_activatorConnection, activated, ratio);
	}

	// ✅ Nouveau TargetRpc pour envoyer l'UI au client
	[TargetRpc]
	private void SendActivationUITargetRpc(NetworkConnection target, bool isActivate, float ratioBar)
	{
		// Ce code s'exécute sur le client ciblé
		InvokeEvent(new DroneActivatedEvent
		{
			p_playerId = target.ClientId,
			p_isActivate = isActivate,
			p_ratioBar = ratioBar,
		});
	}

	private void Update()
	{
		if (!IsServerInitialized) return;
    
		if (!_IsActivated.Value)
		{
			float hover = Mathf.Sin(Time.time * _speedFloat) * _amplitudeFloat;

			transform.position = _startPosition + new Vector3(0, 1+hover, 0);
        
			if (_currentActivatedTime.Value > 0)
			{
				_currentActivatedTime.Value -= Time.deltaTime;

				if (_currentActivatedTime.Value <= 0)
				{
					Activated();
				}
			}
			
			return;
		}
		
		_currentLifetime.Value -= Time.deltaTime;

		FollowTarget();
	}

	private void LateUpdate()
	{
		if (_pales != null && _IsActivated.Value)
		{
			_pales.Rotate(Vector3.up * _speedPaleRotation * Time.deltaTime);
		}
	}

	void Activated()
	{
		_IsActivated.Value = true;
		
		_currentLifetime.Value = _lifeTime;
		
		_effect?.Activated();

		SearchTarget();
	}

	void SearchTarget()
	{
		_targetPosition = _target.position;

		elapsedTimeUpdateSearch = timeUpdateSearch;
	}
	
	public void SetThrower(int throwerId)
	{
		_idThrower = throwerId;
	}
	
	private void FollowTarget()
	{
		if (_target == null) return;

		elapsedTimeUpdateSearch -= Time.deltaTime;

		if (elapsedTimeUpdateSearch <= 0f)
		{
			SearchTarget();
		}
		
		Vector3 desiredPosition = _targetPosition + Vector3.up * _heightOffset;

		transform.position = Vector3.Lerp(transform.position, desiredPosition, _speed * Time.deltaTime);	
	}
	
	private void OnTriggerEnter(Collider other)
	{
		if (!IsServerInitialized) return;
		if (_IsActivated.Value) return;

		if (other.TryGetComponent(out PlayerVisuelBridge player))
		{
			if (player.Owner == null || !player.Owner.IsValid) return;
        
			if (player.Owner.ClientId != _idThrower)
			{
				Cons.Print("Start activated Drone", ColorConsole.Blue);
				
				_activatorConnection = player.Owner;
				_target = player.transform;
				
				SendActivationUITargetRpc(_activatorConnection, true, 0f);
				
				_currentActivatedTime.Value = _timeToActivate;
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!IsServerInitialized) return;

		if (other.TryGetComponent(out PlayerVisuelBridge player))
		{
			if (player.Owner == null || !player.Owner.IsValid) return;
        
			if (player.Owner.ClientId != _idThrower)
			{
				Cons.Print("Stop activated Drone", ColorConsole.Blue);
				
				if (_activatorConnection != null && _activatorConnection.IsValid)
				{
					SendActivationUITargetRpc(_activatorConnection, false, 0f);
				}
				
				_currentActivatedTime.Value = 0;
				_activatorConnection = null;
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (_activatedRangeDEBUG)
		{
			Gizmos.color = Color.cornflowerBlue;
			Gizmos.DrawWireSphere(transform.position, _activatedRange);
		}
	}

	#endregion

}