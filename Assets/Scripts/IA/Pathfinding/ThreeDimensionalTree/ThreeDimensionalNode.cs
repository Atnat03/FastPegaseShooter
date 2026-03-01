
using System.Collections.Generic;
using UnityEngine;

public class ThreeDimensionalNode
{
    public Vector3 value;
    public int deepth;

    public ThreeDimensionalNode parent;
    
    public ThreeDimensionalNode left;
    public ThreeDimensionalNode right;

    public ThreeDimensionalNode(ThreeDimensionalNode parent, Vector3 value, int deepth)
    {
        this.parent = parent;
        this.value = value;
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

    public float Compare(Vector3 otherValue) => otherValue[deepth % 3] - value[deepth % 3];

    public bool IsSimilar(Vector3 otherValue, float threshold) => Vector3.Distance(otherValue, value) <= threshold;
}
