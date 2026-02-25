using System;
using FishNet.Component.Animating;
using FishNet.Object;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
	#region Variables

	[SerializeField]Animator _animator;
	[SerializeField]NetworkAnimator _networkAnimator;
	
	#endregion

	#region Fonctions
	
	public void SetMovingAnim(bool isMoving) => _animator.SetBool("Move", isMoving);
	public void SetJumpAnim(bool isJumping) => _animator.SetBool("Jump", isJumping);
	public void SetFallingAnim(bool isFalling) => _animator.SetBool("Falling", isFalling);
	public void SetGroundedAnim(bool isGrounded) => _animator.SetBool("Grounded", isGrounded);

	public void ChangeAirState(bool isGrounded)
	{
		_animator.SetBool("Falling", !isGrounded);
		_animator.SetBool("Grounded", isGrounded);
	}
	
	#endregion
}
