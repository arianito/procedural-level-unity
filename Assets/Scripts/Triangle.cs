using System.Collections.Generic;
using UnityEngine;

namespace Dungeon
{
    public class Triangle
    {
        private Vector2 _a;
        private Vector2 _b;
        private Vector2 _c;


        public Vector2 A => _a;
        public Vector2 B => _b;
        public Vector2 C => _c;

        public Triangle(Vector2 a, Vector2 b, Vector2 c)
        {
            _a = a;
            _b = b;
            _c = c;
        }

        public bool Contains(Vector2 p)
        {
            var center = GetCenter();
            var radius = Vector2.Distance(center, _a);
            var distance = Vector2.Distance(center, p);
            return distance <= radius;
        }

        public void Debug(float z)
        {
            GetEdges().ForEach(edge => edge.Debug(z));
        }

        private Vector2 GetCenter()
        {
            var d = 2 * (
                _a.x * (_b.y - _c.y) +
                _b.x * (_c.y - _a.y) +
                _c.x * (_a.y - _b.y)
            );
            return (1 / d) * new Vector2(
                (_a.x * _a.x + _a.y * _a.y) * (_b.y - _c.y) +
                (_b.x * _b.x + _b.y * _b.y) * (_c.y - _a.y) +
                (_c.x * _c.x + _c.y * _c.y) * (_a.y - _b.y),
                (_a.x * _a.x + _a.y * _a.y) * (_c.x - _b.x) +
                (_b.x * _b.x + _b.y * _b.y) * (_a.x - _c.x) +
                (_c.x * _c.x + _c.y * _c.y) * (_b.x - _a.x)
            );
        }


        public List<Edge> GetEdges()
        {
            List<Edge> edges = new List<Edge>
            {
                new Edge(_a, _b),
                new Edge(_b, _c),
                new Edge(_c, _a)
            };
            return edges;
        }

        public bool HasEdge(Edge edge)
        {
            return GetEdges().Exists(e => e.Equals(edge));
        }

        public bool HasVertex(Vector2 vertex)
        {
            return _a.Equals(vertex) || _b.Equals(vertex) || _c.Equals(vertex);
        }
    }
}