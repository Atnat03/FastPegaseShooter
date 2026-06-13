using System;
using FishNet.Object;
using UnityEngine;

public class TriggerAscenceur : NetworkBusListener
{
    #region Variables
    
    [SerializeField] Animator[] _animatorToTrigger;
    private bool _triggered = false;

    #endregion

    #region Fonctions

    private void Awake()
    {
        TriggerAnimators(false);
    }

    [ContextMenu("TriggerAscenseur")]
    void Trigger()
    {
        if (!IsServerInitialized) return;
        if (_triggered) return;
        _triggered = true;
        
        TriggerAscenceurObserversRpc();
        enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServerInitialized) return;
        if (_triggered) return;
        
        if (other.TryGetComponent(out PlayerVisuelBridge player))
        {
            _triggered = true;
            TriggerAscenceurObserversRpc();
            enabled = false;
        }
    }

    [ObserversRpc]
    private void TriggerAscenceurObserversRpc()
    {
        TriggerAnimators(true);
        InvokeEvent(new OnAscenseurStart());
        enabled = false;
    }

    private void TriggerAnimators(bool state)
    {
        foreach (Animator animator in _animatorToTrigger)
        {
            animator.enabled = state;
        }
    }

    #endregion
}