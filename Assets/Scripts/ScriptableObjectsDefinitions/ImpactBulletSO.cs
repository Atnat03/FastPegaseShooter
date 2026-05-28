using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum SurfaceType
{
    None, Metal, Enemy
}

[CreateAssetMenu(menuName = "Scriptable Objects/ImpactBulletSO")]
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


public static class ImpactSurface
{
    public static SurfaceType GetSurfaceType(string tag)
    {
        switch (tag)
        {
            case "Enemy": return SurfaceType.Enemy;
            default: return SurfaceType.Metal;
        }
    }
}
