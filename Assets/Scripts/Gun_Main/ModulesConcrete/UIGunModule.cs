using System;
using System.Collections;
using System.Collections.Generic;
using GunDecorator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGunModule : GunModule
{
	#region Properties

	#endregion


	#region Variables
	
	[Header("Reload")]
	[SerializeField, Tooltip("Text des balles actuelles + max balles")] private TextMeshProUGUI _ammoText;
	[SerializeField, Tooltip("Cercle pour le temps de reload")] private Image _imageReload;
	
	[Header("Noise")]
	[SerializeField] private List<ReticuleUI> _reticules;
	
	#endregion
	
	#region Fonctions

	private void OnEnable()
	{
		_gunController.OnShootAmmo += OnAmmoChange;
		_gunController.OnShootNoise += OnNoiseChange;

		_gunController.OnEndReload += EndReload;
		_gunController.OnStartReload += StartReload;
	}
	
	private void OnDisable()
	{
		_gunController.OnShootAmmo -= OnAmmoChange;
		_gunController.OnShootNoise -= OnNoiseChange;

		_gunController.OnEndReload -= EndReload;
		_gunController.OnStartReload -= StartReload;
	}

	private void StartReload(float reloadDuration)
	{
		_imageReload.gameObject.SetActive(true);
		_imageReload.fillAmount = 1;

		StartCoroutine(ReloadUI(reloadDuration));
	}

	IEnumerator ReloadUI(float reloadDuration)
	{
		float duration = reloadDuration;
		float elapsedTime = duration;

		while (elapsedTime > 0)
		{
			elapsedTime -= Time.deltaTime;
			_imageReload.fillAmount = elapsedTime / duration;
			yield return null;
		}
	}
	
	private void EndReload()
	{
		_imageReload.gameObject.SetActive(false);
	}

	private void OnNoiseChange(float ratio)
	{
		foreach (ReticuleUI r in _reticules)
		{
			r.image.transform.localPosition = Vector3.Lerp(r.minPos, r.maxPos, ratio);
		}
	}

	private void OnAmmoChange(int amount, int maxAmmo)
	{
		_ammoText.text = amount + " / " +  maxAmmo;
	}

	#endregion
}

[Serializable]
public class ReticuleUI
{
	public Image image;
	public Vector2 minPos;
	public Vector2 maxPos;
}
