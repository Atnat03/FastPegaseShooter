using System;
using System.Collections;
using MyPrint;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MagneticChargeView : MonoBusListener
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private GunSwitching _gunSwitching;
	
	[Header("UI")]
	[SerializeField] private Image _positiveUI;
	[SerializeField] private Image _negativeUI;
	[SerializeField] private Image _cooldown;
	
	[Header("Polarization")]
	[SerializeField] private TextMeshProUGUI _polarizationText;
	[SerializeField] private string _isPolarizedMessage;
	[SerializeField] private string _isAlignedMessage;
	
	[Header("Conflict")]
	[SerializeField] private GameObject _conflictPanel;
	[SerializeField] private TextMeshProUGUI _conflictText;
	[SerializeField] private Image _conflictTimerBar;

	#endregion


	#region Fonctions

	private void OnEnable()
	{
		_gunSwitching.OnSwapGun += UpdateUI;
		
		ListenToEvent<OnPolarizationStateChanged>(UpdatePolarisation);
		ListenToEvent<OnConflictUIUpdate>(UpdateConflictText);
		ListenToEvent<OnConflictTimerUIUpdate>(UpdateTimerBar);
	}
	
	private void OnDisable()
	{
		_gunSwitching.OnSwapGun -= UpdateUI;
	}

	private void UpdatePolarisation(OnPolarizationStateChanged data)
	{
		_polarizationText.text = data.isAligned ? _isPolarizedMessage : _isAlignedMessage;
	}

	private void UpdateUI(bool positive)
	{
		_positiveUI.gameObject.SetActive(positive);
		_negativeUI.gameObject.SetActive(!positive);
	}
	
	private void UpdateConflictText(OnConflictUIUpdate data)
	{
		_conflictPanel.SetActive(data.isConflict || data.isShortCircuit);

		if (data.isShortCircuit)
			_conflictText.text = "COURT CIRCUIT";
		else if (data.isConflict)
			_conflictText.text = "CONFLIT ÉLECTRIQUE - Temps avant court-circuit";
	}

	private void UpdateTimerBar(OnConflictTimerUIUpdate data)
	{
		_conflictTimerBar.fillAmount = data.ratio;
	}


	#endregion
}
