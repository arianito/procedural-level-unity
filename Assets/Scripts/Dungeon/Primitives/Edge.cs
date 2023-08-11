using System.Collections.Generic;
using UnityEngine;

namespace Dungeon
{
    public class Edge
    {
        public Edge(Vector3 a, Vector3 b)
        {
            A = a;
            B = b;
            Length = (a - b).magnitude;
        }

        public Vector3 A { get; }
        public Vector3 B { get; }

        public float Length { get; }

        public List<Vector3> Vertices => new List<Vector3>
        {
            A, B
        };

        public void DebugDraw()
        {
            Gizmos.DrawLine(A, B);
        }

        public bool HasVertex(Vector3 vertex)
        {
            return A.NearEqual(vertex) || B.NearEqual(vertex);
        }
        
        public override bool Equals(object obj)
        {
            if (obj is Edge other)
                return (other.A.NearEqual(A) && other.B.NearEqual(B)) ||
                       (other.A.NearEqual(B) && other.B.NearEqual(A));
            return false;
        }


        public override int GetHashCode()
        {
            return A.GetHashCode() ^ B.GetHashCode();
        }
    }
}