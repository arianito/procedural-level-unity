using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dungeon
{
    public static class Delaunay
    {
        private static Rect CalculateBbox(List<Vector2> points, float offset = 1.0f)
        {
            if (points.Count == 0)
                return new Rect();

            var min = points[0];
            var max = points[0];

            for (var i = 1; i < points.Count; i++)
            {
                min = Vector2.Min(min, points[i]);
                max = Vector2.Max(max, points[i]);
            }

            min -= Vector2.one * offset;
            max += Vector2.one * offset;

            return new Rect(min, max - min);
        }

        private static Triangle CalculateSupra(List<Vector2> points)
        {
            var bbox = CalculateBbox(points);
            var yOffset = bbox.height / 10;
            return new Triangle(
                new Vector2(bbox.xMin - bbox.width, bbox.yMin - yOffset),
                new Vector2(bbox.xMax + bbox.width, bbox.yMin - yOffset),
                new Vector2(bbox.center.x, bbox.yMax + bbox.height - yOffset)
            );
        }

        public static List<Triangle> Triangulate(List<Vector2> points, bool removeSupra = true)
        {
            var supra = CalculateSupra(points);
            var triangles = new List<Triangle> { supra };

            foreach (var vertex in points)
            {
                // determine bad triangles
                var badTriangles = triangles.Where(triangle => triangle.Contains(vertex)).ToList();

                // create polygon around bad triangles
                // (find and select non-repeating edges)
                var polygon = (from badTriangle in badTriangles
                        from edge in badTriangle.GetEdges()
                        let isShared = badTriangles.Any(other => other != badTriangle && other.HasEdge(edge))
                        where !isShared
                        select edge
                    ).ToList();

                badTriangles.ForEach(t => triangles.Remove(t));
                triangles.AddRange(polygon.Select(edge => new Triangle(edge.A, edge.B, vertex)));
            }

            if (removeSupra)
            {
                triangles.RemoveAll(triangle =>
                    triangle.HasVertex(supra.A) ||
                    triangle.HasVertex(supra.B) ||
                    triangle.HasVertex(supra.C)
                );
            }

            return triangles;
        }

        public static HashSet<Edge> GetUniqueEdges(List<Triangle> triangles)
        {
            return triangles.Aggregate(new HashSet<Edge>(), (edgeSet, t) =>
            {
                t.GetEdges().ForEach(edge => edgeSet.Add(edge));
                return edgeSet;
            });
        }
        
        
        public static List<Edge> FindMinimumSpanningTreeKruscals(HashSet<Edge> e)
        {
            var parent = new Dictionary<Vector2, Vector2>();
            var mst = new List<Edge>();
            var edges = e.ToList();
            edges.Sort((edge1, edge2) => edge1.Length.CompareTo(edge2.Length));

            foreach (var edge in edges)
            {
                if (!parent.ContainsKey(edge.A))
                    parent[edge.A] = edge.A;
                if (!parent.ContainsKey(edge.B))
                    parent[edge.B] = edge.B;
            }

            foreach (var edge in edges)
            {
                var aParent = FindParent(parent, edge.A);
                var bParent = FindParent(parent, edge.B);

                if (aParent == bParent) continue;

                mst.Add(edge);
                parent[FindParent(parent, aParent)] = FindParent(parent, bParent);
            }

            return mst;
        }
        
        private static Vector2 FindParent(Dictionary<Vector2, Vector2> parent, Vector2 p)
        {
            if (parent[p] != p)
                parent[p] = FindParent(parent, parent[p]);
            return parent[p];
        }


        public static List<Edge> MinimumSpanningTreePrim(HashSet<Edge> edges, Vector2 start)
        {
            var mst = new List<Edge>();
            var vertices = new HashSet<Vector2> { start };

            while (edges.Count > 0)
            {
                Edge targetEdge = null;
                var minLength = float.PositiveInfinity;

                foreach (var edge in edges.Where(edge =>
                             // only contains one of vertices at the time
                             vertices.Contains(edge.A) != vertices.Contains(edge.B)
                             && edge.Length < minLength
                         ))
                {
                    targetEdge = edge;
                    minLength = edge.Length;
                }

                // not found!
                if (targetEdge == null) break;

                mst.Add(targetEdge);
                edges.Remove(targetEdge);

                // visit edge vertices
                vertices.Add(targetEdge.A);
                vertices.Add(targetEdge.B);
            }

            return mst;
        }
    }
}