using FishNet.Object;
using UnityEngine;

public class PlayerVisuelBridge : NetworkBehaviour
{
	#region Variables

	[SerializeField] private PlayerHealth _playerHealth;
	[SerializeField] private GunSwitching _playerGun;
	[SerializeField] private PlayerDroneView _playerDroneView;
	[SerializeField] private FPSController _fpsController;
	[SerializeField] private PlayerPositionCaster _playerPositionCaster;
	
	#endregion
}
