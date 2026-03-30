using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class RespawnManager : NetworkBusListener
{
	#region Properties

	#endregion


	#region Variables

	private readonly SyncVar<int> _nbPlayerDead = new SyncVar<int>(0);
	private readonly SyncVar<bool> _isGameOver = new SyncVar<bool>(false);

	[Header("UI")] 
	[SerializeField] private GameObject _playerUIEnd;
	
	#endregion


	#region Fonctions

	public override void OnStartServer()
	{
		//Bus

		ListenToEvent<OnPlayerDeathEvent>(CheckAllPlayerDead);
		ListenToEvent<OnPlayerRespawnEvent>(PlayerRespawn);
	}

	public override void OnStartClient()
	{
		_playerUIEnd.SetActive(false);

		_nbPlayerDead.OnChange += OnNbPlayerDeadChange;
	}

	private void PlayerRespawn(OnPlayerRespawnEvent data)
	{
		_nbPlayerDead.Value = 0;
	}

	private void CheckAllPlayerDead(OnPlayerDeathEvent data)
	{
		_nbPlayerDead.Value++;
	}
	
	private void OnNbPlayerDeadChange(int prev, int next, bool asServer)
	{
		if(_nbPlayerDead.Value == InstanceFinder.ClientManager.Clients.Count)
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			_isGameOver.Value = true;
			_playerUIEnd.SetActive(true);
		}
	}

	public void Quit()
	{
		Debug.Log("Quit");
		Application.Quit();
	}


	#endregion
}
