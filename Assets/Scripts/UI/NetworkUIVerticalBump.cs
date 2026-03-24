using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

public class NetworkUIVerticalBump : NetworkBehaviour
{
	#region Variables
	
	private Rigidbody _playerRB;
	[SerializeField] float _verticalBumpForce = 0.015f;
	[SerializeField] float _smoothTimeVerticalBump = 0.01f;
	
	private float _bumpPosition;
	private float _bumpVelocity;
	private float _lastVerticalVelocity;
	private Vector3 _startPosition;
	
	#endregion


	#region Fonctions

	public override void OnStartClient()
	{
		base.OnStartClient();

		NetworkObject player = InstanceFinder.ClientManager.Connection.FirstObject;

		if (player != null)
		{
			_playerRB = player.GetComponent<Rigidbody>();
		}

		_startPosition = transform.position;
	}


	void LateUpdate()
	{
		if (!IsOwner && _playerRB == null)
			return;
		
		Debug.Log("Bump");

		Vector3 finalPosition = _startPosition;

		float verticalVelocity = _playerRB.linearVelocity.y;

		float targetBump = -verticalVelocity * _verticalBumpForce;

		_bumpPosition = Mathf.SmoothDamp(_bumpPosition, targetBump, ref _bumpVelocity, _smoothTimeVerticalBump);

		finalPosition.y += _bumpPosition;
		
		transform.position = Vector3.Lerp(transform.position, finalPosition, Time.deltaTime * 5);
		
	}
	
	#endregion
}
