using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using GunDecorator;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class GunSwitching : MonoBehaviour
{
	#region Properties

	public bool IsMainGun => _isMainGun;
	public GameObject CurrentMainGun => _mainGunsList[_currentMainGun];
	public GameObject CurrentSecondaryGun => _secondaryGunsList[_currentSecondaryGun];

	public int CurrentMainGunIndex => _currentMainGun;
	
	public IGun CurrentGun => IsMainGun ? CurrentMainGun.GetComponent<IGun>() : CurrentSecondaryGun.GetComponent<IGun>();

	#endregion
	
	#region Variables

	[SerializeField] private bool _isMainGun = true;
	
	[Header("References")]
	[SerializeField] private GameObject _mainGunParent;
	[SerializeField] private GameObject _secondaryGunParent;
	[SerializeField] private Transform _visiblePos;
	[SerializeField] private Transform _unvisiblePos;

	private bool _canSwitch = true;
	private List<GameObject> _mainGunsList;
	private List<GameObject> _secondaryGunsList;
	private int _currentMainGun = 0;
	private int _currentSecondaryGun = 0;

	#endregion

	#region Fonctions

	public void Initialize(int startIndex)
	{
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
		
		_currentMainGun = startIndex;
		
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
			StartCoroutine(SwitchAnimatedWeapon(_mainGunsList, _currentMainGun));
		}
		else
		{
			StartCoroutine(SwitchAnimatedWeapon(_secondaryGunsList, _currentSecondaryGun));
		}
	}

	IEnumerator SwitchAnimatedWeapon(List<GameObject> list, int index)
	{
		_mainGunParent.SetActive(true);
		_secondaryGunParent.SetActive(true);
		
		Vector3 mainFinalPos = IsMainGun ? _visiblePos.position : _unvisiblePos.position;
		Vector3 secondaryFinalPos = IsMainGun ? _unvisiblePos.position : _visiblePos.position;
		
		Quaternion mainFinalRot = IsMainGun ? _visiblePos.rotation : _unvisiblePos.rotation;
		Quaternion secondaryFinalRot = IsMainGun ? _unvisiblePos.rotation : _visiblePos.rotation;

		float duration = 0.5f;
		float elapsedTime = 0;

		_canSwitch = false;
		
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			
			_mainGunParent.transform.position = Vector3.Lerp(_mainGunParent.transform.position, mainFinalPos, elapsedTime / duration);
			_secondaryGunParent.transform.position = Vector3.Lerp(_secondaryGunParent.transform.position, secondaryFinalPos, elapsedTime / duration);
			
			_mainGunParent.transform.rotation = Quaternion.Lerp(_mainGunParent.transform.rotation, mainFinalRot, elapsedTime / duration);
			_secondaryGunParent.transform.rotation = Quaternion.Lerp(_secondaryGunParent.transform.rotation, secondaryFinalRot, elapsedTime / duration);
			
			yield return null;
		}
		
		_mainGunParent.SetActive(_isMainGun);
		_secondaryGunParent.SetActive(!_isMainGun);
		
		_mainGunParent.transform.position = mainFinalPos;
		_secondaryGunParent.transform.position = secondaryFinalPos;
		
		_mainGunParent.transform.rotation = mainFinalRot;
		_secondaryGunParent.transform.rotation = secondaryFinalRot;

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

	public void ChangeCurrentGun_Main(int newIndex)
	{
		_currentMainGun = newIndex;

		ActivateCurrentGun(_mainGunsList, _currentMainGun);
	}
	public void ChangeCurrentGun_Secondary(int newIndex) => _currentSecondaryGun = newIndex;
	
	#endregion
}
