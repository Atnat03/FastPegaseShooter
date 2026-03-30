using Controller;
using UnityEngine;
using UnityEngine.UI;

public class GunSwapView : MonoBehaviour
{
	#region Properties

	#endregion


	#region Variables
	
	[Header("References")]
	[SerializeField] private GunSurcharge _gunSurcharge;

	[Header("Effects")]
	[SerializeField] private Image _infoOverload;
	
	#endregion


	#region Fonctions

	private void SetColor(Color color)
	{
		_infoOverload.color = color;
	}

	private void UpdateUI(bool isOverload, float ratio)
	{
		_infoOverload.gameObject.SetActive(isOverload);
		_infoOverload.fillAmount = ratio;
	}
	
	void OnEnable()
	{
		_gunSurcharge.OnOverloadTimeUpdate += UpdateUI;
		_gunSurcharge.OnInfoOverloadSetColor += SetColor;
	}
	
	void OnDisable()
	{
		_gunSurcharge.OnOverloadTimeUpdate -= UpdateUI;
		_gunSurcharge.OnInfoOverloadSetColor -= SetColor;
	}

	#endregion
}
