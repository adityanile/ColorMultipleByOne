using System;
using XDPaint.Core.PaintObject.Base;
using XDPaint.Tools.Image.Base;

namespace XDPaint.Tools.Image
{
    [Serializable]
    public class BucketToolSettings : BasePatternPaintToolSettings
    {
        [PaintToolSettings, PaintToolRange(0.0001f, 1f)] public float Tolerance { get; set; } = 0.01f;
        
        public BucketToolSettings(IPaintData paintData) : base(paintData)
        {
        }
    }
}
