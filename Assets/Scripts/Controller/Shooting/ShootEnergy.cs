using System;
using MyPrint;
using UnityEngine;

public class ShootEnergy : NetworkBusListener
{
	#region Properties

	#endregion


	#region Variables

	[Header("References")] 
	[SerializeField] private GunSwitching _gunSwitching;
	[SerializeField] private PlayerEnergy _playerEnergy;

	[Header("Settings")] 
	[SerializeField] private float _value;
	[SerializeField] private float _fireRate = 0.3f;
	
	[Header("Detection Bro")]
	[SerializeField] private float _range = 50f;
	[SerializeField] private float _aimAngle = 0.95f; 
	[SerializeField] private LayerMask _targetLayer;
	[SerializeField] private Camera _camera;
	private Transform _target = null;

	private float _nextFireTime = 0f;

	//Actions
	public Action<bool> OnSetUpColor;
	public Action CantThrowEnergy;
	public Action<bool, Vector3> OnDetectBro;

	#endregion
	
	#region Fonctions

	private void OnDisable()
	{
		OnDetectBro?.Invoke(false, Vector3.zero);
	}

	void Start()
	{
		OnSetUpColor?.Invoke(_gunSwitching.IsMainGun);
	}

	public void TryShoot()
	{
		if (Time.time < _nextFireTime) return;
		if (_target == null) return;
		
		if (_playerEnergy.CurrentEnergy <= 0)
		{
			CantThrowEnergy?.Invoke();
			return;
		}

		_nextFireTime = Time.time + _fireRate;

		Cons.Print("Try ConsumeEnergyEvent ", ColorConsole.Blue);

		InvokeEvent(new ConsumeEnergyEvent
		{
			p_player = Owner,
			p_value = -_value
		});
	}
	
	Transform GetTarget()
	{
		Collider[] targets = Physics.OverlapSphere(
			_camera.transform.position,
			_range,
			_targetLayer
		);

		Transform bestTarget = null;
		float bestScore = _aimAngle;

		foreach (Collider col in targets)
		{
			Vector3 dir = (col.transform.position - _camera.transform.position).normalized;

			float dot = Vector3.Dot(_camera.transform.forward, dir);

			if (dot > bestScore)
			{
				bestScore = dot;
				bestTarget = col.transform;
			}
		}

		return bestTarget;
	}
	
	private void Update()
	{
		if (!IsOwner) return;
		
		_target = GetTarget();

		if (_target)
		{
			Vector3 screenPos = _camera.WorldToScreenPoint(_target.position + Vector3.up);
			OnDetectBro?.Invoke(true, screenPos);
		}
		else
		{
			OnDetectBro?.Invoke(false, Vector3.zero);
		}

	}
	
	#endregion
}
