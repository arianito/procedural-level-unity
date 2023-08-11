// Credits: https://github.com/viceroypenguin/RBush

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon
{
    public class BoundingBox : ISpatialData
    {
        public Vector3 Min { get; }
        public Vector3 Max { get; }

        public BoundingBox(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }

        public float Width => Max.x - Min.x;
        public float Height => Max.y - Min.y;
        public float Depth => Max.z - Min.z;

        public Vector3 Center => (Min + Max) / 2.0f;

        public Vector3 Size => new Vector3(Width, Height, Depth);
        
        public Vector3Int SizeInt => new Vector3Int(
            Mathf.CeilToInt(Width),
            Mathf.CeilToInt(Height),
            Mathf.CeilToInt(Depth)
        );

        
        public float Area =>
            Mathf.Max(Max.x - Min.x, 0) * Mathf.Max(Max.y - Min.y, 0) * Mathf.Max(Max.z - Min.z, 0);

        public float Margin =>
            Mathf.Max(Max.x - Min.x, 0) + Mathf.Max(Max.y - Min.y, 0) + Mathf.Max(Max.z - Min.z, 0);

        public static BoundingBox EmptyBounds =>
            new BoundingBox(
                new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity),
                new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity)
            );

        public BoundingBox Expand(int offset) =>
            new BoundingBox(
                Min - Vector3.one * offset,
                Max + Vector3.one * offset
            );

        public static BoundingBox Empty => new BoundingBox(new Vector3(0, 0, 0), new Vector3(0, 0, 0));

        
        public bool IsPlane => Width.NearEqual(0) || Height.NearEqual(0) || Depth.NearEqual(0);


        public bool IsEdge => (Width.NearEqual(0) && Height.NearEqual(0)) ||
                              (Height.NearEqual(0) && Depth.NearEqual(0)) ||
                              (Depth.NearEqual(0) && Width.NearEqual(0));


        public bool IsPoint => Width.NearEqual(0) && Height.NearEqual(0) && Depth.NearEqual(0);



        public Vector3Int PlaneDirection
        {
            get
            {
                if (Width.NearEqual(0)) return Vector3Int.right;
                if (Height.NearEqual(0)) return Vector3Int.up;
                if (Depth.NearEqual(0)) return Vector3Int.forward;

                return Vector3Int.zero;
            }
        }


        public List<Vector3> Vertices
        {
            get
            {
                var a = new Vector3(Min.x, Min.y, Min.z);
                var b = new Vector3(Max.x, Min.y, Min.z);
                var c = new Vector3(Max.x, Min.y, Max.z);
                var d = new Vector3(Min.x, Min.y, Max.z);

                var a2 = new Vector3(Min.x, Max.y, Min.z);
                var b2 = new Vector3(Max.x, Max.y, Min.z);
                var c2 = new Vector3(Max.x, Max.y, Max.z);
                var d2 = new Vector3(Min.x, Max.y, Max.z);


                return new List<Vector3>
                {
                    a, b, c, d, a2, b2, c2, d2
                };
            }
        }

        public List<Edge> Edges
        {
            get
            {
                var a = new Vector3(Min.x, Min.y, Min.z);
                var b = new Vector3(Max.x, Min.y, Min.z);
                var c = new Vector3(Max.x, Min.y, Max.z);
                var d = new Vector3(Min.x, Min.y, Max.z);

                var a2 = new Vector3(Min.x, Max.y, Min.z);
                var b2 = new Vector3(Max.x, Max.y, Min.z);
                var c2 = new Vector3(Max.x, Max.y, Max.z);
                var d2 = new Vector3(Min.x, Max.y, Max.z);

                return new List<Edge>
                {
                    new Edge(a, b),
                    new Edge(b, c),
                    new Edge(c, d),
                    new Edge(d, a),

                    new Edge(a2, b2),
                    new Edge(b2, c2),
                    new Edge(c2, d2),
                    new Edge(d2, a2),

                    new Edge(a2, a),
                    new Edge(b2, b),
                    new Edge(c2, c),
                    new Edge(d2, d)
                };
            }
        }
        public BoundingBox ProjectOnXZ =>
            new BoundingBox(new Vector3(Min.x, 0, Min.z), new Vector3(Max.x, 0, Max.z));


        public BoundingBox BBox => this;

        public BoundingBox Extend(BoundingBox other)
        {
            return new BoundingBox(
                Vector3.Min(Min, other.Min),
                Vector3.Max(Max, other.Max));
        }

        public BoundingBox Intersection(BoundingBox other)
        {
            return new BoundingBox(
                Vector3.Max(Min, other.Min),
                Vector3.Min(Max, other.Max));
        }

        public bool Contains(BoundingBox other)
        {
            return Min.x <= other.Min.x &&
                   Min.y <= other.Min.y &&
                   Min.z <= other.Min.z &&
                   Max.x >= other.Max.x &&
                   Max.y >= other.Max.y &&
                   Max.z >= other.Max.z;
        }
        public bool Contains(Vector3 point)
        {
            return Contains(new BoundingBox(point, point));
        }

        public bool Intersects(BoundingBox other)
        {
            return Min.x <= other.Max.x &&
                   Min.y <= other.Max.y &&
                   Min.z <= other.Max.z &&
                   Max.x >= other.Min.x &&
                   Max.y >= other.Min.y &&
                   Max.z >= other.Min.z;
        }

        public float DistanceTo(float x, float y, float z)
        {
            var dX = AxisDistance(x, Min.x, Max.x);
            var dY = AxisDistance(y, Min.y, Max.y);
            var dZ = AxisDistance(z, Min.z, Max.z);
            return Mathf.Sqrt(dX * dX + dY * dY + dZ * dZ);
        }


        public override bool Equals(object obj)
        {
            if (!(obj is BoundingBox other)) return false;

            const float tolerance = RBush<ISpatialData>.Tolerance;

            return Math.Abs(Min.x - other.Min.x) < tolerance &&
                   Math.Abs(Min.y - other.Min.y) < tolerance &&
                   Math.Abs(Min.z - other.Min.z) < tolerance &&
                   Math.Abs(Max.x - other.Max.x) < tolerance &&
                   Math.Abs(Max.y - other.Max.y) < tolerance &&
                   Math.Abs(Max.z - other.Max.z) < tolerance;
        }

        public override int GetHashCode()
        {
            return Min.GetHashCode() ^ Max.GetHashCode();
        }

        private static float AxisDistance(float p, float min, float max)
        {
            return p < min ? min - p :
                p > max ? p - max :
                0;
        }

        public override string ToString()
        {
            return $"BBox({Min}, {Max})";
        }
    }
}