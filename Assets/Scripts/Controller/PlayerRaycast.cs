using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRaycast : NetworkBehaviour
{
	#region Properties

	#endregion
	
	#region Variables

	[SerializeField] private float _interactDistance = 3;
	private Camera _camera;
	[SerializeField] private PlayerInput _playerInput;
	
	#endregion
	
	#region Fonctions
	

	public override void OnStartClient()
	{
		if(!IsOwner) return;
		
		_camera = GetComponent<FPSController>().Camera;
		_playerInput = GetComponent<PlayerInput>();

		//_playerInput.actions["Interact"].performed += CheckRaycast;
	}

	private void CheckRaycast(InputAction.CallbackContext obj)
	{
		RaycastHit hit;
		Vector3 origin = _camera.transform.position;
		Vector3 direction = _camera.transform.forward;
		
		Debug.DrawRay(origin, direction * _interactDistance, Color.red);

		if (Physics.Raycast(origin, direction, out hit, _interactDistance))
		{
		
		}
	}

	#endregion
}
