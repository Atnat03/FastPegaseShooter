using FishNet.Object;
using UnityEngine;

public class PlayerVisuelBridge : NetworkBehaviour
{
	#region Properties

	public PlayerHealth PlayerHealth => _playerHealth;
	public PlayerEnergy PlayerEnergy => _playerEnergy;
	
	#endregion


	#region Variables

	[SerializeField] private PlayerHealth _playerHealth;
	[SerializeField] private PlayerEnergy _playerEnergy;
	
	#endregion


	#region Fonctions

	
	
	#endregion
}
