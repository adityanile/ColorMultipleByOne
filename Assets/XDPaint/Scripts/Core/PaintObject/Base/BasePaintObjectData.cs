using UnityEngine;
using XDPaint.Core.PaintObject.Base;

namespace XDPaint.Scripts.Core.PaintObject.Base
{
    public class BasePaintObjectData
    {
        private float pressure = 1f;
        public float Pressure
        {
            get => Mathf.Clamp(pressure, 0.01f, 10f);
            set => pressure = value;
        }
        
        public readonly LineData LineData;
        public Vector2 PreviousPaintPosition;
        public Vector3? LocalPosition { get; set; }
        public Vector2? PaintPosition { get; set; }
        public bool InBounds { get; set; }
        public bool IsPainting { get; set; }
        public bool IsPaintingDone { get; set; }

        public BasePaintObjectData()
        {
            LineData = new LineData();
        }
    }
}