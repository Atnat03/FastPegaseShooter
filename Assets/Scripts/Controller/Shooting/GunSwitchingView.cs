using System;
using System.Collections;
using UnityEngine;

public class GunSwitchingView : MonoBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private GunSwitching _gunSwitching;
	[SerializeField] private Transform _enablePos;
	[SerializeField] private Transform _disablePos;
	
	
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
		_enableMainImage.SetActive(_gunSwitching.IsMainGun);
		_disableMainImage.SetActive(!_gunSwitching.IsMainGun);
		_enableSecondImage.SetActive(!_gunSwitching.IsMainGun);
		_disableSecondImage.SetActive(_gunSwitching.IsMainGun);
		
		_gunSwitching.OnStartSwitchGun += SwapUI;
	}

	private void SwapUI()
	{
		bool main = _gunSwitching.IsMainGun;
		GameObject dObject, eObject;
		
		if (main)
		{
			eObject = _enableMainImage;
			dObject = _enableSecondImage;
		}
		else
		{
			dObject = _enableMainImage;
			eObject = _enableSecondImage;
		}
		
		StartCoroutine(SwapGunsUI(dObject, eObject, main));
	}

	IEnumerator SwapGunsUI(GameObject disableObject, GameObject enableObject ,bool main)
	{
		_enableMainImage.SetActive(true);
		_enableSecondImage.SetActive(true);
		
		CanvasGroup disable = disableObject.GetComponent<CanvasGroup>();
		CanvasGroup enable = enableObject.GetComponent<CanvasGroup>();
		
		float duration = 0.5f;
		float elapsedTime = 0f;

		enable.alpha = 0;
		disable.alpha = 1;

		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			
			enable.alpha = Mathf.Lerp(0, 1, elapsedTime / duration);
			disable.alpha = Mathf.Lerp(1, 0, elapsedTime / duration);
			
			disableObject.transform.position = Vector3.Lerp(disableObject.transform.position, _disablePos.position, elapsedTime / duration);
			enableObject.transform.position = Vector3.Lerp(disableObject.transform.position, _enablePos.position, elapsedTime / duration);
			
			yield return null;
		}
		
		enable.alpha = 1;
		disable.alpha = 0;
				
		_enableMainImage.SetActive(main);
		_disableMainImage.SetActive(!main);
		_enableSecondImage.SetActive(!main);
		_disableSecondImage.SetActive(main);
	}


	private void OnDisable()
	{
		_gunSwitching.OnStartSwitchGun -= SwapUI;
	}

	#endregion
}
