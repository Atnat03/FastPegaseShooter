using System;
using System.Collections;
using System.Threading.Tasks;
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
	[SerializeField] private Image _healthBar;
	[SerializeField] private Image _deathImage;
	[SerializeField] private CanvasGroup _damagedWarningImage;
	[SerializeField] private Image _frameDeccordImage;
	[SerializeField] private CanvasGroup _damagedImage;
	[SerializeField] private Image _cooldownHealImage;
	
	[Header("Healing")]
	[SerializeField] private Image _selfHealingImage;
	[SerializeField] private Color _selfHealingColor1;
	[SerializeField] private Color _selfHealingColor2;
	[SerializeField] private LineRenderer _healingTrajectoryLine;
	[SerializeField] private GameObject _healingThrowPosObj;
	
	[Header("Sound")]
	[SerializeField] private SoundsDataSO _soundsData;
	private AudioSource _audioSource;
	
	float _elapsedTimeShowWarning = 0;
	bool _isShowedWarning = false;
	
	#endregion


	#region Fonctions

	private void Start()
	{
		_deathImage.gameObject.SetActive(false);
		_audioSource = GetComponent<AudioSource>();
		_healingTrajectoryLine.enabled = false;
	}
	
	private void UpdateHealth(float targetFill)
	{
		_healthBar.fillAmount = Mathf.Lerp(_healthBar.fillAmount, targetFill, Time.deltaTime * 25);
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

	private async void OnSelfHealing(float selfHealingDelay)
	{
		float waitedTime = 0;
		_selfHealingImage.gameObject.SetActive(true);
		while (waitedTime < selfHealingDelay)
		{
			waitedTime += Time.deltaTime;
			await Task.Delay(Mathf.FloorToInt(Time.deltaTime*1000));
			
			_selfHealingImage.fillAmount = waitedTime/selfHealingDelay;
			_selfHealingImage.color = Color.Lerp(_selfHealingColor1, _selfHealingColor2, waitedTime/selfHealingDelay);
		}
		_selfHealingImage.gameObject.SetActive(false);
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
	}

	private void OnThrowingActivation()
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
		await Task.Delay(3000);
		Destroy(orb);
	}
	
	
	private void UpdateCooldownHeal(float ratio)
	{
		_cooldownHealImage.fillAmount = 1 - ratio;
	}

	void OnEnable()
	{
		_playerHealth.OnUpdateHealth += UpdateHealth;
		_playerHealth.OnStartWarning += StartWarning;
		_playerHealth.OnKOPlayer += KoPlayerUI;
		_playerHealth.OnTakeDamage += TakeDamageEffect;
		
		//Healing
		_playerHealth.OnSelfHealing += OnSelfHealing;
		_playerHealth.OnThrowingActivation += OnThrowingActivation;
		_playerHealth.OnThrowing += OnThrowing;
		_playerHealth.OnHealThrowLanding += OnHealThrowLanding;
		_playerHealth.OnUpdateCooldown += UpdateCooldownHeal;
		
		//Drone Throw
		_droneThrower.OnThrowingActivation += OnThrowingActivation;
		_droneThrower.OnThrowing += OnThrowing;
	}

	void OnDisable()
	{
		_playerHealth.OnUpdateHealth -= UpdateHealth;
		_playerHealth.OnStartWarning -= StartWarning;
		_playerHealth.OnKOPlayer -= KoPlayerUI;
		_playerHealth.OnTakeDamage -= TakeDamageEffect;
		
		//Healing
		_playerHealth.OnSelfHealing -= OnSelfHealing;
		_playerHealth.OnThrowingActivation -= OnThrowingActivation;
		_playerHealth.OnThrowing -= OnThrowing;
		_playerHealth.OnHealThrowLanding -= OnHealThrowLanding;
		_playerHealth.OnUpdateCooldown -= UpdateCooldownHeal;
		
		//Drone Throw
		_droneThrower.OnThrowingActivation -= OnThrowingActivation;
		_droneThrower.OnThrowing -= OnThrowing;
	}

	#endregion
}
