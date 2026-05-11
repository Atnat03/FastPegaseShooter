using FishNet.Object;
using UnityEngine;

public class PlayerVisuelBridge : NetworkBehaviour
{
	#region Properties

	public PlayerHealth PlayerHealth => _playerHealth;
	public GunSwitching PlayerGun => _playerGun;
	public PlayerDroneView PlayerDroneView => _playerDroneView;
	
	#endregion


	#region Variables

	[SerializeField] private PlayerHealth _playerHealth;
	[SerializeField] private GunSwitching _playerGun;
	[SerializeField] private PlayerDroneView _playerDroneView;
	
	#endregion


	#region Fonctions

	
	
	#endregion
}
