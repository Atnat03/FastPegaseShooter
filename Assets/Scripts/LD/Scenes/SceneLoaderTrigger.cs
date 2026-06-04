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
				LoadSceneForAll();
			}
		}

		[Server]
		private void LoadSceneForAll()
		{
			SceneLoadData sld = new SceneLoadData(_sceneToLoad.SceneName);
			sld.ReplaceScenes = ReplaceOption.All;

			NetworkObject[] objs = new NetworkObject[PlayerHealthManager.Instance.RegisteredPlayers.Count + 1];
			for (int i = 0; i < PlayerHealthManager.Instance.RegisteredPlayers.Count; i++)
			{
				objs[i] = PlayerHealthManager.Instance.RegisteredPlayers[i].NetworkObject;
			}
			sld.MovedNetworkObjects = objs;
            
			InstanceFinder.SceneManager.LoadGlobalScenes(sld);
		}
		
		#endregion
	}
}

public struct OnSceneLoadTrigger
{
	
}