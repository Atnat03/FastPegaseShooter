using UnityEngine;

namespace LD.Scenes
{
	public class StartGameTest : MonoBehaviour
	{
		#region Variables

		[SerializeField] private SceneField[] _sceneToLoadOnStart;
		
		#endregion


		#region Fonctions

		void Start()
		{
			SceneManaging.LoadScene(_sceneToLoadOnStart);
		}
		
		#endregion
	}
}
