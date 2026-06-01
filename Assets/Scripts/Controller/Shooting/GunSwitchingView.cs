using System;
using System.Collections;
using MyPrint;
using TMPro;
using UnityEngine;

public class GunSwitchingView : MonoBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private GunSwitching _gunSwitching;
	
	[Header("UI")]
	[SerializeField] private TextMeshProUGUI _message;
	[SerializeField] private string[] message;

	private void OnMainGunChange(bool isMainGunActivated)
	{
		_message.text = isMainGunActivated ? message[0] : message[1];
	}

	#endregion 
}
