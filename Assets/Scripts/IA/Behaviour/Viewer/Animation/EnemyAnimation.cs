using UnityEditor;
using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    [SerializeField] private Animator _animator;
}

#if UNITY_EDITOR

[CustomEditor(typeof(EnemyAnimation))]
public class EnemyAnimationInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(20);
        GUILayout.Label("Debug", EditorStyles.boldLabel);
        if (GUILayout.Button("Go to Idle"))
        {
            
        }
        if (GUILayout.Button("Go to walk"))
        {
            
        }
        if (GUILayout.Button("Make Shoot"))
        {
            
        }
    }
}

#endif
