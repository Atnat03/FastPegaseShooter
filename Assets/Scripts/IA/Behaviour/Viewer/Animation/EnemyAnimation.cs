using System;
using UnityEditor;
using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    [SerializeField] private EnemyMovementModule _movementModule;
    [SerializeField] private EnemyAttackModule _attackModule;
    [SerializeField] private Animator _animator;

    [SerializeField] private Transform _enemyMeshTransform;
    [SerializeField] private Vector3 _localRotationIdle;
    [SerializeField] private Vector3 _localRotationWalk;

    private void Start()
    {
        if (_movementModule)
            _movementModule.p_onChangeMovement += POnChangeMovement;
        
        if(_attackModule)
            _attackModule.p_onAttack += Attack;
        
    }

    private void POnChangeMovement(bool isWalking)
    {
        if(isWalking) ToWalk();
        else ToIdle();
    }

    public void ToIdle()
    {
        _animator.SetBool("IsWalking", false);
        if(_enemyMeshTransform) _enemyMeshTransform.localRotation = Quaternion.Euler(_localRotationIdle);
    }
    public void ToWalk()
    {
        _animator.SetBool("IsWalking", true);
        if(_enemyMeshTransform) _enemyMeshTransform.localRotation = Quaternion.Euler(_localRotationWalk);
    }

    public void Attack()
    {
        _animator.SetTrigger("Attack");
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(EnemyAnimation))]
public class EnemyAnimationInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EnemyAnimation script = (EnemyAnimation)target;
        
        GUILayout.Space(20);
        GUILayout.Label("Debug", EditorStyles.boldLabel);
        if (GUILayout.Button("Go to Idle"))
        {
            script.ToIdle();
        }
        if (GUILayout.Button("Go to walk"))
        {
            script.ToWalk();
        }
        if (GUILayout.Button("Make Shoot"))
        {
            script.Attack();
        }
    }
}

#endif
