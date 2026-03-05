using System;
using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
    }
}
