using System;
using System.Collections;
using System.Collections.Generic;
using GunDecorator;
using MyPrint;
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
	
	[Header("Noise")]
	[SerializeField] private List<ReticuleUI> _reticules;
	
	#endregion
	
	#region Fonctions

	private void OnEnable()
	{
		_gunController.OnShootAmmo += OnAmmoChange;
		_gunController.OnShootNoise += OnNoiseChange;

		_gunController.OnCharging += OnNoiseChange;
		_gunController.OnStopCharging += StopCharging;
	}
	
	private void OnDisable()
	{
		_gunController.OnShootAmmo -= OnAmmoChange;
		_gunController.OnShootNoise -= OnNoiseChange;
		
		_gunController.OnCharging -= OnNoiseChange;
		_gunController.OnStopCharging -= StopCharging;
	}

	private void OnNoiseChange(float ratio)
	{
		foreach (ReticuleUI r in _reticules)
		{
			r.image.transform.localPosition = Vector3.Lerp(r.minPos, r.maxPos, ratio);
			r.image.transform.localScale = Vector3.Lerp(r.minScale, r.maxScale, ratio);
		}
	}

	private void OnAmmoChange(int amount, int maxAmmo)
	{
		_ammoText.text = amount + " / " +  maxAmmo;
	}
	
	private void StopCharging()
	{
		foreach (ReticuleUI r in _reticules)
		{
			r.image.transform.localScale = r.minScale;
			r.image.transform.localPosition = r.minPos;
		}
	}
	
	#endregion
}

[Serializable]
public class ReticuleUI
{
	public Image image;
	public Vector2 minPos;
	public Vector2 maxPos;
	public Vector3 minScale = Vector3.one;
	public Vector3 maxScale = Vector3.one;
}
