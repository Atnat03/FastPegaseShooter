using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomConsole.Runtime.Logger;
using MyPrint;
using ScriptableObjectsDefinitions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthView : MonoBehaviour
{
	#region Variables

	[Header("References")]
	[SerializeField] private PlayerHealth _playerHealth;
	[SerializeField] private DroneThrower _droneThrower;
	
	[Header("UI")]
	[SerializeField] private float _healthVisualFillingSpeed = 1;
	[SerializeField] private Image _healthBar;
	[SerializeField] private CanvasGroup _damagedImage;
	[SerializeField] private Color[] _colorBar;
	[SerializeField] private Color[] _colorBar2;
	
	[Header("Dead")]
	[SerializeField] private GameObject _normalCanva;
	[SerializeField] private GameObject _deathCanva;
	[SerializeField] private TextMeshProUGUI _deathTimer;
	[SerializeField] private Camera _normalCamera;
	[SerializeField] private Camera _deathCamera;
	[SerializeField] private Transform[] _deathCameraOffsets;
	
	[Header("Healing")]
	[SerializeField] private LineRenderer _healingTrajectoryLine;
	[SerializeField] private GameObject _healingThrowPosObj;
	[SerializeField] private GunSwitching gunSwitching;
	private int currentGunID;
	
	[Header("Sound")]
	[SerializeField] private SoundsDataSO _soundsData;
	private AudioSource _audioSource;
	
	float _elapsedTimeShowWarning = 0;
	bool _isShowedWarning = false;
	bool healLineComplete = false;

	private float _healTargetFillAmount = 1;
	
	#endregion
	
	#region Fonctions

	private void Start()
	{
		_deathCanva.gameObject.SetActive(false);
		_deathCamera.gameObject.SetActive(false);
		_audioSource = GetComponent<AudioSource>();
		_healingTrajectoryLine.enabled = false;
		_healingTrajectoryLine.positionCount = 4;
		_healingThrowPosObj = Instantiate(_healingThrowPosObj, Vector3.zero, Quaternion.identity);
		_healingThrowPosObj.SetActive(false);
		
	}
	
	private void UpdateHealth(float targetFill)
	{
		_healTargetFillAmount = targetFill;

		if (targetFill > 0.5f)
		{
			_healthBar.material.SetColor("_Color", _colorBar[0]);
			_healthBar.material.SetColor("_Color2", _colorBar2[0]);
		}
		else if (targetFill is <= 0.5f and >= 0.25f)
		{
			_healthBar.material.SetColor("_Color", _colorBar[1]);
			_healthBar.material.SetColor("_Color2", _colorBar2[1]);
		}
		else
		{
			if (targetFill < 0.25f)
			{
				_healthBar.material.SetColor("_Color", _colorBar[2]);
				_healthBar.material.SetColor("_Color2", _colorBar2[2]);
			}
		}
	}

	private Coroutine _koCoroutine;

	private void KoPlayerUI(bool state, float duration)
	{
		_deathCanva.gameObject.SetActive(state);
		_deathCamera.gameObject.SetActive(state);
		
		if (_koCoroutine != null)
		{
			StopCoroutine(_koCoroutine);
			_koCoroutine = null;
		}

		if (state)
		{
			_koCoroutine = StartCoroutine(KoAnimation(duration));
			SoundManager.PlaySound(_soundsData, "Death", _audioSource);
		}
		else
		{
			_deathCamera.transform.position = _deathCameraOffsets[0].position;
			_deathCamera.transform.rotation = _deathCameraOffsets[0].rotation;
			_deathTimer.text = "";
		}
	}

	IEnumerator KoAnimation(float duration)
	{
		float elapsedTime = 0;

		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;

			_deathTimer.text = (duration - elapsedTime).ToString("F2");
			_deathCamera.transform.position = Vector3.Lerp(_deathCamera.transform.position, _deathCameraOffsets[1].position, elapsedTime / duration);
			_deathCamera.transform.rotation = Quaternion.Lerp(_deathCamera.transform.rotation, _deathCameraOffsets[1].rotation, elapsedTime / duration);

			yield return null;
		}

		_deathTimer.text = "0.00";
		_koCoroutine = null;
	}

	private void StartWarning()
	{
		_elapsedTimeShowWarning = 1f;
	}
	
	private void TakeDamageEffect()
	{
		SoundManager.PlaySound(_soundsData, "Hurt", _audioSource);
		
		StartCoroutine(ApplyVolumeDamagedEffect());
	}
	
	IEnumerator ApplyVolumeDamagedEffect()
	{
		float time = 0.5f;
		float elapsedTime = 0f;

		while (elapsedTime < time)
		{
			elapsedTime += Time.deltaTime;

			float t = elapsedTime / time;
			_damagedImage.alpha = Mathf.Sin(t * Mathf.PI);

			yield return null;
		}

		_damagedImage.alpha = 0f;
	}

	private void Update()
	{
		if (_healingTrajectoryLine.enabled && healLineComplete)
		{
			Vector3[] line = _playerHealth.HealThrowLine(out float distance);
			_healingTrajectoryLine.positionCount = line.Length;
			_healingTrajectoryLine.SetPositions(line);
			ShowHealSphereEffect(line[^1], distance * _playerHealth.healSizeEffectFactor + _playerHealth.minSize);
		}
		_healthBar.fillAmount = Mathf.Lerp(_healthBar.fillAmount, _healTargetFillAmount, Time.deltaTime * _healthVisualFillingSpeed);
	}

	Coroutine showLineCoroutine;
	private void OnThrowingVisualActivation()
	{
		if (gunSwitching) HideGun();
		showLineCoroutine = StartCoroutine(LinePreviewDelay());
	}

	IEnumerator LinePreviewDelay()
	{
		healLineComplete = false;
		yield return new WaitForSeconds(_playerHealth.showLineDelay);
		int progression = 0;
		_healingTrajectoryLine.enabled = true;
		while (progression < _playerHealth.HealThrowLine(out float distance).Length)
		{
			Vector3[] line = _playerHealth.HealThrowLine(out distance);
			_healingTrajectoryLine.positionCount = progression;
			for (int i = 0; i < progression; i++)
			{
				_healingTrajectoryLine.SetPosition(i, line[i]);
			}
			progression++;
			yield return null;
		}
		healLineComplete = true;
	}
	private void StopPreview()
	{
		if (showLineCoroutine != null) StopCoroutine(showLineCoroutine);
		_healingTrajectoryLine.enabled = false;
		if (gunSwitching) ShowGun();
	}
	private async void ShowHealSphereEffect(Vector3 pos, float scale, float time = 0f)
	{
		_healingThrowPosObj.SetActive(true);
		_healingThrowPosObj.transform.position = pos;
		_healingThrowPosObj.transform.localScale = Vector3.one * (scale * _playerHealth.healSizeEffectFactor);
		if(time == 0f) await Task.Delay((int)(Time.deltaTime * 1000));
		else await Task.Delay((int)(time * 1000));
		if(_healingTrajectoryLine.enabled == false) _healingThrowPosObj.SetActive(false);
	}

	private void OnHeal()
	{
		SoundManager.PlaySound(_soundsData, "Heal", _audioSource);
	}

	void HideGun()
	{
		gunSwitching.DesactivateAllMainGun();
	}

	void ShowGun()
	{
		gunSwitching.ActivateCurrentGun(gunSwitching._mainGunsList, gunSwitching.CurrentMainGunIndex);
	}

	void OnEnable()
	{
		_playerHealth.OnUpdateHealth += UpdateHealth;
		_playerHealth.OnStartWarning += StartWarning;
		_playerHealth.OnKOPlayer += KoPlayerUI;
		_playerHealth.OnTakeDamage += TakeDamageEffect;
		
		//Healing
		_playerHealth.OnThrowingVisualActivation += OnThrowingVisualActivation;
		_playerHealth.OnThrowKeyReleased += StopPreview;
		_playerHealth.OnHealThrowLanding += ShowHealSphereEffect;
		_playerHealth.OnHealCanceled += StopPreview;
		_playerHealth.OnHeal += OnHeal;
		
		//Drone Throw
		_droneThrower.OnThrowing += StopPreview;
	}

	void OnDisable()
	{
		_playerHealth.OnUpdateHealth -= UpdateHealth;
		_playerHealth.OnStartWarning -= StartWarning;
		_playerHealth.OnKOPlayer -= KoPlayerUI;
		_playerHealth.OnTakeDamage -= TakeDamageEffect;
		_playerHealth.OnHeal -= OnHeal;
    
		//Healing
		_playerHealth.OnThrowingVisualActivation -= OnThrowingVisualActivation;
		_playerHealth.OnThrowKeyReleased -= StopPreview;
		_playerHealth.OnHealThrowLanding -= ShowHealSphereEffect;
		_playerHealth.OnHealCanceled -= StopPreview;

		//Drone Throw
		_droneThrower.OnThrowing -= StopPreview;
	}


	#endregion
}
