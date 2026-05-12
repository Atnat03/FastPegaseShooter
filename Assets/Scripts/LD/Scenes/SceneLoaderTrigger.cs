using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LD.Scenes
{
	public class SceneLoaderTrigger : MonoBehaviour
	{
		#region Properties

		#endregion


		#region Variables

		[SerializeField] private SceneField[] _sceneToLoad;
		[SerializeField] private SceneField[] _sceneToUnload;

		#endregion


		#region Fonctions

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