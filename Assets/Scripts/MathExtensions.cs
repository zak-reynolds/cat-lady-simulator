using UnityEngine;

namespace MathExtensions
{
    public static class VectorMath
    {
        public static Vector3 Flatten(this Vector3 vector)
        {
            return new Vector3(vector.x, 0, vector.z);
        }
        
        public static Vector2Int Invert(this Vector2Int v)
        {
            return new Vector2Int(v.y, v.x);
        }
    }
}
