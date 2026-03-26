using System;
using System.Collections;
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
	
	[Header("UI")]
	[SerializeField] private Image _healthBar;
	[SerializeField] private Image _deathImage;
	[SerializeField] private CanvasGroup _damagedWarningImage;
	[SerializeField] private Image _frameDeccordImage;
	[SerializeField] private CanvasGroup _damagedImage;
	
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



	void OnEnable()
	{
		_playerHealth.OnUpdateHealth += UpdateHealth;
		_playerHealth.OnStartWarning += StartWarning;
		_playerHealth.OnKOPlayer += KoPlayerUI;
		_playerHealth.OnTakeDamage += TakeDamageEffect;
	}

	void OnDisable()
	{
		_playerHealth.OnUpdateHealth -= UpdateHealth;
		_playerHealth.OnStartWarning -= StartWarning;
		_playerHealth.OnKOPlayer -= KoPlayerUI;
		_playerHealth.OnTakeDamage -= TakeDamageEffect;
	}

	#endregion
}
