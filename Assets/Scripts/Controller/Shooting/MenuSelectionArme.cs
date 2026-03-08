using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuSelectionArme : MonoBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private GameObject ui;
	
	PlayerInput _playerInput;
	
	#endregion


	#region Fonctions

	private void Awake()
	{
		_playerInput = GetComponent<PlayerInput>();

		ui.SetActive(false);
	}
	
	
	private void ActivateUI(InputAction.CallbackContext obj)
	{
		ui.SetActive(!ui.activeSelf);
		
		Cursor.lockState = ui.activeSelf ?  CursorLockMode.None : CursorLockMode.Locked;
		Cursor.visible = ui.activeSelf;
	}

	public void DesactivateUI()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		ui.SetActive(false);
	}
	
	
	private void OnEnable()
	{
		_playerInput.actions["ChooseGun"].performed += ActivateUI;
	}

	
	private void OnDisable()
	{
		_playerInput.actions["ChooseGun"].performed -= ActivateUI;
	}


	#endregion
}
