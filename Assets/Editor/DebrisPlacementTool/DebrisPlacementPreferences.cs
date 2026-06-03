using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DebrisPlacementPreferences : ISavable<DebrisPlacementPreferences>
{
    public bool p_isPrefabListOpened = true;

    public Vector3 p_maxRotation = Vector3.up * 180;
    public float p_minScale = 1f;
    public float p_maxScale = 1.5f;
    public float p_placementRadius = 5;
    public int p_minAmount = 1,  p_maxAmount = 3;
    public List<SavableDebrisClass> p_debrisList = new();

    
    public DebrisPlacementPreferences(){}
    public DebrisPlacementPreferences(List<DebrisClass> list, bool opened, 
        Vector3 rot, float pMinS, float  pMaxS, float radius, int pMinA, int pMaxA)
    {
        p_isPrefabListOpened = opened;
        p_maxRotation = rot;
        p_minScale = pMinS;
        p_maxScale = pMaxS;
        p_placementRadius = radius;
        p_minAmount = pMinA;
        p_maxAmount = pMaxA;

        p_debrisList = new List<SavableDebrisClass>();
        foreach (DebrisClass debris in list)
        {
            p_debrisList.Add(new SavableDebrisClass
            {
                p_prefabPathGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(debris.p_debrisPrefab)),
                p_objectWeight = debris.p_ObjectWeight
            });
        }
    }
    
    public DebrisPlacementPreferences GetFromJSon()
    {
        return SaveManager.Load<DebrisPlacementPreferences>();
    }

    public void SaveToJson()
    {
        SaveManager.Save(this);
    }
}
[System.Serializable]
public class SavableDebrisClass
{
    public string p_prefabPathGuid;
    public float p_objectWeight;
}
