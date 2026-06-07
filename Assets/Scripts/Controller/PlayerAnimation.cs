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
	
	[Header("IK Arm")]
	[Header("IK Targets")]
	[SerializeField] private Transform rig1Target;
	[SerializeField] private Transform rig1Hint;

	[SerializeField] private float followSpeed = 20f;

	private Transform _rightHandSource;
	private Transform _rightHintSource;
	
	#endregion

	#region Fonctions

	public override void OnStartClient()
	{
		_renderer.material = _materialList[_switching.IsPositive ? 0 : 1];
	}

	public void SetMovingHAnim(float value) => UpdateAnimationFloatServerRpc("MoveH", value);
	public void SetMovingVAnim(float value) => UpdateAnimationFloatServerRpc("MoveV", value);
	public void SetJumpAnim() => UpdateAnimationTriggerServerRpc("Jump");
	public void SetDoubleJumpAnim() => UpdateAnimationTriggerServerRpc("DoubleJump"); 
	public void SetDeadAnim(bool isDead) => UpdateAnimationBoolServerRpc("Dead", isDead);
	public void SetDashAnim() => UpdateAnimationTriggerServerRpc("Dash");
	public void SetSlideAnim(bool isSlide) => UpdateAnimationTriggerServerRpc(isSlide ? "Slide" : "StopSlide");
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

	#region  IK

	void LateUpdate()
	{
		if (_rightHandSource != null)
		{
			rig1Target.position = Vector3.Lerp(rig1Target.position, _rightHandSource.position, followSpeed * Time.deltaTime);
			rig1Target.rotation = Quaternion.Slerp(rig1Target.rotation, _rightHandSource.rotation, followSpeed * Time.deltaTime);
		}

		if (_rightHintSource != null)
			rig1Hint.position = Vector3.Lerp(rig1Hint.position, _rightHintSource.position, followSpeed * Time.deltaTime);
	}

	public void SetWeaponTargets(Transform rightHand, Transform leftHand)
	{
		rig1Target.SetParent(rightHand);
		rig1Target.localPosition = Vector3.zero;
		rig1Target.localRotation = Quaternion.identity;
	}

	public void SetIKWeight(float weight)
	{
		GetComponentInChildren<UnityEngine.Animations.Rigging.Rig>().weight = weight;
	}

	#endregion
}
