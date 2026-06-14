using System;
using System.Collections;
using FishNet;
using FishNet.Managing.Scened;
using ScriptableObjectsDefinitions;
using UnityEngine;
using UnityEngine.UI;

public struct PlayUISound
{
	public string keySound;
}

[Serializable]
public struct ColorToChange
{
	public Image image;
	public Color colorPositive;
	public Color colorNegative;
}

public class PlayerUISettings : NetworkBusListener
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private Canvas _canvas;
	
	[Header("Sound")]
	[SerializeField] private SoundsDataSO _soundsData;
	[SerializeField] private AudioSource _source;
	
	[Header("LoadingScene")]
	[SerializeField] private GameObject _loadingSceneUI;
	
	[Header("Colors")]
	[SerializeField] private GunSwitching _gunSwitching;
	[SerializeField] private ColorToChange[] _imageToChangeList;

	private int baseSortingCanvaLayer;
	private bool _isLoading;
	
	#endregion


	#region Fonctions
	
	public override void OnStartNetwork()
	{
		ListenToEvent<PlayUISound>(PlaySoundUI);
		ListenToEvent<OnPlayerOk>(ApplyColor);

		baseSortingCanvaLayer = _canvas.sortingOrder;
		
		ListenToEvent<OnShowLoadingScreen>(_ =>
		{
			_isLoading = true;
			_canvas.sortingOrder = 10;
			_loadingSceneUI.SetActive(true);
		});


		InstanceFinder.SceneManager.OnLoadEnd += OnLoadEnd;
	}
	
	public override void OnStopNetwork()
	{
		if (InstanceFinder.SceneManager != null)
			InstanceFinder.SceneManager.OnLoadEnd -= OnLoadEnd;
	}

	private void PlaySoundUI(PlayUISound data)
	{
		if (SoundManager.GetAudioClip(_soundsData, data.keySound) != null)
		{
			SoundManager.PlaySound(_soundsData, data.keySound, _source);
		}
		else
		{
			Debug.LogError("Key not found in sound data");
		}
	}

	public void PlaySoundUI(string key)
	{
		if (SoundManager.GetAudioClip(_soundsData, key) != null)
		{
			SoundManager.PlaySound(_soundsData, key, _source);
		}
		else
		{
			Debug.LogError("Key not found in sound data");
		}
	}
	
	private void ApplyColor(OnPlayerOk playerData)
	{
		if (playerData.playerID != Owner.ClientId)
			return;
		
		foreach (ColorToChange data in _imageToChangeList)
		{
			data.image.color = playerData.IsPositive ? data.colorPositive : data.colorNegative;
		}
	}
	
	private void OnLoadEnd(SceneLoadEndEventArgs args)
	{
		StartCoroutine(HideLoading());
	}

	private IEnumerator HideLoading()
	{
		yield return null;

		_loadingSceneUI.SetActive(false);
		_canvas.sortingOrder = baseSortingCanvaLayer;
		_isLoading = false;
	}

	#endregion
}
