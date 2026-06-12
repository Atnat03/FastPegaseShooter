using UnityEngine;

public class DesactivateOnElevatorStop : MonoBusListener
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ListenToEvent<OnDapEvent>(
            ODE => gameObject.SetActive(false)
            );
    }
}
