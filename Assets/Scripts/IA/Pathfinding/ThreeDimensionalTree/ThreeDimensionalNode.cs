
using System.Collections.Generic;
using UnityEngine;

public class ThreeDimensionalNode
{
    public PathfindingNode node;
    public int deepth;

    public ThreeDimensionalNode parent;
    
    public ThreeDimensionalNode left;
    public ThreeDimensionalNode right;

    public ThreeDimensionalNode(ThreeDimensionalNode parent, PathfindingNode node, int deepth)
    {
        this.parent = parent;
        this.node = node;
        this.deepth = deepth;
    }
    
    public ThreeDimensionalNode Minimum()
    {
        if(left != null) return left.Minimum();
        return this;
    }
    public ThreeDimensionalNode Maximum()
    {
        if(right != null) return right.Maximum();
        return this;
    }

    public float Compare(Vector3 otherValue) => otherValue[deepth % 3] - node.position[deepth % 3];

    public bool IsSimilar(Vector3 otherValue, float threshold) => Vector3.Distance(otherValue, node.position) <= threshold;
}
