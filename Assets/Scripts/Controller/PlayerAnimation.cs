using System;
using FishNet.Component.Animating;
using FishNet.Object;
using UnityEngine;

public class PlayerAnimation : NetworkBehaviour
{
	#region Variables

	[SerializeField]Animator _animator;
	[SerializeField]NetworkAnimator _networkAnimator;
	
	[SerializeField] private GunSwitching _switching;
	[SerializeField] private SkinnedMeshRenderer _renderer;
	[SerializeField] private Material[] _materialList;
	
	#endregion

	#region Fonctions

	public override void OnStartClient()
	{
		_renderer.material = _materialList[_switching.IsPositive ? 0 : 1];
	}

	public void SetMovingAnim(bool isMoving) => UpdateAnimationBoolServerRpc("Move", isMoving);
	public void SetMovingBackwardAnim(bool isMoving) => UpdateAnimationBoolServerRpc("MoveBackward", isMoving);
	public void SetJumpAnim(bool isJumping) => UpdateAnimationBoolServerRpc("Jump", isJumping);
	public void SetFallingAnim(bool isFalling) => UpdateAnimationBoolServerRpc("Falling", isFalling);
	public void SetGroundedAnim(bool isGrounded) => UpdateAnimationBoolServerRpc("Grounded", isGrounded);
	public void SetDeadAnim(bool isDead) => UpdateAnimationBoolServerRpc("Dead", isDead);
	public void SetDashAnim() => UpdateAnimationTriggerServerRpc("Dash");
	public void SetSlideAnim(bool isSlide) => UpdateAnimationBoolServerRpc("Slide", isSlide);
	public void SetShootAnim() => UpdateAnimationTriggerServerRpc("Shoot");

	public void ChangeAirState(bool isGrounded)
	{
		UpdateAnimationBoolServerRpc("Falling", !isGrounded);
		UpdateAnimationBoolServerRpc("Grounded", isGrounded);
	}

	[ServerRpc]
	private void UpdateAnimationTriggerServerRpc(string name)
	{
		if (IsServerInitialized)
		{
			UpdateAnimationTriggerClientRpc(name);
		}
		else
		{
			_animator.SetTrigger(name);
		}
	}

	[ObserversRpc]
	private void UpdateAnimationTriggerClientRpc(string name)
	{
		_animator.SetTrigger(name);
	}
	
	[ServerRpc]
	private void UpdateAnimationBoolServerRpc(string name, bool value)
	{
		if (IsServerInitialized)
		{
			UpdateAnimationBoolClientRpc(name, value);
		}
		else
		{
			_animator.SetTrigger(name);
		}
	}

	[ObserversRpc]
	private void UpdateAnimationBoolClientRpc(string name, bool value)
	{
		_animator.SetBool(name, value);
	}
	
	#endregion
}
