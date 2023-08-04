using UnityEngine;

namespace Dungeon
{
    public class Edge
    {
        private readonly Vector2 _a;
        private readonly Vector2 _b;
        private readonly float _length;

        public Vector2 A => _a;
        public Vector2 B => _b;
        public float Length => _length;

        public Edge(Vector2 a, Vector2 b)
        {
            _a = a;
            _b = b;
            _length = Vector2.Distance(_a, _b);
        }

        public void Debug(float z)
        {
            Gizmos.DrawLine(
                new Vector3(_a.x, z, _a.y),
                new Vector3(_b.x, z, _b.y)
            );
        }

        public override bool Equals(object obj)
        {
            if (obj is Edge other)
                return (other._a.Equals(_a) && other._b.Equals(_b)) || (other._a.Equals(_b) && other._b.Equals(_a));

            return false;
        }

        public override int GetHashCode()
        {
            return _a.GetHashCode() ^ _b.GetHashCode();
        }
    }
}