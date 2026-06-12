using System;
using System.Threading.Tasks;
using CustomConsole.Runtime.Logger;
using ScriptableObjectsDefinitions;
using UnityEngine;

public class EnemyCoreViewer : MonoBehaviour
{
    [SerializeField] private EnemyCore _enemyCore;
    [SerializeField] private ParticleSystem _explosionVfx;
    
    [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;
    [SerializeField] private ParticleSystem _desintegrationParticles;
    [SerializeField] private GameObject _ballVFX;
    
    [SerializeField] private SoundsDataSO _soundData;
    [SerializeField] private AudioSource _source;
    
    private bool _deathTriggered;

    private void Awake()
    {
        _enemyCore.OnDapExplosion += OnDapExplosion;
        _enemyCore.OnSpawn += OnSpawn;
        _enemyCore.OnDeath += OnDeath;
    }

    private void OnDestroy()
    {
        _enemyCore.OnDapExplosion -= OnDapExplosion;
        _enemyCore.OnSpawn -= OnSpawn;
        _enemyCore.OnDeath -= OnDeath;
    }

    private void OnDapExplosion()
    {
        ParticleSystem ps = Instantiate(_explosionVfx, transform.position, Quaternion.identity);
        ps.Play();
    }
    private void OnSpawn()
    {
        if (_enemyCore.p_coreSo.p_spawningTime <= 0) return;
        
        CustomLogger.ImportantLog("On Spawn");
    }
    private async void OnDeath()
    {
        if (_enemyCore.p_coreSo.p_deathTime <= 0 ||
            !_skinnedMeshRenderer ||
            _deathTriggered) return;
        
        _deathTriggered = true;

        SoundManager.PlaySound(_soundData, "Death", _source);
        
        Material mat = _skinnedMeshRenderer.sharedMaterials[0];
        if (!mat.name.EndsWith("(Instance)"))
        {
            mat = Instantiate(mat);
            mat.name = $"{mat.name}(Instance)";
            Material[] materials = _skinnedMeshRenderer.sharedMaterials;
            materials[0] = mat;
            _skinnedMeshRenderer.sharedMaterials = materials;
        }
        mat = _skinnedMeshRenderer.sharedMaterials[1];
        if (!mat.name.EndsWith("(Instance)"))
        {
            mat = Instantiate(mat);
            mat.name = $"{mat.name}(Instance)";
            Material[] materials = _skinnedMeshRenderer.sharedMaterials;
            materials[1] = mat;
            _skinnedMeshRenderer.sharedMaterials = materials;
        }
        
        if(_ballVFX) _ballVFX.SetActive(false);
        if(_desintegrationParticles)
        {
            _desintegrationParticles.gameObject.SetActive(true);
            _desintegrationParticles.Play();
        }
        
        float t = 0;
        while (t < _enemyCore.p_coreSo.p_deathTime)
        {
            t+=Time.deltaTime;
            _skinnedMeshRenderer.sharedMaterials[0].SetFloat("_DESINTEGRATION", t/_enemyCore.p_coreSo.p_deathTime);
            _skinnedMeshRenderer.sharedMaterials[1].SetFloat("_DESINTEGRATION", t/_enemyCore.p_coreSo.p_deathTime);
            await Task.Yield();
        }
        _skinnedMeshRenderer.sharedMaterials[0].SetFloat("_DESINTEGRATION", 1);
        _skinnedMeshRenderer.sharedMaterials[1].SetFloat("_DESINTEGRATION", 1);
        
    }
}
