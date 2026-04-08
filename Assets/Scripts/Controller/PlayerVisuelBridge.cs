using FishNet.Object;
using UnityEngine;

public class PlayerVisuelBridge : NetworkBehaviour
{
	#region Properties

	public PlayerHealth PlayerHealth => _playerHealth;
	public GunSwitching PlayerGun => _playerGun;
	
	#endregion


	#region Variables

	[SerializeField] private PlayerHealth _playerHealth;
	[SerializeField] private GunSwitching _playerGun;
	
	#endregion


	#region Fonctions

	
	
	#endregion
}
