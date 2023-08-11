using UnityEngine;

namespace Dungeon
{
    public static class MathExt
    {
        public const float Tolerance = 0.001f;

        public static bool NearEqual(in this Vector3 a, in Vector3 b)
        {
            return (a - b).magnitude < Tolerance;
        }

        public static bool NearEqual(in this float a, in float b)
        {
            return Mathf.Abs(a - b) < Tolerance;
        }
    }
}