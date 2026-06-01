using System;
using FishNet.Object;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SwapGunManagerView : NetworkBehaviour
{
	#region Properties

	#endregion


	#region Variables
	
	[Header("References")]
	[SerializeField] private PlayerZoneManager _manager;

	[Header("UI")]
	[SerializeField] private GameObject _barUI;
	[SerializeField] private Image _valueImage;
	[SerializeField] private TextMeshProUGUI _textSwapUI;
	[SerializeField] private string _youAskSwapMessage;
	[SerializeField] private string _broAskyouSwapMessage;
		
	#endregion


	#region Fonctions
	

	private void ChangeAskText(bool isRequester)
	{
		_textSwapUI.text = isRequester ? _youAskSwapMessage : _broAskyouSwapMessage;
	}

	private void OnElapsedTimeChanged(float prev, float next, bool asServer)
	{
		_barUI.SetActive(next > 0);
	}

	private void UpdateUI(float ratio)
	{
		_valueImage.fillAmount = ratio;
	}
	
	#endregion
}
