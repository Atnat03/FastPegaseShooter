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

	public void SetMovingAnim(float value) => UpdateAnimationFloatServerRpc("Move", value);
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
			_animator.SetBool(name, value);
		}
	}

	[ObserversRpc]
	private void UpdateAnimationBoolClientRpc(string name, bool value)
	{
		_animator.SetBool(name, value);
	}
	
	[ServerRpc]
	private void UpdateAnimationFloatServerRpc(string name, float value)
	{
		if (IsServerInitialized)
		{
			UpdateAnimationFloatClientRpc(name, value);
		}
		else
		{
			_animator.SetFloat(name, value);
		}
	}

	[ObserversRpc]
	private void UpdateAnimationFloatClientRpc(string name, float value)
	{
		_animator.SetFloat(name, value);
	}
	
	#endregion
}
