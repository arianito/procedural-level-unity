using System.Collections.Generic;
using UnityEngine;

namespace Dungeon
{
    public class Tetrahedron
    {
        public Tetrahedron(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            // https://mathworld.wolfram.com/Circumsphere.html

            A = a;
            B = b;
            C = c;
            D = d;

            var ap = new Matrix4x4(
                new Vector4(A.x, A.y, A.z, 1),
                new Vector4(B.x, B.y, B.z, 1),
                new Vector4(C.x, C.y, C.z, 1),
                new Vector4(D.x, D.y, D.z, 1)
            ).determinant;

            var a2 = A.sqrMagnitude;
            var b2 = B.sqrMagnitude;
            var c2 = C.sqrMagnitude;
            var d2 = D.sqrMagnitude;

            var bx = new Matrix4x4(
                new Vector4(a2, A.y, A.z, 1),
                new Vector4(b2, B.y, B.z, 1),
                new Vector4(c2, C.y, C.z, 1),
                new Vector4(d2, D.y, D.z, 1)
            ).determinant;

            var by = -new Matrix4x4(
                new Vector4(a2, A.x, A.z, 1),
                new Vector4(b2, B.x, B.z, 1),
                new Vector4(c2, C.x, C.z, 1),
                new Vector4(d2, D.x, D.z, 1)
            ).determinant;

            var bz = new Matrix4x4(
                new Vector4(a2, A.x, A.y, 1),
                new Vector4(b2, B.x, B.y, 1),
                new Vector4(c2, C.x, C.y, 1),
                new Vector4(d2, D.x, D.y, 1)
            ).determinant;

            var cp = new Matrix4x4(
                new Vector4(a2, A.x, A.y, A.z),
                new Vector4(b2, B.x, B.y, B.z),
                new Vector4(c2, C.x, C.y, C.z),
                new Vector4(d2, D.x, D.y, D.z)
            ).determinant;

            Circumsphere = new Vector3(
                bx / (2 * ap),
                by / (2 * ap),
                bz / (2 * ap)
            );

            Radius = (bx * bx + by * by + bz * bz - 4 * ap * cp) / (4 * ap * ap);
        }

        public Vector3 A { get; }
        public Vector3 B { get; }
        public Vector3 C { get; }
        public Vector3 D { get; }


        public Vector3 Circumsphere { get; }
        public float Radius { get; }

        public List<Triangle> Triangles => new List<Triangle>
        {
            new Triangle(A, B, C),
            new Triangle(A, B, D),
            new Triangle(A, C, D),
            new Triangle(B, C, D)
        };

        public List<Edge> Edges => new List<Edge>
        {
            new Edge(A, B),
            new Edge(A, C),
            new Edge(A, D),
            new Edge(D, B),
            new Edge(B, C),
            new Edge(C, D)
        };

        public List<Vector3> Vertices => new List<Vector3>
        {
            A, B, C, D
        };


        public void DebugDraw()
        {
            foreach (var e in Edges)
                e.DebugDraw();
        }

        public bool Contains(Vector3 p)
        {
            return (Circumsphere - p).sqrMagnitude <= Radius;
        }

        public bool HasTriangle(Triangle triangle)
        {
            foreach (var other in Triangles)
                if (other.Equals(triangle))
                    return true;
            return false;
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
            return A.NearEqual(vertex) || B.NearEqual(vertex) || C.NearEqual(vertex) || D.NearEqual(vertex);
        }

        public override bool Equals(object obj)
        {
            if (obj is Tetrahedron other)
                return (other.A.NearEqual(A) && other.B.NearEqual(B) && other.C.NearEqual(C) && other.D.NearEqual(D)) ||
                       (other.A.NearEqual(B) && other.B.NearEqual(C) && other.C.NearEqual(D) && other.D.NearEqual(A)) ||
                       (other.A.NearEqual(C) && other.B.NearEqual(D) && other.C.NearEqual(A) && other.D.NearEqual(B)) ||
                       (other.A.NearEqual(D) && other.B.NearEqual(A) && other.C.NearEqual(B) && other.D.NearEqual(C));

            return false;
        }


        public override int GetHashCode()
        {
            return A.GetHashCode() ^ B.GetHashCode() ^ C.GetHashCode() ^ D.GetHashCode();
        }
    }
}