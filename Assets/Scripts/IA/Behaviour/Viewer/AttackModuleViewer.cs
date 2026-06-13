using System;
using System.Threading.Tasks;
using MyPrint;
using UnityEngine;

public class AttackModuleViewer : MonoBehaviour
{
    [SerializeField] private EnemyAttackModule _attackModule;
    [SerializeField] private float _waitTime;
    [SerializeField] private ParticleSystem _particleSystem;

    private void Awake()
    {
        if(_attackModule) _attackModule.p_onAttack += POnAttack;
    }

    private async void POnAttack()
    {
        Cons.Print("C'est ic Aloys !!", ColorConsole.Red, ConsoleStyle.Bold);
        
        if (_particleSystem == null)
            return;
        
        _particleSystem.gameObject.SetActive(true);
        await Task.Delay((int)(_waitTime * 1000));
        _particleSystem.gameObject.SetActive(false);
    }
}
