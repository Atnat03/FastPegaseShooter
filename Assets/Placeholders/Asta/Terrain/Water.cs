using UnityEngine;

public class Water : TerrainType
{
    public override int ApplyCost(int value, PlayerType player = PlayerType.Human)
    {
        int newValue = Mathf.RoundToInt(value * costMultiplicator);
        
        if(player == PlayerType.FisherMan)
            newValue = Mathf.RoundToInt(newValue * costMultiplicator * 0.3f);
        
        return newValue;
    }
}
