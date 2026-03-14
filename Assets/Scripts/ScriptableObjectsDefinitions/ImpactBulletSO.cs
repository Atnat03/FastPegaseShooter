using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum SurfaceType
{
    None, Metal, Enemy
}

[CreateAssetMenu(menuName = "ImpactBulletSO")]
public class ImpactBulletSO : ScriptableObject
{
    public List<VFXSurface> vfxList = new List<VFXSurface>();

    public GameObject GetVFXFromSurface(SurfaceType surfaceType)
    {
        return vfxList.Find(x => x.surfaceType == surfaceType).VFX;
    }
}

[Serializable]
public class VFXSurface
{
    public SurfaceType surfaceType;
    public GameObject VFX;
}


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

public static class ImpactSurface
{
    public static SurfaceType GetSurfaceType(string tag)
    {
        Debug.Log(tag);
        switch (tag)
        {
            case "Enemy": return SurfaceType.Enemy;
            default: return SurfaceType.Metal;
        }
    }
}
