using FishNet.Object;
using UnityEngine;

public class PlayerVisuelBridge : NetworkBehaviour
{
	#region Properties

	public PlayerHealth PlayerHealth => _playerHealth;
	
	#endregion


	#region Variables

	[SerializeField] private PlayerHealth _playerHealth;
	
	#endregion


	#region Fonctions

	
	
	#endregion
}
