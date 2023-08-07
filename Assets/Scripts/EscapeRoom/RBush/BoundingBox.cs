using System;
using UnityEngine;

namespace RTree
{
    public class BoundingBox : ISpatialData
    {
        public readonly float MinX;
        public readonly float MinY;
        public readonly float MaxX;
        public readonly float MaxY;

        public BoundingBox(float minX, float minY, float maxX, float maxY)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
        }

        public float Area =>
            Mathf.Max(MaxX - MinX, 0) * Mathf.Max(MaxY - MinY, 0);

        public float Margin =>
            Mathf.Max(MaxX - MinX, 0) + Mathf.Max(MaxY - MinY, 0);

        public BoundingBox Extend(BoundingBox other) =>
            new BoundingBox(
                Mathf.Min(MinX, other.MinX),
                Mathf.Min(MinY, other.MinY),
                Mathf.Max(MaxX, other.MaxX),
                Mathf.Max(MaxY, other.MaxY));

        public BoundingBox Intersection(BoundingBox other) =>
            new BoundingBox(
                Mathf.Max(MinX, other.MinX),
                Mathf.Max(MinY, other.MinY),
                Mathf.Min(MaxX, other.MaxX),
                Mathf.Min(MaxY, other.MaxY));

        public bool Contains(BoundingBox other) =>
            MinX <= other.MinX &&
            MinY <= other.MinY &&
            MaxX >= other.MaxX &&
            MaxY >= other.MaxY;

        public bool Intersects(BoundingBox other) =>
            MinX <= other.MaxX &&
            MinY <= other.MaxY &&
            MaxX >= other.MinX &&
            MaxY >= other.MinY;

        public float DistanceTo(float x, float y)
        {
            var dX = AxisDistance(x, MinX, MaxX);
            var dY = AxisDistance(y, MinY, MaxY);
            return Mathf.Sqrt((dX * dX) + (dY * dY));
        }


        public override bool Equals(object obj)
        {
            if (!(obj is BoundingBox other)) return false;
            
            const float tolerance = RBush<ISpatialData>.Tolerance;
            
            return Math.Abs(MinX - other.MinX) < tolerance &&
                   Math.Abs(MinY - other.MinY) < tolerance &&
                   Math.Abs(MaxX - other.MaxX) < tolerance &&
                   Math.Abs(MaxY - other.MaxY) < tolerance;
        }

        public override int GetHashCode()
        {
            return MinX.GetHashCode() ^ MinY.GetHashCode() ^ MaxX.GetHashCode() ^ MaxY.GetHashCode();
        }

        public static BoundingBox EmptyBounds =>
            new BoundingBox(
                float.PositiveInfinity,
                float.PositiveInfinity,
                float.NegativeInfinity,
                float.NegativeInfinity);

        private static float AxisDistance(float p, float min, float max) =>
            p < min ? min - p :
            p > max ? p - max :
            0;

        
        public BoundingBox BBox => this;
    }
}