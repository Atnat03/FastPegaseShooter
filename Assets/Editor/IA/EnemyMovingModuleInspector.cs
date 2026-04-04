using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyMovingModule), true)]
public class EnemyMovingModuleInspector : EnemyBehaviourModuleInspector
{
    private SerializedProperty _doFreezeWithoutTarget;
    private SerializedProperty _targetModule;
    private SerializedProperty _speed;
    
    
    protected override void OnEnable()
    {
        base.OnEnable();
        
        _doFreezeWithoutTarget = serializedObject.FindProperty("_doFreezeWithoutTarget");
        _targetModule = serializedObject.FindProperty("_targetModule");
        _speed = serializedObject.FindProperty("_speed");
    }

    protected override void DrawCustomFields()
    {
        base.DrawCustomFields();
        
        ShowModuleTitle(EditorUtilities.SplitPascalCase(target.GetType().Name),
            new Color(0.51f,0.81f, 0.94f)*0.8f,
            new DashedBorderParameters(4,
                type: dashType.Interior,
                sides: DashedBorderParameters.ALL_SIDES,
                segmentLenght: 15,
                gapLenght: 7));

        EditorGUILayout.PropertyField(_doFreezeWithoutTarget);
        EditorGUILayout.PropertyField(_speed);

        GUILayout.Space(10);
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
        Texture2D moduleIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Icons/AI/Moving.png");
        if (moduleIcon != null)
        {
            EditorGUIUtility.SetIconForObject(target, moduleIcon);
        }
    }
}
