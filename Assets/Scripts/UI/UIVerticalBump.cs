using UnityEngine;

public class UIVerticalBump : MonoBehaviour
{
	#region Variables

	[SerializeField] private Rigidbody _playerRB;
	[SerializeField] float _verticalBumpForce = 0.015f;
	[SerializeField] float _smoothTimeVerticalBump = 0.01f;
	
	private float _bumpPosition;
	private float _bumpVelocity;
	private float _lastVerticalVelocity;
	
	#endregion


	#region Fonctions

	void LateUpdate()
	{
		Vector3 finalPosition = Vector3.zero;
		
		#region Vertical Bump (Jump/Landing)
        
		float verticalVelocity = _playerRB.linearVelocity.y;

		float targetBump = -verticalVelocity * (_verticalBumpForce);

		_bumpPosition = Mathf.SmoothDamp(_bumpPosition, targetBump, ref _bumpVelocity, _smoothTimeVerticalBump);

		finalPosition.y += _bumpPosition;
		#endregion

		transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition, Time.deltaTime * 5);
	}
	
	#endregion
}
