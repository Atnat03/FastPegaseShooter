using System;
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

	[Header("Sound")]
	[SerializeField] private SoundsDataSO _soundsData;
	[SerializeField] private AudioSource _source;
	
	[Header("Colors")]
	[SerializeField] private GunSwitching _gunSwitching;
	[SerializeField] private ColorToChange[] _imageToChangeList;
	
	#endregion


	#region Fonctions
	
	public override void OnStartNetwork()
	{
		ListenToEvent<PlayUISound>(PlaySoundUI);
		ListenToEvent<OnPlayerOk>(ApplyColor);
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

	#endregion
}
