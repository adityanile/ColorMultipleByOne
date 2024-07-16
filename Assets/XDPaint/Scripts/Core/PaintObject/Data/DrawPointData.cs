using UnityEngine;
using XDPaint.Scripts.Core.PaintObject.Base;

namespace XDPaint.Scripts.Core.PaintObject.Data
{
    public class DrawPointData : BasePointerData
    {
        public Vector2 TexturePosition;
        public float Pressure;

        public DrawPointData(Vector2 texturePosition, float pressure, int fingerId = 0) : base(fingerId)
        {
            TexturePosition = texturePosition;
            Pressure = pressure;
        }
    }
}