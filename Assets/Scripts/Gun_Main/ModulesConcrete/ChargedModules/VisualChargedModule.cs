using GunDecorator;
using GunDecorator.ChargedModules;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VisualChargedModule : GunModule
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private ChargedParentModule _chargedModule;

	[Header("UI")]
	[SerializeField] private TextMeshPro _percentageChargeText;
	[SerializeField] private MeshRenderer _textMat;
	[SerializeField] private Color _normalTextColor;
	[SerializeField] private Color _fullChargedTextColor;
	
	#endregion


	#region Fonctions

	void OnEnable()
	{
		_chargedModule.OnPercentageChargeChange += PercentageChanged;
		_chargedModule.OnFullCharged += FullCharged;
	}

	void OnDisable()
	{
		_chargedModule.OnPercentageChargeChange -= PercentageChanged;
	}
	private void PercentageChanged(int percent)
	{
		_percentageChargeText.text = percent + "%";
	}
	
	private void FullCharged(bool isFull)
	{
		_textMat.material.SetColor("_FresnelColor" , isFull ? _fullChargedTextColor : _normalTextColor);

		if (isFull)
		{
			_gunController.PlaySound("IsCharged");
		}
	}

	#endregion
}
