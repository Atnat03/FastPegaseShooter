using System;
using System.Collections;
using MyPrint;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GunSwitchingView : MonoBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private GunSwitching _gunSwitching;

	[Header("UI")] 
	[SerializeField] private Sprite armGunSprite;
	[SerializeField] private Image imageGun;
	[SerializeField] private GameObject ammo;

	void OnEnable()
	{
		_gunSwitching.OnNotMainGunChange += SetIcone;
	}

	private void OnDisable()
	{
		_gunSwitching.OnNotMainGunChange -= SetIcone;
	}

	private void SetIcone(bool isMain)
	{
		if(!isMain) 
			imageGun.sprite = armGunSprite;
		
		ammo.SetActive(isMain);
	}

	#endregion 
}
