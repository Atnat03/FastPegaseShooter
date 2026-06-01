using System;
using MyPrint;
using UnityEngine;
using UnityEngine.UI;

public class EnemyCoreViewer : MonoBehaviour
{
    [SerializeField] private EnemyCore _enemyCore;
    [SerializeField] private ParticleSystem _explosionVfx;    

    private void Awake()
    {
        _enemyCore.OnDapExplosion += OnDapExplosion;
    }

    private void OnDapExplosion()
    {
        ParticleSystem ps = Instantiate(_explosionVfx, transform.position, Quaternion.identity);
        ps.Play();
    }
}
