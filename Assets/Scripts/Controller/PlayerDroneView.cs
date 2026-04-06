using System;
using MyPrint;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDroneView : NetworkBusListener
{
	#region Properties

	#endregion


	#region Variables

	[Header("UI")]
	[SerializeField] private GameObject _uiActivated;
	[SerializeField] private Image _imageActivated;

	#endregion


	#region Fonctions

	public override void OnStartNetwork()
	{
		ListenToEvent<DroneActivatedEvent>(ActivatedDrone);
	}

	private void ActivatedDrone(DroneActivatedEvent data)
	{
		if (data.p_playerId != NetworkObject.ObjectId)
			return;
		
		_uiActivated.SetActive(data.p_isActivate);

		_imageActivated.fillAmount = 1 - data.p_ratioBar;
	}

	#endregion
}
