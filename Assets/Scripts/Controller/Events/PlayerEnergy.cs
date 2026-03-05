using FishNet.Object;
using UnityEngine;

public class PlayerEnergy : NetworkBehaviour
{
	#region Properties
	
	#endregion


	#region Variables

	[SerializeField] private FPSController _fps;
	
	#endregion


	#region Fonctions

	public void AddEnergy(float energy)
	{
		
	}
	
	#endregion
	
	public struct OnEnergyEvent
	{
		public NetworkObject player;
	}
}
