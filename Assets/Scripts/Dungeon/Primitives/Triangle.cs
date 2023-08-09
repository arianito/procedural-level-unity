using System.Collections.Generic;
using UnityEngine;

namespace Dungeon
{
    public class Triangle
    {
        public Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            // https://mathworld.wolfram.com/Circumcircle.html

            A = a;
            B = b;
            C = c;

            var ap = new Matrix4x4(
                new Vector4(1, 0, 0, 0),
                new Vector4(0, A.x, A.z, 1),
                new Vector4(0, B.x, B.z, 1),
                new Vector4(0, C.x, C.z, 1)
            ).determinant;

            var a2 = A.sqrMagnitude;
            var b2 = B.sqrMagnitude;
            var c2 = C.sqrMagnitude;

            var bx = -new Matrix4x4(
                new Vector4(1, 0, 0, 0),
                new Vector4(0, a2, A.z, 1),
                new Vector4(0, b2, B.z, 1),
                new Vector4(0, c2, C.z, 1)
            ).determinant;


            var bz = new Matrix4x4(
                new Vector4(1, 0, 0, 0),
                new Vector4(0, a2, A.x, 1),
                new Vector4(0, b2, B.x, 1),
                new Vector4(0, c2, C.x, 1)
            ).determinant;


            var cp = -new Matrix4x4(
                new Vector4(1, 0, 0, 0),
                new Vector4(0, a2, A.x, A.z),
                new Vector4(0, b2, B.x, B.z),
                new Vector4(0, c2, C.x, C.z)
            ).determinant;

            Circumcenter = new Vector3(
                -bx / (2 * ap),
                0,
                -bz / (2 * ap)
            );

            Radius = (bx * bx + bz * bz - 4 * ap * cp) / (4 * ap * ap);
        }

        public Vector3 A { get; }
        public Vector3 B { get; }
        public Vector3 C { get; }

        public Vector3 Circumcenter { get; }
        public float Radius { get; }

        public List<Edge> Edges => new List<Edge>
        {
            new Edge(A, B),
            new Edge(B, C),
            new Edge(C, A)
        };

        public List<Vector3> Vertices => new List<Vector3>
        {
            A, B, C
        };

        public void DebugDraw()
        {
            foreach (var e in Edges)
                e.DebugDraw();
        }

        public bool Contains(Vector3 p)
        {
            return (Circumcenter - p).sqrMagnitude <= Radius;
        }

        public bool HasEdge(Edge edge)
        {
            foreach (var other in Edges)
                if (other.Equals(edge))
                    return true;
            return false;
        }

        public bool HasVertex(Vector3 vertex)
        {
            return A.NearEqual(vertex) || B.NearEqual(vertex) || C.NearEqual(vertex);
        }


        public override bool Equals(object obj)
        {
            if (obj is Triangle other)
                return (other.A.NearEqual(A) && other.B.NearEqual(B) && other.C.NearEqual(C)) ||
                       (other.A.NearEqual(B) && other.B.NearEqual(C) && other.C.NearEqual(A)) ||
                       (other.A.NearEqual(C) && other.B.NearEqual(A) && other.C.NearEqual(B));

            return false;
        }


        public override int GetHashCode()
        {
            return A.GetHashCode() ^ B.GetHashCode() ^ C.GetHashCode();
        }
    }
}