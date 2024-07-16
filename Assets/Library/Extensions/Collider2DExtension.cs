// ReSharper disable All
using UnityEngine;

namespace Library.Extensions
{
    public static class Collider2DExtension
    {
        public static void CopyFrom(this Collider2D destCollider, Collider2D sourceCollider) {
            switch (sourceCollider) {
                case BoxCollider2D collider2D:
                    ((BoxCollider2D)destCollider).size = collider2D.size;
                    break;
                case PolygonCollider2D collider2D:
                    ((PolygonCollider2D)destCollider).points = collider2D.points;
                    break;
                case CircleCollider2D collider2D:
                    ((CircleCollider2D)destCollider).radius = collider2D.radius;
                    break;
            }

            destCollider.offset = sourceCollider.offset;
        }

        public static bool IsOverlapping(this Collider2D thisCollider, Collider2D otherCollider)
        {
            var b1 = thisCollider.bounds;
            var b2 = otherCollider.bounds;
            
            b1.SetMinMax(b1.min.SetZ(-999), b1.max.SetZ(999));

            return b1.Intersects(b2);
        }
    }
}
