using System;
using FishNet;
using UnityEngine;

public class PlayerSeparator : MonoBehaviour
{
    [SerializeField] private float _expulsionForce;
    
    [SerializeField] private Vector3 _expulsionDirectionRed;
    [SerializeField] private Vector3 _expulsionDirectionBlue;
    public void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerVisuelBridge bridge = other.GetComponent<PlayerVisuelBridge>();
            
            if (bridge.FPSController.OwnerId == 0)
            {
                bridge.FPSController.Rb.AddForce(_expulsionDirectionRed.normalized * _expulsionForce, ForceMode.Acceleration);
            }
            else
            {
                bridge.FPSController.Rb.AddForce(_expulsionDirectionBlue.normalized * _expulsionForce, ForceMode.Acceleration);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.crimson;
        Gizmos.DrawLine(transform.position, transform.position + _expulsionDirectionRed.normalized);
        Gizmos.DrawSphere(transform.position +  _expulsionDirectionRed.normalized, 0.2f);
        Gizmos.color = Color.cornflowerBlue;
        Gizmos.DrawLine(transform.position, transform.position + _expulsionDirectionBlue.normalized);
        Gizmos.DrawSphere(transform.position +  _expulsionDirectionBlue.normalized, 0.2f);
    }
}
