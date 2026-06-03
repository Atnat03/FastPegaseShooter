using System;
using GunDecorator;
using GunDecorator.ChargedModules;
using MyPrint;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VisualChargedModule : GunModule
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private ChargedParentModule _chargedModule;
	
	[Header("VFX")]
	[SerializeField] private ParticleSystem _chargedParticleSystem;
	
	#endregion


	#region Fonctions

	private void OnEnable()
	{
		_chargedModule.OnStartCharged += Charging;
	}

	private void OnDisable()
	{
		_chargedModule.OnStartCharged -= Charging;
	}

	private void Charging()
	{
		_chargedParticleSystem.Play();
	}

	#endregion
}
