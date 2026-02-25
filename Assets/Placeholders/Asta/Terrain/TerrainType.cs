using UnityEngine;

public class TerrainType : MonoBehaviour
{
    public float costMultiplicator;

    public virtual int ApplyCost(int value, PlayerType player = PlayerType.Human)
    {
        int newValue = Mathf.RoundToInt(value * costMultiplicator);
        
        return newValue;
    }
}
