using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using MyPrint;
using Unity.VisualScripting;
using UnityEngine;

public class EndGameManager : NetworkBusListener
{
	#region Variables

	[SerializeField] private Camera _cameraEnd;
	
	List<FPSController> _playerList = new List<FPSController>(); 
	
	#endregion

	#region Fonctions

	public override void OnStartNetwork()
	{
		_cameraEnd.gameObject.SetActive(false);
		
		ListenToEvent<OnDapEvent>(StartEndGame);
	}

	private void StartEndGame(OnDapEvent data)
	{
		if (!IsServerInitialized) return;
	
		_playerList.Add(InstanceFinder.ServerManager.Clients[0].FirstObject.GetComponent<FPSController>());
		_playerList.Add(InstanceFinder.ServerManager.Clients[1].FirstObject.GetComponent<FPSController>());

		foreach (FPSController player in _playerList)
		{
			player.SetFreeze(true);
		}

		StartEndGameObserverRpc();
	}

	[ObserversRpc]
	private void StartEndGameObserverRpc()
	{
		StartCoroutine(EndGame());
	}

	IEnumerator EndGame()
	{
		yield return new WaitForSeconds(2);
		
		_cameraEnd.gameObject.SetActive(true);
		
		yield return new WaitForSeconds(5f);
		
		Cons.Print("End game !!", ColorConsole.Cyan);
	}

	#endregion
}
