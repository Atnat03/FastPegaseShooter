using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyTargetModule), true)]
public class EnemyTargetModuleInspector : EnemyBehaviourModuleInspector
{
    protected override void DrawCustomFields()
    {
        base.DrawCustomFields();
        ShowModuleTitle(EditorUtilities.SplitPascalCase(target.GetType().Name),
            new Color(0.99f,0.75f, 0.01f)*0.9f,
            new DashedBorderParameters(4,
                type: dashType.Interior,
                sides: DashedBorderParameters.ALL_SIDES,
                segmentLenght: 15,
                gapLenght: 7));
    }

    protected override void SetIcon()
    {
        Texture2D moduleIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Icons/AI/Target.png");
        if (moduleIcon != null)
        {
            EditorGUIUtility.SetIconForObject(target, moduleIcon);
        }
    }
}
