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
	FPSController _fpsController;
	
	#endregion


	#region Fonctions

	private void Awake()
	{
		_playerInput = GetComponent<PlayerInput>();
		_fpsController = GetComponent<FPSController>();

		ui.SetActive(false);
	}
	
	
	private void ActivateUI(InputAction.CallbackContext obj)
	{
		ui.SetActive(!ui.activeSelf);
		
		Cursor.lockState = ui.activeSelf ?  CursorLockMode.None : CursorLockMode.Locked;
		Cursor.visible = ui.activeSelf;
		
		_fpsController.IsFreeze = ui.activeSelf;
	}

	public void DesactivateUI()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		ui.SetActive(false);
		_fpsController.IsFreeze = false;
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
