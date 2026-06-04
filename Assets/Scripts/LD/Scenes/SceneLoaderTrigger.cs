using System.Collections.Generic;
using FishNet;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;

namespace LD.Scenes
{
	public class SceneLoaderTrigger : NetworkBusListener
	{

		#region Variables
		[SerializeField] private GameObject _door;
		[SerializeField] private SceneField _sceneToLoad;
		#endregion
		
		#region Fonctions
		
		void Start()
		{
			ListenToEvent<OnDapEvent>(OpenDoor);
		}

		void OpenDoor(OnDapEvent evt)
		{
			if (!_door) return;
			if(_door.GetComponent<Animation>()) _door.GetComponent<Animation>().Play();
			else _door.SetActive(false);
		}
		

		private void OnTriggerEnter(Collider other)
		{
			if (other.TryGetComponent(out PlayerVisuelBridge player))
			{
				InvokeEvent(new OnSceneLoadTrigger());
				LoadSceneServerRpc();
			}
		}

		[ServerRpc(RequireOwnership = false)]
		private void LoadSceneServerRpc()
		{
			LoadSceneForAll();
		}

		[Server]
		private void LoadSceneForAll()
		{
			
			// Ajoute ce debug temporaire dans LoadSceneForAll
			Debug.Log($"Chargement de la scène : '{_sceneToLoad.SceneName}'");
			Debug.Log($"Nombre de joueurs : {PlayerHealthManager.Instance.RegisteredPlayers.Count}");
			
			
			SceneLoadData sld = new SceneLoadData(_sceneToLoad.SceneName);
			sld.ReplaceScenes = ReplaceOption.All;

			List<NetworkObject> objsList = new List<NetworkObject>();

			foreach (var player in PlayerHealthManager.Instance.RegisteredPlayers)
			{
				objsList.Add(player.NetworkObject);
			}
			
			sld.MovedNetworkObjects = objsList.ToArray();

			InstanceFinder.SceneManager.LoadGlobalScenes(sld);
		}
		
		#endregion
	}
}

public struct OnSceneLoadTrigger
{
	
}