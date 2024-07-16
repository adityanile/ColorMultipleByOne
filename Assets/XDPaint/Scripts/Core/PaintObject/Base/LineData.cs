using System.Collections.Generic;
using UnityEngine;
using XDPaint.Tools.Raycast;

namespace XDPaint.Core.PaintObject.Base
{
    public class LineData
    {
        public readonly List<Triangle> Triangles = new List<Triangle>();
        public readonly List<Vector2> PaintPositions = new List<Vector2>();
        public readonly List<float> BrushSizes = new List<float>();
        
        public void AddBrush(float brushSize)
        {
            if (BrushSizes.Count > 1)
            {
                BrushSizes.RemoveAt(0);
            }
            BrushSizes.Add(brushSize);
        }

        public void AddPosition(Vector2 position)
        {
            if (PaintPositions.Count > 1)
            {
                PaintPositions.RemoveAt(0);
            }
            PaintPositions.Add(position);
        }

        public void AddTriangleBrush(Triangle triangle, float brushSize)
        {
            if (Triangles.Count > 1)
            {
                Triangles.RemoveAt(0);
            }
            Triangles.Add(triangle);
            if (BrushSizes.Count > 1)
            {
                BrushSizes.RemoveAt(0);
            }
            BrushSizes.Add(brushSize);
        }

        public float[] GetBrushes()
        {
            return BrushSizes.ToArray();
        }
        
        public Triangle[] GetTriangles()
        {
            return Triangles.ToArray();
        }
        
        public Vector2[] GetPositions()
        {
            return PaintPositions.ToArray();
        }

        public bool HasOnePosition()
        {
            return PaintPositions.Count == 1;
        }

        public bool HasNotSameTriangles()
        {
            return Triangles.Count == 2 && Triangles[0].Id != Triangles[1].Id;
        }

        public void Clear()
        {
            Triangles.Clear();
            PaintPositions.Clear();
            BrushSizes.Clear();
        }
    }
}