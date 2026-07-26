using UnityEngine;
using UnityEngine.Events;

public class TriggerZone : MonoBehaviour
{
    [SerializeField] private UnityEvent _events;
    [SerializeField] private bool _activateOnce = true;

    private bool _activated;
    public void OnTriggerEnter(Collider other)
    {
        if(_activateOnce && _activated) return;
        
        if (other.CompareTag("Player"))
        {
            _activated = true;
            _events?.Invoke();
        }
    }
}
