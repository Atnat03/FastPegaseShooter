using System;
using MyPrint;
using UnityEngine;
using UnityEngine.UI;

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
		Debug.Log("SetInfoUnderDrone : " + state);
		
		_underDroneEffect.SetActive(state);

		if (state)
		{
			Debug.Log("_colorFrame : ");
			_baseFrame.color = _colorFrame;
		}
		else
		{
			_baseFrame.color = Color.white;
		}
	}


#endregion
}
