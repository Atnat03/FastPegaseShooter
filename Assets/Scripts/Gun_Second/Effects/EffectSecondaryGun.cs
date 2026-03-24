using System;
using UnityEngine;

public abstract class EffectSecondaryGun : MonoBehaviour
{
	private float _speed;
	private Vector3 _targetPoint;
	protected RaycastHit _hit;
	private Element _element;
	
	public void SetUpVariables(float speed, Vector3 targetPoint, int element)
	{
		_speed = speed;
		_targetPoint = targetPoint;
		_element = (Element)element;
	}
	
	private void FixedUpdate()
	{
		DetectCollision();
		Move();
	}

	private void Move()
	{
		transform.Translate(transform.forward * (_speed * Time.deltaTime), Space.World);
	}
	
	private void DetectCollision()
	{
		if (Physics.Raycast(transform.position, transform.forward, out _hit,
			    _speed * Time.fixedDeltaTime, ~LayerMask.NameToLayer("Owner"), 
			    QueryTriggerInteraction.Ignore))
		{
			_hit.transform.TryGetComponent(out IDamagable damagable);
			ApplyEffect(damagable);

			Destroy(gameObject);
		}
	}

	protected virtual void ApplyEffect(IDamagable damagable)
	{
		if (damagable == null) return;
		
		Debug.Log("Freeze effect");
	}
}
