using UnityEngine;

[CreateAssetMenu(fileName = "StepTargetModuleSO", menuName = "Scriptable Objects/AI/Entity/Target/StepTargetModuleSO")]
public class StepTargetModuleSO : ScriptableObject
{
    public float p_fakeTargetSwitchingDistance;
    
    [Tooltip("If this booléan is set to false, the enemy will only target te fake targets and never the player directly")]
    public bool p_doSwitchTargeting = false;
    [Tooltip("If one of the value is negative, the target module won't ever switch back to fake target")]
    public Vector2 p_rangeTimeForDirectTargeting = new Vector2(10, 30);
}
