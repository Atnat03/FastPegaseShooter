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
	
	[Header("Silouette")]
	[SerializeField] private Image _imageSilouette;
	[SerializeField] private Sprite _spriteSilouette;
	
	#endregion
	
	#region Fonctions
	
	private void OnEnable()
	{
		_gunController.OnShootAmmo += OnAmmoChange;
		_gunController.OnShootNoise += OnNoiseChange;

		_gunController.OnCharging += OnNoiseChange;
		_gunController.OnStopCharging += StopCharging;
		
		_gunController.OnSetUp += OnSetUp;
	}

	private void OnDisable()
	{
		_gunController.OnShootAmmo -= OnAmmoChange;
		_gunController.OnShootNoise -= OnNoiseChange;
		
		_gunController.OnCharging -= OnNoiseChange;
		_gunController.OnStopCharging -= StopCharging;
		
		_gunController.OnSetUp -= OnSetUp;
	}

	private void OnNoiseChange(float ratio)
	{
		if (_reticules.Count <= 0)
			return;
		
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
		if (_reticules.Count <= 0)
			return;
		
		foreach (ReticuleUI r in _reticules)
		{
			r.image.transform.localScale = r.minScale;
			r.image.transform.localPosition = r.minPos;
		}
	}
	
	private void OnSetUp()
	{
		if(_imageSilouette && _spriteSilouette)
			_imageSilouette.sprite = _spriteSilouette;
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
