using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dungeon
{
    public class Delaunay
    {
        private List<Room> _rooms;
        private List<Triangle> _triangles;
        private List<Edge> _edges;
        private List<Edge> _mst;
        private List<Edge> _outline;
        private Dictionary<Vector2, Vector2> _parent;

        public List<Triangle> Triangles => _triangles;
        public List<Edge> Edges => _edges;
        public List<Edge> MST => _mst;
        public List<Edge> Outline => _outline;


        public Delaunay(List<Room> rooms)
        {
            _rooms = rooms;
        }


        private Rect CalculateBBOX(List<Room> rooms)
        {
            if (rooms.Count == 0)
                return new Rect();

            var min = rooms[0].bounds.center;
            var max = rooms[0].bounds.center;

            for (var i = 1; i < rooms.Count; i++)
            {
                min = Vector2.Min(min, rooms[i].bounds.center);
                max = Vector2.Max(max, rooms[i].bounds.center);
            }

            return new Rect(min, max - min);
        }

        private Triangle CalculateSupra(List<Room> rooms)
        {
            var bbox = CalculateBBOX(rooms);
            var yOffset = bbox.height / 10;
            return new Triangle(
                new Vector2(bbox.xMin - bbox.width, bbox.yMin - yOffset),
                new Vector2(bbox.xMax + bbox.width, bbox.yMin - yOffset),
                new Vector2(bbox.center.x, bbox.yMax + bbox.height - yOffset)
            );
        }

        public void Triangulate()
        {
            var supra = CalculateSupra(_rooms);
            _triangles = new List<Triangle> { supra };

            foreach (var room in _rooms)
            {
                // determine bad triangles
                var badTriangles = _triangles.Where(triangle => triangle.Contains(room.bounds.center)).ToList();

                // create polygon around bad triangles
                // (find and select non-repeating edges)
                var polygon = (from badTriangle in badTriangles
                        from edge in badTriangle.GetEdges()
                        let isShared = badTriangles.Any(other => other != badTriangle && other.HasEdge(edge))
                        where !isShared
                        select edge
                    ).ToList();

                badTriangles.ForEach(t => _triangles.Remove(t));
                _triangles.AddRange(polygon.Select(edge => new Triangle(edge.A, edge.B, room.bounds.center)));
            }

            // remove supra
            _triangles.RemoveAll(triangle =>
                triangle.HasVertex(supra.A) ||
                triangle.HasVertex(supra.B) ||
                triangle.HasVertex(supra.C)
            );

            // get unique edges
            var edgeSet = new HashSet<Edge>();
            _triangles.ForEach(t => t.GetEdges().ForEach(edge => edgeSet.Add(edge)));
            _edges = edgeSet.ToList();

            // get outline
            var outlineSet = new HashSet<Edge>();
            _triangles.ForEach(t => t.GetEdges().ForEach(edge =>
            {
                var scale = 1f;
                var e = new Edge(edge.A * scale, edge.B * scale);
                if (outlineSet.Contains(e))
                    outlineSet.Remove(e);
                else outlineSet.Add(e);
            }));
            _outline = outlineSet.ToList();

            MinimumSpanningTreePrim();
        }


        private Vector2 FindParent(Vector2 p)
        {
            if (_parent[p] != p)
                _parent[p] = FindParent(_parent[p]);
            return _parent[p];
        }

        private void MinimumSpanningTreeKruscals()
        {
            _parent = new Dictionary<Vector2, Vector2>();
            _mst = new List<Edge>();
            _edges.Sort((edge1, edge2) => edge1.Length.CompareTo(edge2.Length));

            foreach (var edge in _edges)
            {
                if (!_parent.ContainsKey(edge.A))
                    _parent[edge.A] = edge.A;
                if (!_parent.ContainsKey(edge.B))
                    _parent[edge.B] = edge.B;
            }

            foreach (var edge in _edges)
            {
                var aParent = FindParent(edge.A);
                var bParent = FindParent(edge.B);

                if (aParent == bParent) continue;

                _mst.Add(edge);
                _parent[FindParent(aParent)] = FindParent(bParent);
            }
        }

        private void MinimumSpanningTreePrim()
        {
            _mst = new List<Edge>();
            var distancePoints = FindTwoMostDistancePoints();
            var start = distancePoints.A;

            var edges = new HashSet<Edge>(_edges);
            var vertices = new HashSet<Vector2> { start };

            while (edges.Count > 0)
            {
                Edge targetEdge = null;
                var minLength = float.PositiveInfinity;

                foreach (var edge in _edges.Where(edge =>
                             // only contains one of vertices at the time
                             vertices.Contains(edge.A) != vertices.Contains(edge.B)
                         ).Where(edge =>
                             // where it is shorter than others
                             edge.Length < minLength))
                {
                    targetEdge = edge;
                    minLength = edge.Length;
                }
                
                // not found!
                if (targetEdge == null) break;
                
                _mst.Add(targetEdge);
                edges.Remove(targetEdge);

                // visit edge vertices
                vertices.Add(targetEdge.A);
                vertices.Add(targetEdge.B);
            }
        }

        private bool ContainsOnlyOneVertex(HashSet<Vector2> vertices, Edge edge)
        {
            var containA = vertices.Contains(edge.A);
            var containB = vertices.Contains(edge.B);
            return containA == !containB;
        }


        private Edge FindTwoMostDistancePoints()
        {
            var n = _rooms.Count;
            var maxDistance = -1.0f;
            var room1 = Vector2.zero;
            var room2 = Vector2.zero;

            for (var i = 0; i < n; i++)
            {
                for (var j = i + 1; j < n; j++)
                {
                    var distance = Vector2.Distance(_rooms[i].bounds.center, _rooms[j].bounds.center);
                    if (distance <= maxDistance) continue;
                    
                    maxDistance = distance;
                    room1 = _rooms[i].bounds.center;
                    room2 = _rooms[j].bounds.center;
                }
            }

            return new Edge(room1, room2);
        }
    }
}