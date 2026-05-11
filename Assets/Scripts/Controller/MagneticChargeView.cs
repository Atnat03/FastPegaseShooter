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
	[SerializeField] private ParticleSystem _positiveParticles;
	[SerializeField] private ParticleSystem _negativeParticles;
    /*
	[Header("UI")]
	[SerializeField] private Transform _imagePositive;
	[SerializeField] private Image _imageSelectPositive;
	[SerializeField] private Transform _imageNegative;
	[SerializeField] private Image _imageSelectNegative;
	[SerializeField] private Material _materialLine;*/

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
		
		Cons.PrintBool(positive, "MagneticCharge :");
	}

	#endregion
}
