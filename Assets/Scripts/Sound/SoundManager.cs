using System.Collections.Generic;
using FishNet.Object;
using ScriptableObjectsDefinitions;
using UnityEngine;

public class SoundManager : NetworkBehaviour
{
	public static AudioClip GetAudioClip(SoundsDataSO data, string soundName)
	{
		List<AudioClip> clip = new List<AudioClip>();

		foreach (SoundData soundData in data.sounds)
		{
			if(soundData.soundName == soundName)
			{
				clip.Add(soundData.audioClip);
			}
		}
		
		return clip[Random.Range(0, clip.Count)];
	}
	
	public static void PlaySound(AudioClip clip, AudioSource source, float volume = 0.5f, float pitch = 1f)
	{
		source.pitch = pitch;
		source.volume = volume;
		AudioSource.PlayClipAtPoint(clip, source.transform.position, source.volume);
	}
	
	public static void PlaySound(SoundsDataSO data, string soundName, AudioSource source)
	{
		List<AudioClip> clip = new List<AudioClip>();
		float volume = 0.5f;
		SoundType t = SoundType.Global;

		foreach (SoundData soundData in data.sounds)
		{
			if(soundData.soundName == soundName)
			{
				clip.Add(soundData.audioClip);
				volume = soundData.volume;
				t = soundData.type;
			}
		}

		if (clip.Count == 0)
			return;
		
		AudioClip c = clip[Random.Range(0, clip.Count)];
		
		switch (t)
		{
			case SoundType.Spatial: PlaySpatialSound(volume, c, source); break;
			case SoundType.Global: PlayGlobalSound(volume, c, source); break;
		}
	}

	private static void PlayGlobalSound(float volume, AudioClip clip, AudioSource source)
	{
		source.PlayOneShot(clip, volume);
	}

	private static void PlaySpatialSound(float volume, AudioClip clip, AudioSource source)
	{
		source.pitch = 1f;
		source.volume = volume;
		AudioSource.PlayClipAtPoint(clip, source.transform.position, source.volume);
	}
}