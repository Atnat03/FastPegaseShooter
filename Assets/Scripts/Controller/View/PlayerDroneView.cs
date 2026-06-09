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

	[Header("Under Effect")] 
	[SerializeField] private GameObject _underDroneEffect;

	#endregion

	#region Fonctions
	
	public void SetInfoUnderDrone(bool state)
	{
		Cons.Print("SetInfoUnderDrone ", ColorConsole.Orange);

		_underDroneEffect.SetActive(state);
	}


#endregion
}
