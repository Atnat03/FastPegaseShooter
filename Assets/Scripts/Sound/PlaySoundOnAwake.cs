using System;
using ScriptableObjectsDefinitions;
using UnityEngine;

namespace Sound
{
    public class PlaySoundOnAwake : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private SoundsDataSO _dataSound;
        [SerializeField] private string _keySound;
        
        private void OnEnable()
        {
            SoundManager.PlaySound(_dataSound, _keySound, _audioSource);
        }
    }
}