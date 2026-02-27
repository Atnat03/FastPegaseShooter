using System;
using UnityEngine;

[ExecuteAlways]
public class FloorDetection : MonoBehaviour
{
    [SerializeField] private Vector3 detectionExtent;
    [SerializeField] private float detectionPrecision = 1f;
    private void Update()
    {
        GetRaycastPositions();
    }

    Vector3[] GetRaycastPositions()
    {
        int xRaycastAmount = (int)(detectionExtent.x / detectionPrecision);
        int zRaycastAmount = (int)(detectionExtent.z / detectionPrecision);
        Vector3 positionOffset = transform.position + new Vector3(-detectionExtent.x*0.5f+detectionPrecision, detectionExtent.y*0.5f, -detectionExtent.z*0.5f+detectionPrecision);
        Vector3[] raycastPositions = new Vector3[xRaycastAmount * zRaycastAmount];
        for (int i = 0; i < xRaycastAmount; i++)
        {
            for (int j = 0; j < zRaycastAmount; j++)
            {
                raycastPositions[i * xRaycastAmount + j] = positionOffset + new Vector3(i * detectionPrecision, 0, j * detectionPrecision);
            }
        }
        
        //Debug.Log(raycastPositions.Length);
        
        return raycastPositions;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, detectionExtent);
        
        Vector3[] raycastPositions = GetRaycastPositions();
        Gizmos.color = Color.red;
        foreach (Vector3 position in raycastPositions)
        {
            Gizmos.DrawLine(position, Vector3.down * detectionExtent.y + position);
        }
    }
}
