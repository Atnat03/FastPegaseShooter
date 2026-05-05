using CustomConsole.Runtime.Logger;
using UnityEngine;

public static class Extensions
{
    public static Transform GetRootTransform(this Transform t)
    {
        if (t.parent == null) return t;
        return GetRootTransform(t.parent);
    }

    public static Vector3 RemoveY(this Vector3 v)
    {
        v.y = 0;
        return v;
    }
}
