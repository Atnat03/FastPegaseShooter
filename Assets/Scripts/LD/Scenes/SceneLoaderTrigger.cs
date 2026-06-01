using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LD.Scenes
{
	public class SceneLoaderTrigger : MonoBusListener
	{
		[SerializeField] private GameObject _door;

		#region Variables

		[SerializeField] private SceneField[] _sceneToLoad;
		[SerializeField] private SceneField[] _sceneToUnload;

		#endregion


		#region Fonctions
		
		void Start()
		{
			ListenToEvent<OnDapEvent>(OpenDoor);
		}

		void OpenDoor(OnDapEvent evt)
		{
			if(_door.GetComponent<Animation>()) _door.GetComponent<Animation>().Play();
			else _door.SetActive(false);
		}
		

		private void OnTriggerEnter(Collider other)
		{
			if (other.TryGetComponent(out PlayerVisuelBridge player))
			{
				SceneManaging.LoadScene(_sceneToLoad);
				SceneManaging.UnloadScene(_sceneToUnload);
			}
		}

		#endregion
	}
}