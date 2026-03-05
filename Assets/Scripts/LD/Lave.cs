using System;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class Lave : NetworkBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private float _damage = 10;
	[SerializeField] private float _timeTickDamage = 1;
	private float elapsedTime = 0;
	
	private EventBus _bus;

	#endregion


	#region Fonctions

	public override void OnStartServer()
	{
		_bus = EventBusInitialiser.instance.Bus;
	}

	private void Update()
	{
		if (!IsServerInitialized) return;

		if (elapsedTime > 0)
			elapsedTime -= Time.deltaTime;
	}

	public void OnTriggerStay(Collider other)
	{
		if (!IsServerInitialized) return;		
		
		if (other.TryGetComponent(out PlayerVisuelBridge player))
		{
			ApplyDamage(player.NetworkObject);
		}
	}

	void ApplyDamage(NetworkObject playerCollision)
	{ 
		if (elapsedTime > 0) return;
		
		elapsedTime =  _timeTickDamage;
		
		Debug.Log("Apply player dmg");

		_bus.InvokeEvent(new PlayerTakeDamageEvent
		{
			playerN = playerCollision,
			value = _damage
		});
	}

	#endregion
}
