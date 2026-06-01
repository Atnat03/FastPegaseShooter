using Controller;
using FishNet.Object;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class PlayerSetup : NetworkBehaviour
{
    [SerializeField] private GunBridgePlayer _gunBridge;
    
    public override void OnStartClient()
    {
        if (!IsOwner) return;

        PlayerLocalData data = PlayerLocalData.Instance;
        int gunId = data != null ? data.LocalPlayerGunId : 0;

        SendGunDataServerRpc(gunId);
        
    }

    [ServerRpc]
    private void SendGunDataServerRpc(int gunId)
    {
        if (_gunBridge == null)
        {
            Debug.LogError("_gunBridge est null côté serveur !");
            return;
        }

        _gunBridge.InitializeWithGunId(gunId);
        InitGunObserversRpc(gunId);
    }

    [ObserversRpc(BufferLast = true)]
    private void InitGunObserversRpc(int gunId)
    {
        if (IsServerInitialized) return;

        if (_gunBridge == null)
            _gunBridge = GetComponent<GunBridgePlayer>();
            
        if (_gunBridge != null)
            _gunBridge.InitializeWithGunId(gunId);
    }
}