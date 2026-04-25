using CustomConsole.Runtime.Logger;
using UnityEngine;

public static class MathUtilities
{
    public static int SolveQuadratic(float a, float b, float c, out float root1, out float root2)
    {
        float discriminant = b * b - 4 * a * c;
        if (discriminant < 0)
        {
            root1 = Mathf.Infinity;
            root2 = -root1;
            return 0;
        }
        root1 =  (-b + Mathf.Sqrt(discriminant)) / (2 * a);
        root2 =  (-b - Mathf.Sqrt(discriminant)) / (2 * a);
        return discriminant > 0 ? 2 : 1;
    }
}
