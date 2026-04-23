using System;
using System.Collections;
using System.Threading.Tasks;
using CustomConsole.Runtime.Logger;
using ScriptableObjectsDefinitions;
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
	[SerializeField] private Image _deathImage;
	[SerializeField] private CanvasGroup _damagedWarningImage;
	[SerializeField] private Image _frameDeccordImage;
	[SerializeField] private CanvasGroup _damagedImage;
	[SerializeField] private Image _cooldownHealImage;
	
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
		_deathImage.gameObject.SetActive(false);
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

	private void KoPlayerUI(bool state)
	{
		_deathImage.gameObject.SetActive(state);
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
		_droneThrower.OnThrowingActivation += OnThrowingVisualActivation;
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
		_droneThrower.OnThrowingActivation -= OnThrowingVisualActivation;
		_droneThrower.OnThrowing -= OnThrowing;
	}

	#endregion
}
