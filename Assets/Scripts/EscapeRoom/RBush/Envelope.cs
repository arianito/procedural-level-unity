using UnityEngine;

namespace RBush
{
    public class Envelope
    {
        public float MinX;
        public float MinY;
        public float MaxX;
        public float MaxY;

        public Envelope(float minX, float minY, float maxX, float maxY)
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

        public Envelope Extend(in Envelope other) =>
            new Envelope(
                Mathf.Min(MinX, other.MinX),
                Mathf.Min(MinY, other.MinY),
                Mathf.Max(MaxX, other.MaxX),
                Mathf.Max(MaxY, other.MaxY));

        public Envelope Intersection(in Envelope other) =>
            new Envelope(
                Mathf.Max(MinX, other.MinX),
                Mathf.Max(MinY, other.MinY),
                Mathf.Min(MaxX, other.MaxX),
                Mathf.Min(MaxY, other.MaxY));

        public bool Contains(in Envelope other) =>
            MinX <= other.MinX &&
            MinY <= other.MinY &&
            MaxX >= other.MaxX &&
            MaxY >= other.MaxY;

        public bool Intersects(in Envelope other) =>
            MinX <= other.MaxX &&
            MinY <= other.MaxY &&
            MaxX >= other.MinX &&
            MaxY >= other.MinY;

        public static Envelope InfiniteBounds =>
            new Envelope(
                float.NegativeInfinity,
                float.NegativeInfinity,
                float.PositiveInfinity,
                float.PositiveInfinity);

        public static Envelope EmptyBounds =>
            new Envelope(
                float.PositiveInfinity,
                float.PositiveInfinity,
                float.NegativeInfinity,
                float.NegativeInfinity);
    }
}