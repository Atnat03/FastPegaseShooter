using System;
using FishNet;
using FishNet.Managing.Scened;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class WaitingManager : MonoBehaviour
{

	#region Variables

	[SerializeField] private Button _buttonReady;
	
	#endregion


	#region Fonctions

	private void Awake()
	{
		_buttonReady.onClick.AddListener(() => CheckAllPlayerReady());
	}

	private void CheckAllPlayerReady()
	{
		Debug.Log("Load GAME SCENE");
		
		SceneLoadData scene =  new SceneLoadData("GameTest");
		scene.ReplaceScenes = ReplaceOption.All;
		InstanceFinder.SceneManager.LoadGlobalScenes(scene);
	}

	#endregion
}
