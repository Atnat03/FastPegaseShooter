using System;
using MyPrint;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDroneView : NetworkBusListener
{
	#region Properties

	public DroneThrower DroneThrower => _droneThrower;
	
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
		_droneThrower.OnThrow += ThrowDrone;
		_droneThrower.OnGetDrone += GetDrone;
	}

	private void ThrowDrone() => _imageCooldown.gameObject.SetActive(false);
	private void GetDrone() => _imageCooldown.gameObject.SetActive(true);

	private void ActivatedDrone(DroneActivatedEvent data)
	{
		if (data.p_playerId != LocalConnection.ClientId)
			return;

		_uiActivated.SetActive(data.p_isActivate);

		_imageActivated.fillAmount = 1 - data.p_ratioBar;
	}

	
	public void SetInfoUnderDrone(bool state)
	{
		Cons.Print("SetInfoUnderDrone ", ColorConsole.Orange);

		_underDroneEffect.SetActive(state);
		_baseFrame.color = state ? _colorFrame : Color.white;
	}


#endregion
}
