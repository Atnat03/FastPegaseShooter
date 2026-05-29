using System;
using MyPrint;
using UnityEngine;
using UnityEngine.UI;

public class EnemyCoreViewer : MonoBehaviour
{
    [SerializeField] private EnemyCore _enemyCore;
    

    private void Awake()
    {
        _enemyCore.OnDapExplosion += OnDapExplosion;
    }

    private void OnDapExplosion()
    {
        //create explosion particle here
    }
}
