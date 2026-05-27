using FishNet.Object;
using UnityEngine;

public class EnergyOrb : MonoBusListener
{
    #region Variables

    private float _currentEnergyOrb;
    private int _ownerId;

    #endregion

    #region Fonctions

    public void SetUpOrb(float value, int ownerId)
    {
        _currentEnergyOrb = value;
        _ownerId = ownerId;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out PlayerVisuelBridge player))
            return;

        NetworkObject playerNet = player.transform.root.GetComponent<NetworkObject>();

        if (playerNet == null)
            return;

        if (playerNet.OwnerId != _ownerId)
            return;

        InvokeEvent(new ModifyEnergyEvent
        {
            p_player = playerNet.OwnerId,
            p_value = _currentEnergyOrb
        });

        Destroy(gameObject);
    }

    #endregion
}