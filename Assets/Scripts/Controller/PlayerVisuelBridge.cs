using FishNet.Object;
using UnityEngine;

public class PlayerVisuelBridge : NetworkBehaviour
{
	#region Properties

	public PlayerHealth PlayerHealth => _playerHealth;
	public GunSwitching PlayerGun => _playerGun;
	public PlayerDroneView PlayerDroneView => _playerDroneView;
	
	public FPSController FPSController => _fpsController;
	public PlayerPositionCaster PlayerPositionCaster => _playerPositionCaster;
	
	#endregion


	#region Variables

	[SerializeField] private PlayerHealth _playerHealth;
	[SerializeField] private GunSwitching _playerGun;
	[SerializeField] private PlayerDroneView _playerDroneView;
	[SerializeField] private FPSController _fpsController;
	[SerializeField] private PlayerPositionCaster _playerPositionCaster;
	
	#endregion


	#region Fonctions

	
	
	#endregion
}
