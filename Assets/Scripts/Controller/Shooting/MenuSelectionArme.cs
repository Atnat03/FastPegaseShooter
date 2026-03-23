using System;
using FishNet;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuSelectionArme : NetworkBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private GameObject ui;
	
	PlayerInput _playerInput;
	[SerializeField] private GunSwitching _gunSwitching;
	[SerializeField] private Image[] _imagesCircles;
	[SerializeField] private Color _baseColor;
	[SerializeField] private Color _selectedColor;
	
	#endregion


	#region Fonctions

	// ⚠️ c'est crade mais c'est pour le debug 
	
	private void Awake()
	{
		_playerInput = GetComponent<PlayerInput>();
		ActivateUI(_gunSwitching.CurrentMainGunIndex);
	}

	void ChangeGun1(InputAction.CallbackContext obj)
	{
		if (!IsOwner) return;
		_gunSwitching.ChangeCurrentGun_Main(0); ActivateUI(0); 
	}
	void ChangeGun2(InputAction.CallbackContext obj) 
	{ 		
		if (!IsOwner) return;
		_gunSwitching.ChangeCurrentGun_Main(1);ActivateUI(1);
	}
	void ChangeGun3(InputAction.CallbackContext obj) 
	{ 		
		if (!IsOwner) return;
		_gunSwitching.ChangeCurrentGun_Main(2);ActivateUI(2);
	}
	void ChangeGun4(InputAction.CallbackContext obj) 
	{
		if (!IsOwner) return;
		_gunSwitching.ChangeCurrentGun_Main(3);ActivateUI(3);
	}

	void ActivateUI(int index )
	{
		for (int i = 0; i < _imagesCircles.Length; i++)
		{
			if (i == index)
			{
				_imagesCircles[i].color = _selectedColor;
			}
			else
			{
				_imagesCircles[i].color = _baseColor;
			}
		}
	}
	
	private void OnEnable()
	{
		_playerInput.actions["ChooseGun1"].performed += ChangeGun1;
		_playerInput.actions["ChooseGun2"].performed += ChangeGun2;
		_playerInput.actions["ChooseGun3"].performed += ChangeGun3;
		_playerInput.actions["ChooseGun4"].performed += ChangeGun4;
	}

	private void OnDisable()
	{
		_playerInput.actions["ChooseGun1"].performed -= ChangeGun1;
		_playerInput.actions["ChooseGun2"].performed -= ChangeGun2;
		_playerInput.actions["ChooseGun3"].performed -= ChangeGun3;
		_playerInput.actions["ChooseGun4"].performed -= ChangeGun4;	
	}


	#endregion
}
