using UnityEngine;

public class Boue : TerrainType
{
    public override int ApplyCost(int value, PlayerType player = PlayerType.Human)
    {
        int newValue = Mathf.RoundToInt(value * costMultiplicator);
        
        if(player == PlayerType.FisherMan)
            newValue = Mathf.RoundToInt(newValue * costMultiplicator * 2f);
        
        return newValue;    
    }
}
