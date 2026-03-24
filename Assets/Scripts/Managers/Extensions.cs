using UnityEngine;

public static class Extensions
{
    public static Transform GetRootTransform(this Transform t)
    {
        if (t.parent == null) return t;
        return GetRootTransform(t.parent);
    }
}
