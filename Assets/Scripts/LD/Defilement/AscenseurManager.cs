using System;
using FishNet.Object;
using UnityEngine;

public class AscenseurManager : NetworkBusListener
{
	#region Properties

	#endregion


	#region Variables

	[Header("Prefabs")]
	[SerializeField] private Ascenseur _ascenseurPrefabDefault;
	[SerializeField] private Ascenseur _ascenseurPrefabStart;
	[SerializeField] private Ascenseur _ascenseurPrefabEnd;
	
	[Header("Settings")]
	[SerializeField] private Transform _spawnAscenseurPoint;
	[SerializeField] private Transform _endRunAscenseurPoint;
	[SerializeField] private float _durationTraveling;

	#endregion
	
	#region Fonctions
	
	public override void OnStartNetwork()
	{
		SpawnNewAscenseur();
	}

	[ContextMenu("Spawn Ascenseur")]
	public void SpawnNewAscenseur()
	{
		if(IsServerInitialized)
			SpawnNewAscenseurObserverRpc();
		else
		{
			RequestSpawnAscenseurServerRpc();
		}
	}

	[ServerRpc]
	private void RequestSpawnAscenseurServerRpc()
	{
		SpawnNewAscenseurObserverRpc();
	}

	[ObserversRpc]
	private void SpawnNewAscenseurObserverRpc()
	{
		//si plus de monstres => on change le prefab pour celui de fin
		
		Ascenseur newAscenseur = Instantiate(_ascenseurPrefabDefault, _spawnAscenseurPoint.position, _spawnAscenseurPoint.rotation, transform);
		
		newAscenseur.StartDescente(
			_spawnAscenseurPoint.position,
			_endRunAscenseurPoint.position, 
			_durationTraveling, this);
	}
	
	#endregion
}
