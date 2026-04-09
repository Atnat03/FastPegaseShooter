using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyLifeModule), true)]
public class EnemyLifeModuleInspector : EnemyBehaviourModuleInspector
{
    private SerializedProperty _life;

    private SerializedProperty _enemyLifeModule;
    
    
    protected override void OnEnable()
    {
        base.OnEnable();
        
        _life = serializedObject.FindProperty("_life");
        _enemyLifeModule = serializedObject.FindProperty("_enemyLifeModule");
    }

    protected override void DrawCustomFields()
    {
        base.DrawCustomFields();
        
        ShowModuleTitle(EditorUtilities.SplitPascalCase(target.GetType().Name),
            new Color(0.71f,0.81f, 0.27f)*0.8f,
            new DashedBorderParameters(4,
                type: dashType.Interior,
                sides: DashedBorderParameters.ALL_SIDES,
                segmentLenght: 15,
                gapLenght: 7));

        EditorGUILayout.PropertyField(_life);
        if (target.GetType() == typeof(WeakPointLifeModule))
        {
            Rect targetBlockRect = EditorUtilities.WrapInBlock(() =>
                {
                    GUILayout.BeginVertical();
                    GUILayout.Label("Life Module", _titleStyle);
                    EditorGUILayout.PropertyField(_enemyLifeModule);
                    GUILayout.EndVertical();
                },
                hPadding: 15,
                backgroundColor: _enemyLifeModule.objectReferenceValue == null ? Color.crimson : new Color());
            EditorUtilities.DrawDashedBorders(
                targetBlockRect,
                new DashedBorderParameters(4,
                    new Color(0.71f,0.81f, 0.27f),
                    0b1000,
                    dashType.Interior,
                    10, 5));
        }
    }

    protected override void SetIcon()
    {
        Texture2D moduleIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Icons/AI/Life.png");
        if (moduleIcon != null)
        {
            EditorGUIUtility.SetIconForObject(target, moduleIcon);
        }
    }
}
