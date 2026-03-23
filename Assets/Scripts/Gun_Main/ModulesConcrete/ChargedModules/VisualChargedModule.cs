using GunDecorator;
using GunDecorator.ChargedModules;
using UnityEngine;
using UnityEngine.UI;

public class VisualChargedModule : GunModule
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private ChargedParentModule _chargedModule;

	[Header("UI")]
	[SerializeField] private GameObject _chargedUI;
	[SerializeField] private Image _valueCharging;
	[SerializeField] private Color _chargingColor = Color.orange;
	[SerializeField] private Color _fullChargeColor = Color.red;
	
	#endregion


	#region Fonctions

	void Start()
	{
		Debug.Log("Start");
		
		_chargedModule.OnStartCharging += StartCharing;
		_chargedModule.OnEndCharging += EndCharging;
		_chargedModule.OnCharging += Charging;
		_chargedModule.OnFullCharged += FullCharged;
		
		_chargedUI.SetActive(false);
	}

	void OnDisable()
	{
		_chargedModule.OnStartCharging -= StartCharing;
		_chargedModule.OnEndCharging -= EndCharging;
		_chargedModule.OnCharging -= Charging;
		_chargedModule.OnFullCharged -= FullCharged;
	}

	private void Charging(float amount)
	{
		_valueCharging.fillAmount = amount;
	}
	
	private void StartCharing()
	{
		Debug.Log("StartCharging");
		
		_chargedUI.SetActive(true);
		_valueCharging.color = _chargingColor;
	}
	
	private void EndCharging()
	{
		_chargedUI.SetActive(false);
	}

	private void FullCharged()
	{
		_valueCharging.color = _fullChargeColor;
	}

	#endregion
}
