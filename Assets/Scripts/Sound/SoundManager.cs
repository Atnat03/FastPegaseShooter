using System.Collections.Generic;
using FishNet.Object;
using ScriptableObjectsDefinitions;
using UnityEngine;

public class SoundManager : MonoBehaviour
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
		PlayClipAtPoint(clip, source.transform.position, source, source.transform);
	}
	
	public static void PlayClipAtPoint(AudioClip clip, Vector3 position, AudioSource source, Transform parent = null)
	{
		GameObject gameObject = new GameObject(clip.name + " spatial play");
		gameObject.transform.position = position;
		
		gameObject.transform.SetParent(parent);
    
		AudioSource audioSource = (AudioSource) gameObject.AddComponent(typeof (AudioSource));
		audioSource.clip = clip;
		audioSource.spatialBlend = 1f;
		audioSource.volume = source.volume;
		
		audioSource.outputAudioMixerGroup = source.outputAudioMixerGroup;
		
		audioSource.Play();
		Object.Destroy((Object) gameObject, clip.length * ((double) Time.timeScale < 0.009999999776482582 ? 0.01f : Time.timeScale));
	}
}