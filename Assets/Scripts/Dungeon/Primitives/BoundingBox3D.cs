using System.Collections.Generic;
using UnityEngine;

namespace Dungeon
{
    public class BoundingBox3D
    {
        public BoundingBox3D(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }

        public Vector3 Min { get; set; }
        public Vector3 Max { get; set; }

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

        public static BoundingBox3D Empty => new BoundingBox3D(new Vector3(0, 0, 0), new Vector3(0, 0, 0));


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

        public BoundingBox3D ProjectOnXZ =>
            new BoundingBox3D(new Vector3(Min.x, 0, Min.z), new Vector3(Max.x, 0, Max.z));

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

        public bool Contains(BoundingBox3D other)
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
            return Contains(new BoundingBox3D(point, point));
        }

        public void DebugDraw()
        {
            foreach (var edge in Edges) edge.DebugDraw();
        }
    }
}