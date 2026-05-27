using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VFXRegistry", menuName = "VFXRegistry")]
public class VFXRegistry : ScriptableObject
{
    public List<VFXData> _vfxList = new List<VFXData>();

    public VFXData CreateVFX(string name)
    {
        foreach (VFXData data in _vfxList)
        {
            if (data.p_vfxName == name)
            {
                return data;
            }
        }

        return null;
    }
}

[Serializable]
public class VFXData
{
    public string p_vfxName;
    public ParticleSystem p_particle;
    public Vector3 p_spawnPos;
    public float p_timeBeforeDestroy = 1;
}