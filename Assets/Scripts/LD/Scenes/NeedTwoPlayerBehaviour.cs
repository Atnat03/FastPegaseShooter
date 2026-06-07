using System.Collections.Generic;
using LD.Scenes;
using UnityEngine;

public class NeedTwoPlayerBehaviour : MonoBehaviour
{
    [SerializeField] bool need2Players = false;
    private int playerCount = 0;
    private List<PlayerVisuelBridge> alreadyCountedPlayers = new List<PlayerVisuelBridge>();
    
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerVisuelBridge player))
        {
            if (!alreadyCountedPlayers.Contains(player))
            {
                playerCount++;
                alreadyCountedPlayers.Add(player);
                if (playerCount >= 2 || !need2Players)
                {
                    OnTwoPlayerFunction();
                }
            }
        }
    }

    protected virtual void OnTwoPlayerFunction() => Debug.Log("OnTwoPlayerFunction");
}
