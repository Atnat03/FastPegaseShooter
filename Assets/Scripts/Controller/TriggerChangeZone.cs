using System;
using MyPrint;
using UnityEngine;

public class TriggerChangeZone : NetworkBusListener
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private int _newZone = 0;
	
	#endregion


	#region Fonctions
	
	public void OnTriggerEnter(Collider other)
	{
		if (!IsServerInitialized) return;
		
		if (other.TryGetComponent(out PlayerVisuelBridge player))
		{
			Cons.Print("ChangeZone");
			
			InvokeEvent(new OnPlayerChangeZone
			{
				playerId = player.Owner.ClientId,
				newZone = _newZone
			});
		}
	}
	
	
	#endregion
}

public struct OnPlayerChangeZone
{
	public int playerId;
	public int newZone;
}