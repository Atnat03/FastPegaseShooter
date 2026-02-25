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
	public void PlayJumpAnim() => _networkAnimator.SetTrigger("Jump");
	public void PlayLandingAnim(bool isFalling) => _animator.SetBool("Falling", isFalling);

	#endregion
}
