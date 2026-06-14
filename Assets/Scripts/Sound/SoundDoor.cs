using ScriptableObjectsDefinitions;
using UnityEngine;

public class SoundDoor : MonoBehaviour
{
    public SoundsDataSO Data;
    public AudioSource AudioSource;
    public string key;

    public void PlaySound()
    {
        SoundManager.PlaySound(Data, key, AudioSource);
    }
}
