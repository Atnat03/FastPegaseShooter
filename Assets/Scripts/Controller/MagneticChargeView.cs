using System;
using System.Collections;
using MyPrint;
using UnityEngine;
using UnityEngine.UI;

public class MagneticChargeView : MonoBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private GunSwitching _gunSwitching;
	
	[Header("Particles")]
	[SerializeField] private Image _positiveParticles;
	[SerializeField] private Image _negativeParticles;

	#endregion


	#region Fonctions

	private void OnEnable()
	{
		_gunSwitching.OnSwapGun += UpdateUI;
	}

	private void UpdateUI(bool positive)
	{
		_positiveParticles.gameObject.SetActive(positive);
		_negativeParticles.gameObject.SetActive(!positive);
	}

	#endregion
}
