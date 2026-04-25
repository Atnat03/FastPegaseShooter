using System;
using System.Collections;
using System.Threading.Tasks;
using CustomConsole.Runtime.Logger;
using MyPrint;
using ScriptableObjectsDefinitions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthView : MonoBehaviour
{
	#region Properties

	#endregion


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
			Vector3 startPos = _playerHealth.p_healThrowPoint.position;
			Vector3 endPos = _playerHealth.p_healThrowLandingPos;
			Vector3 dir = (endPos-startPos).normalized;
			_healingTrajectoryLine.SetPosition(0, startPos);
			_healingTrajectoryLine.SetPosition(1, startPos+dir*0.3f);
			_healingTrajectoryLine.SetPosition(2, endPos-dir*0.3f);
			_healingTrajectoryLine.SetPosition(3, endPos);
		}
		_healthBar.fillAmount = Mathf.Lerp(_healthBar.fillAmount, _healTargetFillAmount, Time.deltaTime * _healthVisualFillingSpeed);
	}

	private void OnThrowingVisualActivation()
	{
		_healingTrajectoryLine.enabled = true;
	}
	private void OnThrowing()
	{
		_healingTrajectoryLine.enabled = false;
	}
	private async void OnHealThrowLanding(Vector3 pos)
	{
		GameObject orb = Instantiate(_healingThrowPosObj, pos, Quaternion.identity);
		orb.transform.localScale = Vector3.one * _playerHealth.p_healThrowRadius;
		await Task.Delay(1000);
		Destroy(orb);
	}

	void OnEnable()
	{
		_playerHealth.OnUpdateHealth += UpdateHealth;
		_playerHealth.OnStartWarning += StartWarning;
		_playerHealth.OnKOPlayer += KoPlayerUI;
		_playerHealth.OnTakeDamage += TakeDamageEffect;
		
		//Healing
		_playerHealth.OnThrowingVisualActivation += OnThrowingVisualActivation;
		_playerHealth.OnThrowing += OnThrowing;
		_playerHealth.OnHealThrowLanding += OnHealThrowLanding;
		
		//Drone Throw
		//_droneThrower.OnThrowingActivation += OnThrowingVisualActivation;
		_droneThrower.OnThrowing += OnThrowing;
	}

	void OnDisable()
	{
		_playerHealth.OnUpdateHealth -= UpdateHealth;
		_playerHealth.OnStartWarning -= StartWarning;
		_playerHealth.OnKOPlayer -= KoPlayerUI;
		_playerHealth.OnTakeDamage -= TakeDamageEffect;
		
		//Healing
		_playerHealth.OnThrowingVisualActivation -= OnThrowingVisualActivation;
		_playerHealth.OnThrowing -= OnThrowing;
		_playerHealth.OnHealThrowLanding -= OnHealThrowLanding;
		
		//Drone Throw
		//_droneThrower.OnThrowingActivation -= OnThrowingVisualActivation;
		_droneThrower.OnThrowing -= OnThrowing;
	}

	#endregion
}
