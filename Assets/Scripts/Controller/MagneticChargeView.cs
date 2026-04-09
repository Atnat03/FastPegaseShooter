using System;
using UnityEngine;
using UnityEngine.UI;

public class MagneticChargeView : MonoBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private GunSwitching _gunSwitching;
	
	[Header("UI")]
	[SerializeField] private Image _imageCurrentCharge;
	[SerializeField] private Sprite _positiveSprite;
	[SerializeField] private Sprite _negativeSprite;

	#endregion


	#region Fonctions

	private void OnEnable()
	{
		_gunSwitching.OnSwapGun += UpdateUI;
	}

	private void UpdateUI(bool positive)
	{
		_imageCurrentCharge.sprite = positive ? _positiveSprite : _negativeSprite;
	}

	#endregion
}
