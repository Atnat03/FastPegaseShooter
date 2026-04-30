using System;
using MyPrint;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
	[SerializeField] private Image[] _imageOutlineList;

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
		if (!IsOwner) return;
	
		Cons.Print("Show UI", ColorConsole.Blue);
		
		_gun.DesactivateAllMainGun();
		_fps.IsFreeze = true;
		
		CursorManager.instance.PushState(CursorState.UI, _fps);

		_newIndexGun = _gun.CurrentMainGunIndex;
		
		UpdateUI();
		
		_uiSelect.SetActive(true);
		_uiInput.SetActive(false);
	}

	private void CanSelectGun(OnAllPlayerCanSelectGun data)
	{
		if (!IsOwner) return;
		
		_uiInput.SetActive(data.p_open);
	}
	
	public void ChangeGun(int id)
	{
		if (!IsOwner) return;
		
		_newIndexGun = id;
		UpdateUI();
	}

	public void FinishSelection()
	{
		if (!IsOwner) return;
		
		_uiSelect.SetActive(false);
		_gun.ChangeCurrentGun_Main_ServerRpc(_newIndexGun);
		_fps.IsFreeze = false;
		
		CursorManager.instance.PopState(_fps);
	}

	private void UpdateUI()
	{
		for (int i = 0; i < _imageOutlineList.Length; i++)
		{
			if (i == _newIndexGun)
			{
				_imageOutlineList[i].gameObject.SetActive(true);
			}
			else
			{
				_imageOutlineList[i].gameObject.SetActive(false);
			}
		}
	}
	
	#endregion
}
