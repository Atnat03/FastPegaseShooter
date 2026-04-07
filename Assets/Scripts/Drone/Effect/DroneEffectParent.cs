using System.Collections.Generic;
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
	
	[Header("View")]
	[SerializeField] private GameObject _view;
	
	#endregion


	#region Fonctions

	private void OnEnable()
	{
		_isActivated.OnChange += OnActivatedChange;
	}

	private void OnActivatedChange(bool prev, bool next, bool asServer)
	{
		if(next)
		{		
			_view.SetActive(true);
		}
	}

	void Update()
	{
		_elapsedTimeApplyEffect -= Time.deltaTime;
		
		if(_elapsedTimeApplyEffect <= 0)
		{
			_elapsedTimeApplyEffect = _applyEffectTimer;
			ApplyEffect();
		}
	}

	protected virtual void ApplyEffect()
	{ }

	public virtual void ApplyDeathEffect()
	{ }

	public void OnTriggerStay(Collider other)
	{
		if (!_isActivated.Value) return;
		
		if(!IsServerInitialized)
			return;

		if (other.TryGetComponent(out PlayerVisuelBridge player))
		{
			PlayerVisuelBridge g = player;
			
			if(!_playerUnderEffect.Contains(g))
				_playerUnderEffect.Add(g);
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
			
			if(!_playerUnderEffect.Contains(g))
				_playerUnderEffect.Remove(g);
		}
	}

	public void Activated()
	{
		GetComponent<SphereCollider>().radius = _radius;
		_isActivated.Value = true;
	}
	
	#endregion
}
