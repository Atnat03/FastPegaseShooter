using FishNet.Object;
using ScriptableObjectsDefinitions;
using UnityEngine;

public class SoundManager : NetworkBehaviour
{
	public static AudioClip GetAudioClip(SoundsDataSO data, string soundName)
	{
		AudioClip clip = null;

		foreach (SoundData soundData in data.sounds)
		{
			if(soundData.soundName == soundName)
			{
				clip = soundData.audioClip;
			}
		}
		
		return clip;
	}
	
	public static void PlaySound(AudioClip clip, AudioSource source, float volume = 0.5f, float pitch = 1f)
	{
		source.pitch = pitch;
		source.volume = volume;
		AudioSource.PlayClipAtPoint(clip, source.transform.position, source.volume);
	}
	
	public static void PlaySound(SoundsDataSO data, string soundName, AudioSource source)
	{
		AudioClip clip = null;
		float volume = 0.5f;
		SoundType t = SoundType.Global;

		foreach (SoundData soundData in data.sounds)
		{
			if(soundData.soundName == soundName)
			{
				clip = soundData.audioClip;
				volume = soundData.volume;
				t = soundData.type;
			}
		}

		switch (t)
		{
			case SoundType.Global: PlayGlobalSound(volume, clip, source); break;
			case SoundType.Spatial: PlaySpatialSound(volume, clip, source); break;
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