using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(ImpactBulletSO))]
public class ImpactBulletVFXEditor : Editor
{
    SerializedProperty vfxList;

    void OnEnable()
    {
        vfxList = serializedObject.FindProperty("vfxList");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(vfxList, true);

        CheckForDuplicates();

        serializedObject.ApplyModifiedProperties();
    }

    void CheckForDuplicates()
    {
        ImpactBulletSO data = (ImpactBulletSO)target;

        HashSet<SurfaceType> seen = new HashSet<SurfaceType>();

        foreach (var vfx in data.vfxList)
        {
            if (!seen.Add(vfx.surfaceType))
            {
                EditorGUILayout.HelpBox(
                    $"Duplicate SurfaceType detected: {vfx.surfaceType}",
                    MessageType.Warning
                );
                return;
            }
        }
    }
}