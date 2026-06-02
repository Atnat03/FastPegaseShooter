using UnityEngine;

[CreateAssetMenu(fileName = "MovementModuleSO", menuName = "Scriptable Objects/AI/Entity/Movement/MovementModuleSO")]
public class MovementModuleSO : ScriptableObject
{
    public bool p_doFreezeWithoutTarget = true;
    public float p_speed = 3;
}
