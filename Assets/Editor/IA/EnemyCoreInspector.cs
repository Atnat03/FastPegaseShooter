using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyCore))]
public class EnemyCoreInspector : Editor
{
    private SerializedProperty _maxEnemySwelling;
    private SerializedProperty _attackingModules;
    private SerializedProperty _lifeModules;
    private SerializedProperty _movingModule;

    private GUIStyle _centeredTitleStyle;
    private GUIStyle _titleStyle;
    private EditorListDrawerStyle _listStyle;

    private bool _attackListOpened = true;
    private bool _lifeListOpened = true;
    private void OnEnable()
    {
        _maxEnemySwelling = serializedObject.FindProperty("_maxEnemySwelling");
        _attackingModules = serializedObject.FindProperty("_attackingModules");
        _lifeModules = serializedObject.FindProperty("_lifeModules");
        _movingModule = serializedObject.FindProperty("_movingModule");

        _centeredTitleStyle = new GUIStyle
        {
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
        };
        _centeredTitleStyle.normal.textColor = Color.white;
        _titleStyle = new GUIStyle
        {
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Bold,
            fontSize = 18
        };
        _titleStyle.normal.textColor = Color.white;
        _listStyle = new EditorListDrawerStyle
        {
            p_titleStyle = _titleStyle,
            p_label = (i => { return $"Element {i}";}),
            p_removeColor = Color.crimson,
            p_addColor = Color.aquamarine,
            p_verticalMargin = 5
        };
        
        /*Texture2D moduleIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Icons/Energy.png");
        if (moduleIcon != null)
        {
            EditorGUIUtility.SetIconForObject(target, moduleIcon);
        }*/
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        //EnemyCore enemyCore = (EnemyCore)target;
        
        GUILayout.Space(10);
        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(60));
        EditorGUI.DrawRect(rect, Color.aquamarine*0.6f);
        GUI.Label(rect, "Enemy Core", _centeredTitleStyle);
        GUILayout.Space(10);

        EditorGUILayout.PropertyField(_maxEnemySwelling);
        EditorUtilities.DrawList(_attackingModules, "Attacking Modules", _listStyle, ref _attackListOpened);
        EditorUtilities.DrawList(_lifeModules, "Life Modules", _listStyle, ref _lifeListOpened);
        
        GUILayout.BeginVertical("box");
        GUILayout.Label("Moving Module", _titleStyle);
        EditorGUILayout.PropertyField(_movingModule);
        GUILayout.EndVertical();
        
        serializedObject.ApplyModifiedProperties();
    }
}
