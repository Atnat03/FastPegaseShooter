using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyAttackModule), true)]
public class EnemyAttackModuleInspector : EnemyBehaviourModuleInspector
{
    private SerializedProperty _targetModule;
    private SerializedProperty _attackModuleSO;
    
    
    protected override void OnEnable()
    {
        base.OnEnable();
        
        _targetModule = serializedObject.FindProperty("_targetModule");
        _attackModuleSO = serializedObject.FindProperty("_attackModuleSO");
    }

    protected override void DrawCustomFields()
    {
        base.DrawCustomFields();
        
        ShowModuleTitle(EditorUtilities.SplitPascalCase(target.GetType().Name),
            new Color(0.81f,0.55f, 0.74f)*0.8f,
            new DashedBorderParameters(4,
                type: dashType.Interior,
                sides: DashedBorderParameters.ALL_SIDES,
                segmentLenght: 15,
                gapLenght: 7));

        EditorGUILayout.PropertyField(_attackModuleSO);
        
        Rect targetBlockRect = EditorUtilities.WrapInBlock(() =>
            {
                GUILayout.BeginVertical();
                GUILayout.Label("Target Module", _titleStyle);
                EditorGUILayout.PropertyField(_targetModule);
                GUILayout.EndVertical();
            },
            hPadding: 15,
            backgroundColor: _targetModule.objectReferenceValue == null ? Color.crimson : new Color());
        EditorUtilities.DrawDashedBorders(
            targetBlockRect,
            new DashedBorderParameters(4,
                new Color(0.99f,0.75f, 0.01f),
                0b1000,
                dashType.Interior,
                10, 5));
    }

    protected override void SetIcon()
    {
        Texture2D moduleIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Icons/AI/Attack.png");
        if (moduleIcon != null)
        {
            EditorGUIUtility.SetIconForObject(target, moduleIcon);
        }
    }
}
