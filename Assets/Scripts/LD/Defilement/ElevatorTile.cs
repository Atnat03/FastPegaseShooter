using System;
using TMPro;
using UnityEngine;

public class ElevatorTile : MonoBehaviour
{
    [SerializeField] private TextMeshPro TMP;

    private int _currentFloor;

    public void ChangeFloor(int floorIncrease)
    {
        if(TMP) TMP.text = $"GR-{(_currentFloor + floorIncrease):000}";
        _currentFloor+=floorIncrease;
    }
}
