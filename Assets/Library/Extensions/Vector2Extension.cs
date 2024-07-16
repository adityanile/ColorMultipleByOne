// ReSharper disable All

using UnityEngine;

namespace Library.Extensions{
    public static class Vector2Extension{
        public static Vector2 SetX(this Vector2 vector2, float newX) {
            vector2.x = newX;
            return vector2;
        }

        public static Vector2 SetY(this Vector2 vector2, float newY) {
            vector2.y = newY;
            return vector2;
        }

        public static Vector2 SetXY(this Vector2 vector2, float newX, float newY) {
            vector2.x = newX;
            vector2.y = newY;
            return vector2;
        }

        private static Vector2 _tmp;

        public static Vector2 Lerp(Vector2 start, Vector2 end, float alpha, EasingFunction.Function easingFunc) {
            alpha = Mathf.Clamp01(alpha);

            _tmp.x = easingFunc(start.x, end.x, alpha);
            _tmp.y = easingFunc(start.y, end.y, alpha);

            return _tmp;
        }

        public static Vector2 Rotate(this Vector2 vector2, Vector2 origin, float degrees) {
            var sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            var cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            var tx = vector2.x - origin.x;
            var ty = vector2.y - origin.y;
            vector2.x = (cos * tx) - (sin * ty);
            vector2.y = (sin * tx) + (cos * ty);
            return vector2 + origin;
        }
    }
}