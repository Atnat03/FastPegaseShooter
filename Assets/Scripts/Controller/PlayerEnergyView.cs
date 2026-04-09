using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEnergyView : MonoBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private PlayerEnergy _playerEnergy;

	[Header("UI")]
	[SerializeField] private Image _energyBar;

	#endregion


	#region Fonctions

	private void OnEnable()
	{
		_playerEnergy.OnAddEnergy += UpdateEnergy;
	}

	private void OnDisable()
	{
		_playerEnergy.OnAddEnergy -= UpdateEnergy;

	}

	private void UpdateEnergy(float ratio)
	{
		_energyBar.fillAmount = ratio;
	}

	#endregion
}
