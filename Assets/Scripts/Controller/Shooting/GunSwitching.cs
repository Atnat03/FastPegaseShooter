using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using GunDecorator;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class GunSwitching : NetworkBehaviour
{
	#region Properties

	public bool IsMainGun => _isMainGun;
	public GameObject CurrentMainGun => _mainGunsList[_currentMainGun.Value];
	public GameObject CurrentSecondaryGun => _secondaryGunsList[_currentSecondaryGun.Value];
	public int CurrentMainGunIndex => _currentMainGun.Value;
	

	#endregion
	
	#region Variables

	[SerializeField] private bool _isMainGun = true;
	
	[Header("References")]
	[SerializeField] private GameObject _mainGunParent;
	[SerializeField] private GameObject _secondaryGunParent;

	private bool _canSwitch = true;
	private List<GameObject> _mainGunsList;
	private List<GameObject> _secondaryGunsList;
	
	private readonly SyncVar<int> _currentMainGun = new SyncVar<int>(0);
	private readonly SyncVar<int> _currentSecondaryGun = new SyncVar<int>(0);
	
	#endregion

	#region Fonctions

	public void Initialize(int startIndex)
	{

		_currentMainGun.OnChange += OnCurrentGunMainChange;
		_currentSecondaryGun.OnChange += OnCurrentGunSecondaireChange;
		
		
		_mainGunsList = new List<GameObject>();
		_secondaryGunsList = new List<GameObject>();

		foreach (Transform gun in _mainGunParent.transform)
		{
			_mainGunsList.Add(gun.gameObject);
		}

		foreach (Transform gun in _secondaryGunParent.transform)
		{
			_secondaryGunsList.Add(gun.gameObject);
		}
		
		_currentMainGun.Value = startIndex;
		
		UpdateVisual();
	}
	
	public void SwitchGunType()
	{
		if (!_canSwitch) return;
		
		_isMainGun = !_isMainGun;
		UpdateVisual();
	}

	private void UpdateVisual()
	{
		if (_isMainGun)
		{
			SwitchAnimatedWeapon(_mainGunsList, _currentMainGun.Value);
		}
		else
		{
			SwitchAnimatedWeapon(_secondaryGunsList, _currentSecondaryGun.Value);
		}
	}
	
	void SwitchAnimatedWeapon(List<GameObject> list, int index)
	{
		_mainGunParent.SetActive(false);
		_secondaryGunParent.SetActive(false);
		
		_canSwitch = false;
		
		_mainGunParent.SetActive(_isMainGun);
		_secondaryGunParent.SetActive(!_isMainGun);

		_canSwitch = true;
		
		ActivateCurrentGun(list, index);
	}
	
	private void ActivateCurrentGun(List<GameObject> list, int index)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if(i == index)
				list[i].gameObject.SetActive(true);
			else
				list[i].gameObject.SetActive(false);
		}
	}

	public void DesactivateAllMainGun()
	{
		foreach (GameObject t in _mainGunsList)
		{
			t.gameObject.SetActive(false);
		}
	}
	
	[ServerRpc]
	public void ChangeCurrentGun_Main_ServerRpc(int newIndex)
	{
		ChangeCurrentGun_Main(newIndex);
	}

	[Server]
	void ChangeCurrentGun_Main(int newIndex)
	{
		_currentMainGun.Value = newIndex;
	}
		
	private void OnCurrentGunMainChange(int prev, int next, bool asServer)
	{
		if (_mainGunsList == null || _mainGunsList.Count == 0) return;
       
		if (next < _mainGunsList.Count)
		{
			if (IsOwner)
			{
				CurrentMainGun.GetComponent<GunController>().p_authorizedToShoot = true;
				CurrentMainGun.GetComponent<GunController>().StopReload();
			}
          
			if (_isMainGun)
			{
				ActivateCurrentGun(_mainGunsList, next);
			}
		}
	}

	
	private void OnCurrentGunSecondaireChange(int prev, int next, bool asServer)
	{
		if (_secondaryGunsList == null || _secondaryGunsList.Count == 0) return;
       
		if (next < _secondaryGunsList.Count && !_isMainGun)
		{
			ActivateCurrentGun(_secondaryGunsList, next);
		}
	}
	
	public void ChangeCurrentGun_Secondary(int newIndex) => _currentSecondaryGun.Value = newIndex;
	
	#endregion
}
