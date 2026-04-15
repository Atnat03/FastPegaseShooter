using FishNet;
using Unity.Multiplayer.PlayMode;
using UnityEngine;

public class AutoJoin : MonoBehaviour
{
    private void Start()
    {
        if (CurrentPlayer.IsMainEditor)
        {
            InstanceFinder.NetworkManager.ServerManager.StartConnection();
        }
        else
        {
            InstanceFinder.NetworkManager.ClientManager.StartConnection();
        }
    }
}
