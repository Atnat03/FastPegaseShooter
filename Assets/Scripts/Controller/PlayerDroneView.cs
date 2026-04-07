using System;
using MyPrint;
using UnityEngine;
using UnityEngine.UI;

public struct DroneUpdateActivation
{
	public float p_ratio;
}

public class PlayerDroneView : NetworkBusListener
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private DroneThrower _droneThrower;

	[Header("UI")] [SerializeField] private GameObject _uiActivated;
	[SerializeField] private Image _imageActivated;
	[SerializeField] private Image _imageCooldown;

	[Header("Under Effect")] 
	[SerializeField] private GameObject _underDroneEffect;
	[SerializeField] private Image _baseFrame;
	[SerializeField] private Color _colorFrame;

	#endregion

	#region Fonctions

	public void OnEnable()
	{
		ListenToEvent<DroneActivatedEvent>(ActivatedDrone);
		_droneThrower.OnCooldownUpdate += UpdateCooldown;
	}

	private void UpdateCooldown(float ratio)
	{
		_imageCooldown.fillAmount = 1 - ratio;
	}

	private void ActivatedDrone(DroneActivatedEvent data)
	{
		if (data.p_playerId != LocalConnection.ClientId)
			return;

		_uiActivated.SetActive(data.p_isActivate);

		_imageActivated.fillAmount = 1 - data.p_ratioBar;
	}

	public void SetInfoUnderDrone(bool state)
	{
		_underDroneEffect.SetActive(state);
		_baseFrame.gameObject.SetActive(state);
		
		_baseFrame.color = state ? _colorFrame : Color.white;
	}


#endregion
}
