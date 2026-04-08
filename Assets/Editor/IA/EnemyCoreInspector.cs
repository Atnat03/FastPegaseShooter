using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyCore))]
public class EnemyCoreInspector : Editor
{
    private SerializedProperty _maxEnemySwelling;
    private SerializedProperty _attackingModules;
    private SerializedProperty _lifeModules;
    private SerializedProperty _targetModules;
    private SerializedProperty _movementModule;
    
    private SerializedProperty scriptProperty;

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
        _targetModules = serializedObject.FindProperty("_targetingModules");
        _movementModule = serializedObject.FindProperty("_movementModule");

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
            p_removeColor = new Color(0.85f, 0.25f, 0.2f),
            p_addColor = new Color(0.52f, 0.82f, 0.96f),
            p_verticalMargin = 5
        };
        
        scriptProperty = serializedObject.FindProperty("m_Script");
        
        Texture2D moduleIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Icons/AI/Core.png");
        if (moduleIcon != null)
        {
            EditorGUIUtility.SetIconForObject(target, moduleIcon);
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(scriptProperty);
        EditorGUI.EndDisabledGroup();
        
        GUILayout.Space(10);
        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(60));
        EditorGUI.DrawRect(rect, new Color(0.93f, 0.43f, 0.28f)*0.8f);
        GUI.Label(rect, "Enemy Core", _centeredTitleStyle);
        GUILayout.Space(10);

        EditorGUILayout.PropertyField(_maxEnemySwelling);
        EditorUtilities.DrawList(_attackingModules, "Attacking Modules", _listStyle, ref _attackListOpened);
        EditorUtilities.DrawList(_lifeModules, "Life Modules", _listStyle, ref _lifeListOpened);
        EditorUtilities.DrawList(_targetModules, "Target Modules", _listStyle, ref _lifeListOpened);
        
        GUILayout.BeginVertical("box");
        GUILayout.Label("Moving Module", _titleStyle);
        EditorGUILayout.PropertyField(_movementModule);
        GUILayout.EndVertical();
        
        EditorUtilities.Draw("_explosionChargedDamage", serializedObject);
        EditorUtilities.Draw("_negativeChargeMax", serializedObject);
        EditorUtilities.Draw("_positiveChargeMax", serializedObject);
        EditorUtilities.Draw("_timeBeforeStatReset", serializedObject);
        EditorUtilities.Draw("_positiveUI", serializedObject);
        EditorUtilities.Draw("_positiveCurValue", serializedObject);
        EditorUtilities.Draw("_negativeUI", serializedObject);
        EditorUtilities.Draw("_negativeCurValue", serializedObject);
        EditorUtilities.Draw("_explosionParticle", serializedObject);
        
        serializedObject.ApplyModifiedProperties();
    }
}
