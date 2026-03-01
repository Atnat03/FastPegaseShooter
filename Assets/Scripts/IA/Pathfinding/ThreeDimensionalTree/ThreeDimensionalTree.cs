using System;
using System.Collections.Generic;
using UnityEngine;

public class ThreeDimensionalTree
{
    ThreeDimensionalNode root;

    public void Populate(List<Vector3> values)
    {
        root = Build(values, 0, null);
    }
    ThreeDimensionalNode Build(List<Vector3> points, int depth, ThreeDimensionalNode parent)
    {
        if (points == null || points.Count == 0)
            return null;

        int axis = depth % 3;
        
        points.Sort((a, b) => a[axis].CompareTo(b[axis]));
        int medianIndex = points.Count / 2;
        ThreeDimensionalNode node = new ThreeDimensionalNode(parent, points[medianIndex], depth);

        List<Vector3> leftPoints = points.GetRange(0, medianIndex);
        List<Vector3> rightPoints = points.GetRange(medianIndex + 1, points.Count - medianIndex - 1);

        node.left = Build(leftPoints, depth + 1, node);
        node.right = Build(rightPoints, depth + 1, node);

        return node;
    }

    public bool Add(Vector3 value)
    {
        if(root == null)
        {
            root = new ThreeDimensionalNode(null, value, 0);
            return true;
        }
        
        return Add(value, root);
    }
    bool Add(Vector3 value, ThreeDimensionalNode n)
    {
        float comparisonResult = n.Compare(value);
        if(Mathf.Abs(comparisonResult) < 1e-6f) return false;//Value already in the tree

        if (comparisonResult < 0)
        {
            if(n.left == null) n.left = new ThreeDimensionalNode(n,value, n.deepth+1);
            else Add(value, n.left);
        }
        else
        {
            if(n.right == null) n.right = new ThreeDimensionalNode(n,value, n.deepth+1);
            else Add(value, n.right);
        }
        return true;
    }

    public ThreeDimensionalNode Search(Vector3 value) => Search(value, root);
    ThreeDimensionalNode Search(Vector3 value, ThreeDimensionalNode n)
    {
        if(n == null) return null;
        
        float comparisonResult = n.Compare(value);
        if(Mathf.Abs(comparisonResult) < 1e-6f) return n;

        if (comparisonResult < 0) return Search(value, n.left);
        return Search(value, n.right);
    }

    public bool ExistSimilar(Vector3 value, float threshold) => ExistSimilar(value, threshold, root);
    
    bool ExistSimilar(Vector3 value, float threshold, ThreeDimensionalNode n)
    {
        if(n == null) return false;
        
        if(n.IsSimilar(value, threshold)) return true;
        
        float comparisonResult = n.Compare(value);
        
        ThreeDimensionalNode primaryBranch = comparisonResult < 0 ? n.left : n.right;
        ThreeDimensionalNode secondaryBranch = comparisonResult < 0 ? n.right : n.left;
        
        if(ExistSimilar(value, threshold, primaryBranch)) return true;
        
        float axisDiff = Mathf.Abs(value[n.deepth%3] - n.value[n.deepth%3]);
        
        if(axisDiff <= threshold)
            if(ExistSimilar(value, threshold, secondaryBranch)) return true;
        
        return false;
    }

    public ThreeDimensionalNode FindClosest(Vector3 value)
    {
        return FindClosest(value, root);
    }
    public ThreeDimensionalNode FindClosest(Vector3 value, ThreeDimensionalNode n)
    {
        if(n == null) return null;

        ThreeDimensionalNode bestNode = n;
        float bestDistance = (value - n.value).sqrMagnitude;

        int axis = n.deepth % 3;

        ThreeDimensionalNode primary = value[axis] < n.value[axis] ? n.left : n.right;
        ThreeDimensionalNode secondary = value[axis] < n.value[axis] ? n.right : n.left;

        // 1. explorer primaire
        if(primary != null)
        {
            ThreeDimensionalNode candidate = FindClosest(value, primary);
            float dist = (value - candidate.value).sqrMagnitude;

            if(dist < bestDistance)
            {
                bestDistance = dist;
                bestNode = candidate;
            }
        }

        // 2. tester si on doit explorer secondaire
        float axisDiff = value[axis] - n.value[axis];
        if(axisDiff * axisDiff < bestDistance)
        {
            if(secondary != null)
            {
                ThreeDimensionalNode candidate = FindClosest(value, secondary);
                float dist = (value - candidate.value).sqrMagnitude;

                if(dist < bestDistance)
                {
                    bestDistance = dist;
                    bestNode = candidate;
                }
            }
        }

        return bestNode;
    }

    #region Deleting Entry (Unused for now)
    /*public ThreeDimensionalNode Minimum()
    {
        if(root == null) return null;
        return root.Minimum();
    }
    public ThreeDimensionalNode Maximum()
    {
        if(root == null) return null;
        return root.Maximum();
    }
    
    public void Delete(Vector3 value)
    {
        if(root == null) return;
        ThreeDimensionalNode node = Search(value);
        
        if(node == null) return;

        int childAmount = 0;
        childAmount += node.left == null ? 0 : 1;
        childAmount += node.right == null ? 0 : 1;
        
        if (childAmount == 0)
            if (node.parent == null) root = null;
            else if(node.parent.left == node) node.parent.left = null;
            else node.parent.right = null;
        
        else if(childAmount == 1) DeleteWithSingleChild(node);
        else DeleteWithTwoChildren(node);
    }

    void DeleteWithSingleChild(ThreeDimensionalNode node)
    {
        if(node.left != null && node.right == null)
            if(node.parent == null) root = node.left;
            else if(node.parent.left == node) node.parent.left = node.left;
            else node.parent.right = node.left;
        
        else if (node.right != null && node.left == null)
            if (node.parent == null) root = node.right;
            else if (node.parent.right == node) node.parent.right = node.right;
            else node.parent.left = node.right;

        else throw new Exception("error, DeleteSingleChild called with two children node");
    }

    ///
    /// Carefull, problem with deleting with two children, the minimum isn't always on left !
    ///
    void DeleteWithTwoChildren(ThreeDimensionalNode node)
    {
        ThreeDimensionalNode minimumOnRight = node.right.Minimum();

        if (node.right.Compare(minimumOnRight.value) == 0) node.right = null;
        else minimumOnRight.parent.left = null;
        
        node.value = minimumOnRight.value;
    }*/
    #endregion

    public void ClearTree()
    {
        root = null;
    }
}
