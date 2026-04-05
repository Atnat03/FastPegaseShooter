using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyLifeModule), true)]
public class EnemyLifeModuleInspector : EnemyBehaviourModuleInspector
{
    private SerializedProperty _energyGainWhenTouch;
    private SerializedProperty _life;
    
    
    protected override void OnEnable()
    {
        base.OnEnable();
        
        _energyGainWhenTouch = serializedObject.FindProperty("_energyGainWhenTouch");
        _life = serializedObject.FindProperty("_life");
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

        EditorGUILayout.PropertyField(_energyGainWhenTouch);
        EditorGUILayout.PropertyField(_life);
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
