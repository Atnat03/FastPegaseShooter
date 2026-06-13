using System;
using ScriptableObjectsDefinitions;
using UnityEngine;
using UnityEngine.UI;

public struct PlayUISound
{
	public string keySound;
}

public class PlayerUISettings : NetworkBusListener
{
	#region Properties

	#endregion


	#region Variables

	[Header("Sound")]
	[SerializeField] private SoundsDataSO _soundsData;
	[SerializeField] private AudioSource _source;
	
	[Header("Colors")]
	[SerializeField] private GunSwitching _gunSwitching;
	[SerializeField] private Image[] _imageList;
	[SerializeField] private Color[] _colorList;
	
	#endregion


	#region Fonctions

	private void Start()
	{
		ApplyColor();
	}
	
	public override void OnStartNetwork()
	{
		ListenToEvent<PlayUISound>(PlaySoundUI);
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
	
	private void ApplyColor()
	{
		Color c = _gunSwitching.IsPositive ? _colorList[0] : _colorList[1];

		foreach (Image image in _imageList)
		{
			image.color = c;
		}
	}

	#endregion
}
