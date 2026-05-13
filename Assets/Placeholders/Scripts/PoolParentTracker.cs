using UnityEngine;

public class PoolParentTracker : MonoBehaviour
{
    public string poolName;
    
    void OnDestroy()
    {
        Debug.LogError($"POOL PARENT DÉTRUIT: {poolName}\n{System.Environment.StackTrace}");
    }
}
