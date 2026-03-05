using System;
using UnityEngine;
using UnityEngine.Events;

public class TriggerZone : MonoBehaviour
{
    [SerializeField] private UnityEvent _events;
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _events?.Invoke();
        }
    }
}
