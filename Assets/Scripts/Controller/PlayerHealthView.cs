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
	[SerializeField] private CanvasGroup _damagedWarningImage;
	[SerializeField] private Image _frameDeccordImage;
	[SerializeField] private CanvasGroup _damagedImage;
	[SerializeField] private Image _cooldownHealImage;
	
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
		ShowWarning();
	}
	
	void ShowWarning()
	{
		bool critik = _playerHealth.IsCritik;
		
		_frameDeccordImage.color = critik ? Color.red : Color.white;
		_damagedWarningImage.gameObject.SetActive(critik);

		if (critik)
		{
			_elapsedTimeShowWarning -= Time.deltaTime;

			if (_elapsedTimeShowWarning <= 0)
			{
				_isShowedWarning = !_isShowedWarning;
				_elapsedTimeShowWarning = 1f;
			}
			
			_damagedWarningImage.alpha = Mathf.Sin(_elapsedTimeShowWarning * Mathf.PI);
		}
		else
		{
			_damagedWarningImage.alpha = 0f;
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
		
		_frameDeccordImage.color = Color.white;
	}

	private void StartWarning()
	{
		_elapsedTimeShowWarning = 1f;
	}
	
	private void TakeDamageEffect()
	{
		AudioClip clip = SoundManager.GetAudioClip(_soundsData, "Hurt");
		SoundManager.PlaySound(clip, _audioSource);
		
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
		if (_healingTrajectoryLine.enabled)
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
		yield return new WaitForSeconds(_playerHealth.showLineDelay);
		_healingTrajectoryLine.enabled = true;
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

	void HideGun()
	{
		currentGunID = gunSwitching.CurrentMainGunIndex;
		gunSwitching.DesactivateAllMainGun();
	}

	void ShowGun()
	{
		gunSwitching.ActivateCurrentGun(gunSwitching._mainGunsList, currentGunID);
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
		
		//Drone Throw
		//_droneThrower.OnThrowingActivation += OnThrowingVisualActivation;
		_droneThrower.OnThrowing += StopPreview;
	}

	void OnDisable()
	{
		_playerHealth.OnUpdateHealth -= UpdateHealth;
		_playerHealth.OnStartWarning -= StartWarning;
		_playerHealth.OnKOPlayer -= KoPlayerUI;
		_playerHealth.OnTakeDamage -= TakeDamageEffect;
		
		//Healing
		_playerHealth.OnThrowingVisualActivation -= OnThrowingVisualActivation;
		_playerHealth.OnThrowing -= StopPreview;
		_playerHealth.OnHealThrowLanding -= ShowHealSphereEffect;
		
		//Drone Throw
		//_droneThrower.OnThrowingActivation -= OnThrowingVisualActivation;
		_droneThrower.OnThrowing -= StopPreview;
	}


	#endregion
}
