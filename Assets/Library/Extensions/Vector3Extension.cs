// ReSharper disable All

using UnityEngine;

namespace Library.Extensions
{
    public static class Vector3Extension
    {
        public static Vector3 Clone(this Vector3 vector3)
        {
            return new Vector3(vector3.x, vector3.y, vector3.z);
        }

        public static Vector3 SetX(this Vector3 vector3, float newX)
        {
            vector3.x = newX;
            return vector3;
        }


        public static Vector3 SetY(this Vector3 vector3, float newY)
        {
            vector3.y = newY;
            return vector3;
        }

        public static Vector3 SetZ(this Vector3 vector3, float newZ)
        {
            vector3.z = newZ;
            return vector3;
        }

        public static Vector3 SetXYZ(this Vector3 vector3, float newX, float newY, float newZ)
        {
            vector3.x = newX;
            vector3.y = newY;
            vector3.z = newZ;
            return vector3;
        }

        private static Vector3 _tmp;

        public static Vector3 Lerp(Vector3 start, Vector3 end, float alpha, EasingFunction.Function easingFunc)
        {
            alpha = Mathf.Clamp01(alpha);

            _tmp.x = easingFunc(start.x, end.x, alpha);
            _tmp.y = easingFunc(start.y, end.y, alpha);
            _tmp.z = easingFunc(start.z, end.z, alpha);

            return _tmp;
        }

        public static Vector3 Rotate2D(this Vector3 vector3, Vector2 origin, float degrees)
        {
            return ((Vector2)vector3).Rotate(origin, degrees);
        }

        public static Vector3 Random(this Vector3 vector3, Vector3 min, Vector3 max)
        {
            return new Vector3(UnityEngine.Random.Range(min.x, max.x), UnityEngine.Random.Range(min.y, max.y),
                UnityEngine.Random.Range(min.z, max.z));
        }

        public static void MultiplyMagnitude(this ref Vector3 a, Vector3 b)
        {
            a.x *= b.x;
            a.y *= b.y;
            a.z *= b.z;
        }
    }
}