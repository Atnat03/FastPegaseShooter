using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

//[CreateAssetMenu(fileName = "PathfindingGridSO", menuName = "Scriptable Objects/PathfindingGridSO")]
public class PathfindingGridSO : ScriptableObject
{
    [ReadOnly] public List<PathfindingNode> nodes = new List<PathfindingNode>();
}
