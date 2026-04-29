using System;
using UnityEngine;

public class PlayerSelectGun : NetworkBusListener
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private GunSwitching _gun;
	[SerializeField] private FPSController _fps;
	
	[Header("View")]
	[SerializeField] private GameObject _uiInput;
	[SerializeField] private GameObject _uiSelect;

	private int _newIndexGun = 0;
	
	#endregion


	#region Fonctions

	public override void OnStartClient()
	{
		ListenToEvent<OnAllPlayerAtBorne>(OnShowUI);
		ListenToEvent<OnAllPlayerCanSelectGun>(CanSelectGun);
	}

	private void OnShowUI(OnAllPlayerAtBorne data)
	{
		_uiSelect.SetActive(true);
		_uiInput.SetActive(false);
		
		_gun.DesactivateAllMainGun();
		_fps.IsFreeze = true;
		
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		_newIndexGun = _gun.CurrentMainGunIndex;
	}

	private void CanSelectGun(OnAllPlayerCanSelectGun obj)
	{
		_uiInput.SetActive(true);
	}
	
	public void ChangeGun(int id)
	{
		_newIndexGun = id;
	}

	public void FinishSelection()
	{
		_uiSelect.SetActive(false);
		_gun.ChangeCurrentGun_Main_ServerRpc(_newIndexGun);
		_fps.IsFreeze = false;
		
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
	
	#endregion
}
