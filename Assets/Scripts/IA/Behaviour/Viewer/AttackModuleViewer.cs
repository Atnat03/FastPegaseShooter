using System;
using System.Threading.Tasks;
using MyPrint;
using ScriptableObjectsDefinitions;
using UnityEngine;

public class AttackModuleViewer : MonoBehaviour
{
    [SerializeField] private EnemyAttackModule _attackModule;
    [SerializeField] private float _waitTime;
    [SerializeField] private ParticleSystem _particleSystem;
    
    [Header("Sounds")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private SoundsDataSO _soundData;

    private void Awake()
    {
        if(_attackModule) _attackModule.p_onAttack += POnAttack;
    }

    private async void POnAttack()
    {
        if(_audioSource && _soundData)
            SoundManager.PlaySound(_soundData, "Attack", _audioSource);
        
        if (_particleSystem == null)
            return;
        
        _particleSystem.gameObject.SetActive(true);
        await Task.Delay((int)(_waitTime * 1000));
        _particleSystem.gameObject.SetActive(false);
    }
}
