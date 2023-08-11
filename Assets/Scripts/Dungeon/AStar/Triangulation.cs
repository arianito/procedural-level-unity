using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dungeon
{
    public class Triangulation
    {
        public static BoundingBox CalculateBBox(List<Vector3> vertices, int offset)
        {
            if (vertices.Count == 0)
                return new BoundingBox(Vector3.zero, Vector3.zero);

            var min = vertices[0];
            var max = vertices[0];

            for (var i = 1; i < vertices.Count; i++)
            {
                min = Vector3.Min(min, vertices[i]);
                max = Vector3.Max(max, vertices[i]);
            }

            min -= Vector3.one * offset;
            max += Vector3.one * offset;

            return new BoundingBox(min, max);
        }

        public static Triangle CalculateSupraTriangle(List<Vector3> vertices)
        {
            var bbox = CalculateBBox(vertices, 2);
            var max = Mathf.Max(bbox.Width, bbox.Depth) * 2;
            return new Triangle(
                bbox.Min,
                bbox.Min + Vector3.right * max,
                bbox.Min + Vector3.forward * max
            );
        }

        public static List<Triangle> Triangulate2D(List<Vector3> vertices)
        {
            var supra = CalculateSupraTriangle(vertices);
            var triangles = new List<Triangle> { supra };

            foreach (var vertex in vertices)
            {
                var badTriangles = triangles.Where(t => t.Contains(vertex)).ToList();

                var polygon = (from badTriangle in badTriangles
                        from edge in badTriangle.Edges
                        where !badTriangles.Any(other => !other.Equals(badTriangle) && other.HasEdge(edge))
                        select edge
                    ).ToList();

                badTriangles.ForEach(t => triangles.Remove(t));
                triangles.AddRange(polygon.Select(edge => new Triangle(edge.A, edge.B, vertex)));
            }

            triangles.RemoveAll(triangle =>
                triangle.HasVertex(supra.A) ||
                triangle.HasVertex(supra.B) ||
                triangle.HasVertex(supra.C)
            );

            return triangles;
        }


        public static Tetrahedron CalculateSupraTetrahedron(List<Vector3> vertices)
        {
            var bbox = CalculateBBox(vertices, 3);
            var max = Mathf.Max(bbox.Width, bbox.Height, bbox.Depth) * 2;
            return new Tetrahedron(
                bbox.Min,
                bbox.Min + Vector3.right * max,
                bbox.Min + Vector3.forward * max,
                bbox.Min + Vector3.up * max
            );
        }


        public static List<Tetrahedron> Triangulate3D(List<Vector3> vertices)
        {
            var supra = CalculateSupraTetrahedron(vertices);
            var tetrahedrons = new List<Tetrahedron> { supra };

            foreach (var vertex in vertices)
            {
                var badTetrahedrons = tetrahedrons.Where(t => t.Contains(vertex)).ToList();


                var polygon = (from t in badTetrahedrons
                        from triangle in t.Triangles
                        where !badTetrahedrons.Any(other => !other.Equals(t) && other.HasTriangle(triangle))
                        select triangle
                    ).ToList();


                badTetrahedrons.ForEach(t => tetrahedrons.Remove(t));
                tetrahedrons.AddRange(
                    polygon.Select(triangle => new Tetrahedron(triangle.A, triangle.B, triangle.C, vertex)));
            }

            tetrahedrons.RemoveAll(triangle =>
                triangle.HasVertex(supra.A) ||
                triangle.HasVertex(supra.B) ||
                triangle.HasVertex(supra.C) ||
                triangle.HasVertex(supra.D)
            );

            return tetrahedrons;
        }

        public static HashSet<Edge> GetUniqueEdges(List<Tetrahedron> tetrahedrons)
        {
            return tetrahedrons.Aggregate(new HashSet<Edge>(), (edgeSet, t) =>
            {
                t.Edges.ForEach(edge => edgeSet.Add(edge));
                return edgeSet;
            });
        }

        public static HashSet<Edge> GetUniqueEdges(List<Triangle> triangles)
        {
            return triangles.Aggregate(new HashSet<Edge>(), (edgeSet, t) =>
            {
                t.Edges.ForEach(edge => edgeSet.Add(edge));
                return edgeSet;
            });
        }


        public static HashSet<Edge> Triangulate(List<Vector3> vertices)
        {
            var bbox = CalculateBBox(vertices, 0);
            if (bbox.IsPoint || bbox.IsEdge) return new HashSet<Edge>();
            if (!bbox.IsPlane)
            {
                var tetrahedrons = Triangulate3D(vertices);
                return GetUniqueEdges(tetrahedrons);
            }

            var planeDirection = bbox.PlaneDirection;

            ProjectOnXZ(vertices, planeDirection);
            var triangles = Triangulate2D(vertices);
            RevertFromXZ(triangles, planeDirection);
            return GetUniqueEdges(triangles);
        }

        public static void RevertFromXZ(List<Triangle> triangles, Vector3Int dir)
        {
            // y direction
            if (dir == Vector3Int.up)
                return;

            if (dir == Vector3Int.right)
            {
                // x direction

                for (var i = 0; i < triangles.Count; i++)
                {
                    var t = triangles[i];
                    triangles[i] = new Triangle(
                        new Vector3(t.A.y, t.A.x, t.A.z),
                        new Vector3(t.B.y, t.B.x, t.B.z),
                        new Vector3(t.C.y, t.C.x, t.C.z)
                    );
                }

                return;
            }


            // z direction
            for (var i = 0; i < triangles.Count; i++)
            {
                var t = triangles[i];
                triangles[i] = new Triangle(
                    new Vector3(t.A.x, t.A.z, t.A.y),
                    new Vector3(t.B.x, t.B.z, t.B.y),
                    new Vector3(t.C.x, t.C.z, t.C.y)
                );
            }
        }

        public static void ProjectOnXZ(List<Vector3> vertices, Vector3Int dir)
        {
            // y direction
            if (dir == Vector3Int.up)
                return;

            if (dir == Vector3Int.right)
            {
                // x direction
                for (var i = 0; i < vertices.Count; i++)
                {
                    var v = vertices[i];
                    vertices[i] = new Vector3(v.y, v.x, v.z);
                }

                return;
            }


            // z direction
            for (var i = 0; i < vertices.Count; i++)
            {
                var v = vertices[i];
                vertices[i] = new Vector3(v.x, v.z, v.y);
            }
        }

        public static List<Edge> FindMinimumSpanningTreeKruscals(HashSet<Edge> e)
        {
            var parent = new Dictionary<Vector3, Vector3>();
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

        private static Vector3 FindParent(Dictionary<Vector3, Vector3> parent, Vector3 p)
        {
            if (parent[p] != p)
                parent[p] = FindParent(parent, parent[p]);
            return parent[p];
        }


        public static List<Edge> MinimumSpanningTreePrim(HashSet<Edge> edges, Vector3 start)
        {
            var mst = new List<Edge>();
            var vertices = new HashSet<Vector3> { start };

            while (edges.Count > 0)
            {
                Edge targetEdge = null;
                var minLength = float.PositiveInfinity;

                foreach (var edge in edges.Where(edge =>
                             // only contain one of vertices
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