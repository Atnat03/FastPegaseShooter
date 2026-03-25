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

	public bool IsMainGun => _isMainGun.Value;
	public GameObject CurrentMainGun => _mainGunsList[_currentMainGun.Value];
	public GameObject CurrentSecondaryGun => _secondaryGunsList[_currentSecondaryGun.Value];
	public int CurrentMainGunIndex => _currentMainGun.Value;

	public bool IsSwitching => !_canSwitch;
	
	#endregion
	
	#region Variables

	private readonly SyncVar<bool> _isMainGun = new SyncVar<bool>(true);
	
	[Header("References")]
	[SerializeField] private GameObject _mainGunParent;
	[SerializeField] private GameObject _secondaryGunParent;

	private bool _canSwitch = true;
	private List<GameObject> _mainGunsList;
	private List<GameObject> _secondaryGunsList;
	
	private readonly SyncVar<int> _currentMainGun = new SyncVar<int>(0);
	private readonly SyncVar<int> _currentSecondaryGun = new SyncVar<int>(0);

	public Action OnStartSwitchGun;
	public Action OnEndSwitchGun;
	
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

		UpdateVisual(true);
		
		_currentMainGun.Value = startIndex;
		_currentSecondaryGun.Value = startIndex;
	}
	
	[ServerRpc]
	public void SwitchGunType(bool state)
	{
		if (!_canSwitch) return;
		if(IsMainGun == state) return;

		SwitchGunServer(state);
	}

	[Server]
	void SwitchGunServer(bool state)
	{
		_isMainGun.Value = state;
		SwitchGunAnimation_ObserversRpc(state);
	}
	
	[ObserversRpc]
	private void SwitchGunAnimation_ObserversRpc(bool isMain)
	{
		UpdateVisual(isMain);
	}

	private void UpdateVisual(bool main)
	{
		if (main)
		{
			StartCoroutine(SwitchAnimatedWeapon(_mainGunsList, _currentMainGun.Value, true));
		}
		else
		{
			StartCoroutine(SwitchAnimatedWeapon(_secondaryGunsList, _currentSecondaryGun.Value, false));
		}
	}
	
	IEnumerator SwitchAnimatedWeapon(List<GameObject> list, int index, bool isMain)
	{
		_canSwitch = false;
		
		OnStartSwitchGun?.Invoke();

		float duration = 0.25f;
		float elapsedTime = 0;
	
		Quaternion startRotation = transform.localRotation;
		Quaternion targetRot = startRotation * Quaternion.Euler(-90, 0, 0);
	
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			transform.localRotation = Quaternion.Lerp(startRotation, targetRot, elapsedTime / duration);
			yield return null;
		}

		if(IsServerInitialized)
			ActivateCurrentGun(list, index);
	
		_mainGunParent.SetActive(isMain);
		_secondaryGunParent.SetActive(!isMain);
	
		elapsedTime = 0;
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			transform.localRotation = Quaternion.Lerp(targetRot, startRotation, elapsedTime / duration);
			yield return null;
		}
	
		transform.localRotation = startRotation;

		_canSwitch = true;
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
		if (!IsMainGun) return;
		
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
          
			if (!asServer)
			{
				ActivateCurrentGun(_mainGunsList, next);
			}
		}
	}

	
	private void OnCurrentGunSecondaireChange(int prev, int next, bool asServer)
	{
		if (_secondaryGunsList == null || _secondaryGunsList.Count == 0) return;
       
		if (next < _secondaryGunsList.Count && !IsMainGun)
		{
			ActivateCurrentGun(_secondaryGunsList, next);
		}
	}
	
	public void ChangeCurrentGun_Secondary(int newIndex) => _currentSecondaryGun.Value = newIndex;

	
	#endregion
}
