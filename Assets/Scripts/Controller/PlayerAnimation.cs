using System;
using System.Collections;
using FishNet.Component.Animating;
using FishNet.Object;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
	#region Variables

	[Header("Animator")]
	[SerializeField]Animator _animator;
	[SerializeField]NetworkAnimator _networkAnimator;

	[Header("IK")] 
	[SerializeField] private Transform _leftArmTarget;
	[SerializeField] private Transform _rightArmTarget;
	
	[SerializeField] Transform _leftArmShootTransform;
	[SerializeField] Transform _rightArmShootTransform;
	[SerializeField] Transform _leftArmRunShootTransform;
	[SerializeField] Transform _rightArmRunShootTransform;

	private IKAnimationState _currentIKState = IKAnimationState.Shooting;
	
	#endregion

	#region Fonctions

	#region Animator
	public void SetMovingAnim(bool isMoving) => _animator.SetBool("Move", isMoving);
	public void SetJumpAnim(bool isJumping) => _animator.SetBool("Jump", isJumping);
	public void ChangeAirState(bool isGrounded)
	{
		_animator.SetBool("Falling", !isGrounded);
		_animator.SetBool("Grounded", isGrounded);
	}
	
	#endregion

	#region IK Bones

	public void SetShootingIKPos()
	{
		if (_currentIKState == IKAnimationState.Shooting) return;
		
		_currentIKState = IKAnimationState.Shooting;
		
		StartCoroutine(ChangeTargetArmPos(_leftArmShootTransform, _leftArmRunShootTransform));
	}
	
	public void SetRunningIKPos()
	{
		if (_currentIKState == IKAnimationState.Running) return;
		
		_currentIKState = IKAnimationState.Running;
		
		StartCoroutine(ChangeTargetArmPos(_leftArmShootTransform, _leftArmRunShootTransform));
	}

	IEnumerator ChangeTargetArmPos(Transform leftArmFinal, Transform rightArmFinal)
	{
		Vector3 finalPos_LeftArm = leftArmFinal.position;
		Vector3 finalPos_RightArm = rightArmFinal.position;
		
		Quaternion finalRot_LeftArm = leftArmFinal.rotation;
		Quaternion finalRot_RightArm = rightArmFinal.rotation;

		float transitionDuration = 0.2f;
		float elasped = 0;

		while (elasped < transitionDuration)
		{
			elasped += Time.deltaTime;
			
			_leftArmTarget.position = Vector3.Lerp(_leftArmTarget.position, finalPos_LeftArm, elasped / transitionDuration);
			_rightArmTarget.position = Vector3.Lerp(_rightArmTarget.position, finalPos_RightArm, elasped / transitionDuration);
			
			_leftArmTarget.rotation = Quaternion.Lerp(_leftArmTarget.rotation, finalRot_LeftArm, elasped / transitionDuration);
			_rightArmTarget.rotation = Quaternion.Lerp(_rightArmTarget.rotation, finalRot_RightArm, elasped / transitionDuration);
			yield return null;
		}
	}

	#endregion
	
	#endregion

	enum IKAnimationState
	{
		Shooting,
		Running
	}
}
