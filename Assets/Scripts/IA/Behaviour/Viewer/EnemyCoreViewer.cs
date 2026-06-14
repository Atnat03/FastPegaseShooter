using System;
using System.Threading.Tasks;
using CustomConsole.Runtime.Logger;
using ScriptableObjectsDefinitions;
using UnityEngine;

public class EnemyCoreViewer : MonoBehaviour
{
    [SerializeField] private EnemyCore _enemyCore;
    [SerializeField] private ParticleSystem _explosionVfx;
    
    [Header("Spawn")]
    [SerializeField] private ParticleSystem _SpawnParticle;
    [SerializeField] private SkinnedMeshRenderer _SpawnParticleSkinnedMesh;
    
    [Header("Death")]
    [SerializeField] private SkinnedMeshRenderer _EnemySkinnedMeshRenderer;
    [SerializeField] private ParticleSystem _desintegrationParticles;
    
    [Header("Shoot")]
    [SerializeField] private GameObject _ballVFX;
    
    [Header("Sounds")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private SoundsDataSO _soundData;
    
    private bool _deathTriggered;

    private void Awake()
    {
        if(_EnemySkinnedMeshRenderer) _EnemySkinnedMeshRenderer.gameObject.SetActive(false);
        
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
    private async void OnSpawn()
    {
        if (_enemyCore.p_coreSo.p_spawningTime <= 0) return;
        
        if(_ballVFX) _ballVFX.SetActive(false);
        if(_EnemySkinnedMeshRenderer) _EnemySkinnedMeshRenderer.gameObject.SetActive(false);
        
        if(_audioSource && _soundData)
            SoundManager.PlaySound(_soundData, "Spawn", _audioSource);
        
        if(_SpawnParticle)
        {
            _SpawnParticle.gameObject.SetActive(true);
            _SpawnParticle.Play();
            await Task.Delay(Mathf.FloorToInt(_SpawnParticle.main.duration * 1000));
            Debug.Log($"spawn particle time :  {_SpawnParticle.main.duration}");
        }
        
        if(_SpawnParticleSkinnedMesh)
        {
            Material mat = _SpawnParticleSkinnedMesh.sharedMaterials[0];
            if (!mat.name.EndsWith("(Instance)"))
            {
                mat = Instantiate(mat);
                mat.name = $"{mat.name}(Instance)";
                Material[] materials = _SpawnParticleSkinnedMesh.sharedMaterials;
                materials[0] = mat;
                _SpawnParticleSkinnedMesh.sharedMaterials = materials;
            }

            if(_EnemySkinnedMeshRenderer) _EnemySkinnedMeshRenderer.gameObject.SetActive(true);
            
            float t = 0;
            float explosionTime = _SpawnParticle
                ? _enemyCore.p_coreSo.p_spawningTime - _SpawnParticle.main.duration
                : _enemyCore.p_coreSo.p_spawningTime;
            _SpawnParticleSkinnedMesh.gameObject.SetActive(true);
            while (t < explosionTime)
            {
                t += Time.deltaTime;
                _SpawnParticleSkinnedMesh.sharedMaterials[0].SetFloat("_CHANGEMENT", t / explosionTime);
                _SpawnParticleSkinnedMesh.SetBlendShapeWeight(0, t / explosionTime * 100);
                await Task.Yield();
            }
        }
        
        if(_SpawnParticleSkinnedMesh) _SpawnParticleSkinnedMesh.gameObject.SetActive(false);
        if(_EnemySkinnedMeshRenderer) _EnemySkinnedMeshRenderer.gameObject.SetActive(true);
        if(_ballVFX) _ballVFX.SetActive(true);
    }
    private async void OnDeath()
    {
        if (_enemyCore.p_coreSo.p_deathTime <= 0 ||
            !_EnemySkinnedMeshRenderer ||
            _deathTriggered) return;
        
        _deathTriggered = true;

        if(_audioSource && _soundData)
            SoundManager.PlaySound(_soundData, "Death", _audioSource);
        
        Material mat = _EnemySkinnedMeshRenderer.sharedMaterials[0];
        if (!mat.name.EndsWith("(Instance)"))
        {
            mat = Instantiate(mat);
            mat.name = $"{mat.name}(Instance)";
            Material[] materials = _EnemySkinnedMeshRenderer.sharedMaterials;
            materials[0] = mat;
            _EnemySkinnedMeshRenderer.sharedMaterials = materials;
        }
        mat = _EnemySkinnedMeshRenderer.sharedMaterials[1];
        if (!mat.name.EndsWith("(Instance)"))
        {
            mat = Instantiate(mat);
            mat.name = $"{mat.name}(Instance)";
            Material[] materials = _EnemySkinnedMeshRenderer.sharedMaterials;
            materials[1] = mat;
            _EnemySkinnedMeshRenderer.sharedMaterials = materials;
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
            _EnemySkinnedMeshRenderer.sharedMaterials[0].SetFloat("_DESINTEGRATION", t/_enemyCore.p_coreSo.p_deathTime);
            _EnemySkinnedMeshRenderer.sharedMaterials[1].SetFloat("_DESINTEGRATION", t/_enemyCore.p_coreSo.p_deathTime);
            await Task.Yield();
        }
        _EnemySkinnedMeshRenderer.sharedMaterials[0].SetFloat("_DESINTEGRATION", 1);
        _EnemySkinnedMeshRenderer.sharedMaterials[1].SetFloat("_DESINTEGRATION", 1);
        
    }
}
