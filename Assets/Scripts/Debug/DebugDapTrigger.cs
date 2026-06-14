using Unity.VisualScripting;
using UnityEngine;

public class DebugDapTrigger : MonoBusListener
{
    void OnTriggerEnter(Collider other)
    {
        InvokeEvent(new OnDapEvent());
        InvokeEvent(new OnDappEventObserveurs());
    }
}