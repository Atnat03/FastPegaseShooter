using ScriptableObjectsDefinitions;
using UnityEngine;

public class UISound : MonoBehaviour
{
	#region Properties

	#endregion


	#region Variables

	[SerializeField] private SoundsDataSO soundData;
	[SerializeField] private AudioSource audioSource;
	
	#endregion


	#region Fonctions

	public void PlaySound(string key)
	{
		SoundManager.PlaySound(soundData, key, audioSource);
	}
	
	#endregion
}
