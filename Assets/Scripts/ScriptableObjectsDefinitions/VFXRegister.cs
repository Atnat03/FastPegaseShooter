using UnityEngine;

[CreateAssetMenu(fileName = "VFXRegistry", menuName = "GunDecorator/VFXRegistry")]
public class VFXRegistry : ScriptableObject
{
    [SerializeField] private GameObject[] _vfxList;
    
    public GameObject Get(int index)
    {
        if (index < 0 || index >= _vfxList.Length) return null;
        return _vfxList[index];
    }
}