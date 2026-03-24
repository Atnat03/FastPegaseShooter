using System;
using System.Collections;
using UnityEngine;

public class GunSwitchingView : MonoBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private GunSwitching _gunSwitching;
	
	[Header("UI Main")]
	[SerializeField] private GameObject _enableMainImage;
	[SerializeField] private GameObject _disableMainImage;

	[Header("UI Second")]
	[SerializeField] private GameObject _enableSecondImage;
	[SerializeField] private GameObject _disableSecondImage;
	
	#endregion


	#region Fonctions

	private void OnEnable()
	{
		SwapUI();
		_gunSwitching.OnStartSwitchGun += SwapUI;
	}

	private void SwapUI()
	{
		bool main = _gunSwitching.IsMainGun;
		
		_enableMainImage.SetActive(main);
		_disableMainImage.SetActive(!main);
		_enableSecondImage.SetActive(!main);
		_disableSecondImage.SetActive(main);
		
		//StartCoroutine(SwapGunsUI());
	}

	/*IEnumerator SwapGunsUI(GameObject target, Transform final, bool main)
	{
		
	}*/


	private void OnDisable()
	{
		_gunSwitching.OnStartSwitchGun -= SwapUI;
	}

	#endregion
}
