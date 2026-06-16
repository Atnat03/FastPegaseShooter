using System;
using System.Collections;
using FishNet.Object;
using ScriptableObjectsDefinitions;
using UnityEngine;
using UnityEngine.UI;

public class FPSControllerView : NetworkBusListener
{
	#region Variables

	[SerializeField] private FPSController _fps;
	[SerializeField] private PlayerPing _ping;
	[SerializeField] private SoundsDataSO _soundsDataFps;
	[SerializeField] private SoundsDataSO _soundsDataPing;
	[SerializeField] private AudioSource _audioSource;
	[SerializeField] private AudioSource _audioSourceLocal;

	[Header("Footsteps")] 
	[SerializeField] private float _frequency;
	private bool _isTriggered = false;
	
	[Header("Dash")] 
	[SerializeField] private Image _dashCooldownImage;
	[SerializeField] private ParticleSystem _dashParticles;

	private bool _canSoundLand = true;
	
	#endregion

	#region Fonctions

	private void OnEnable()
	{
		_fps.OnDash += Dash;
		_fps.OnUpdateDashCooldown += UpdateDashCooldown;

		_fps.OnFootstep += Footsteps;

		_fps.OnJump += Jump;
		_fps.OnLanding += Landing;

		_fps.OnGrappling += Grappling;

		_fps.OnSlide += BeginSlide;
		_fps.OnEndSlide += EndSlide;
		
		_ping.OnPinging += Pinging;
	}
	
	private void OnDisable()
	{
		_fps.OnDash -= Dash;
		_fps.OnUpdateDashCooldown -= UpdateDashCooldown;

		_fps.OnFootstep -= Footsteps;

		_fps.OnJump -= Jump;
		_fps.OnLanding -= Landing;

		_fps.OnGrappling -= Grappling;
		
		_fps.OnSlide -= BeginSlide;
		_fps.OnEndSlide -= EndSlide;

		_ping.OnPinging -= Pinging;
	}

	private void Pinging(bool isNormal)
	{
		string s = isNormal ? "Normal" : "Enemy";
		SoundManager.PlaySound(_soundsDataPing, s, _audioSource);
	}

	private void Grappling()
	{
		SoundManager.PlaySound(_soundsDataFps, "Grapple", _audioSource);
	}

	private void Landing(float verticalVelocity)
	{
		if (!_canSoundLand || verticalVelocity >-5f) return;
		
		PlaySound("Landing");
		StartCoroutine(LandingSoundBuffer());
	}

	IEnumerator LandingSoundBuffer()
	{
		_canSoundLand = false;
		yield return new WaitForSeconds(0.5f);
		_canSoundLand = true;
	}

	private void Jump()
	{
		PlaySound("Jump");
	}

	private void Footsteps()
	{
		float sin = Mathf.Sin(Time.time * _frequency);
		if (sin > 0.97f && !_isTriggered)
		{
			_isTriggered = true;
			PlaySound("Footstep");
		}else if (_isTriggered && sin < -0.97f)
		{
			_isTriggered = false;
		}
	}


	private void UpdateDashCooldown(float ratio)
	{
		_dashCooldownImage.fillAmount = ratio;
	}

	private void Dash()
	{
		PlaySound("Dash");
		_dashParticles.Play();
	}

	private void BeginSlide()
	{
		PlaySound("Slide");
	}

	private void EndSlide()
	{
		StopSound();
	}

	#region SFX
	
	private void PlaySound(string clip)
	{
		//SoundManager.PlaySoundGlobal(_soundsDataFps, clip, _audioSourceLocal);
		
		if (IsServerInitialized)
		{
			PlaySoundObserversRpc(clip);
		}else
		{
			PlaySoundServerRpc(clip);
		}
	}

	[ServerRpc]
	private void PlaySoundServerRpc(string clip)
	{
		PlaySoundObserversRpc(clip);
	}

	[ObserversRpc]
	private void PlaySoundObserversRpc(string clip)
	{
		SoundManager.PlaySound(_soundsDataFps, clip, _audioSource, transform);
	}

	private void StopSound()
	{
		if (IsServerInitialized)
		{
			StopSoundServerRpc();
		}else
		{
			StopSoundObserversRpc();
		}
	}

	[ServerRpc]
	private void StopSoundServerRpc()
	{
		StopSoundObserversRpc();
	}

	[ObserversRpc]
	private void StopSoundObserversRpc()
	{
		SoundManager.StopSound(_audioSource);
	}
	
	#endregion
	
	#endregion
}
