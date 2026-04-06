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
	private int _idActivator;

	[Header("Follow")] 
	[SerializeField] private float _speed = 5f;
	[SerializeField] private float _orbitRadius = 2f;
	[SerializeField] private float _orbitSpeed = 2f;
	private float _orbitAngle;
	private Transform _target;
	private int idActivated;
	private Vector3 _targetPosition;
	private float elapsedTimeUpdateSearch = 0;
	private float timeUpdateSearch = 0.2f;
	
	private Vector3 _startPosition;
	
	[Header("View")]
	[SerializeField] private Transform _view;

	#endregion

	#region Fonctions
	
	private void Awake()
	{
		_startPosition = transform.position;
		GetComponent<SphereCollider>().radius = _activatedRange;
	}

	public override void OnStartNetwork()
	{
		_currentActivatedTime.OnChange += OnActivatedTimerChange;
	}

	private void OnActivatedTimerChange(float prev, float next, bool asServer)
	{
		bool activated = false;
		float ratio = 0;
		
		if (next > 0)
		{
			activated = true;
			ratio = next / _timeToActivate;
		}
		
		InvokeEvent(new DroneActivatedEvent
		{
			p_playerId = _idActivator,
			p_isActivate = activated,
			p_ratioBar = ratio,
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

			UpdateVisualObserversRpc();
        
			return;
		}

		FollowTarget();
	}

	private void LateUpdate()
	{
		if (_pales != null && _IsActivated.Value)
		{
			_pales.Rotate(Vector3.up * _speedPaleRotation * Time.deltaTime);
		}
	}
	
	[ObserversRpc]
	private void UpdateVisualObserversRpc()
	{
		bool isThrower = (_idThrower == LocalConnection.ClientId);

		_view.gameObject.SetActive(!isThrower && !_IsActivated.Value);
	}

	void Activated()
	{
		_IsActivated.Value = true;
		_target = InstanceFinder.ClientManager.Objects.Spawned[_idActivator].transform;
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
		
		transform.position = Vector3.MoveTowards(transform.position, _targetPosition + Vector3.up * 2f, _speed * Time.deltaTime);
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
				_idActivator = player.Owner.ClientId;
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
				_currentActivatedTime.Value = 0;
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
