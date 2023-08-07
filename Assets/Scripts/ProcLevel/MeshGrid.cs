using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;


namespace Dungeon
{
    public class MeshGrid
    {
        public enum NodeType
        {
            Empty,
            Room,
            Hallway
        }

        public class Node
        {
            public bool walkable;
            public bool occupied;
            public NodeType type;
            public Vector2 position;
            public int gridX;
            public int gridY;

            public float gCost;
            public float hCost;

            public Node prev;

            public float fCost => gCost + hCost;

            public Node(Vector2 pos, bool wlk)
            {
                position = pos;
                walkable = wlk;
            }
        }


        public Node[,] nodes;

        private Rect bounds;

        public int width;
        public int height;
        public int scalar = 1;
        private Random random;


        public int size => width * height;

        public MeshGrid(List<Room> rooms, Random r, int sc)
        {
            scalar = sc;
            bounds = RoomUtils.CalculateBbox(rooms, 2.0f);
            width = Mathf.FloorToInt(bounds.width * scalar);
            height = Mathf.FloorToInt(bounds.height * scalar);

            nodes = new Node[width, height];

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    var pos = bounds.position + new Vector2(i, j) / scalar;

                    var walkable = !rooms.Any(r => r.Intersect(pos + (Vector2.one * 0.5f) / scalar));
                    nodes[i, j] = new Node(pos, walkable)
                    {
                        gridX = i,
                        gridY = j,
                        type = NodeType.Room,
                    };
                }
            }
        }


        public List<Node> GetNeighbors(Node node)
        {
            var pos = new Vector2Int(node.gridX, node.gridY);
            var offsets = new[]
            {
                new Vector2Int(-1, 0),
                new Vector2Int(0, -1),
                new Vector2Int(1, 0),
                new Vector2Int(0, 1),
            };


            return (
                from offset in offsets
                select pos + offset
                into final
                where final.x >= 0 && final.x < width && final.y >= 0 && final.y < height
                select nodes[final.x, final.y]
            ).ToList();
        }

        public Node GetRoomNode(Room r)
        {
            var offset = new Vector2(Mathf.FloorToInt(bounds.width / 2.0f), Mathf.FloorToInt(bounds.height / 2.0f));
            var cell = r.bounds.position + Vector2.one / scalar * 0.5f;
            var pos = (cell + offset) * scalar;

            var i = Mathf.FloorToInt(pos.x);
            var j = Mathf.FloorToInt(pos.y);

            return nodes[i, j];
        }

        public Node GetSocketFacing(Room r, Vector2 p)
        {
            var (w, h, total) = GetSocketDetails(r);

            var dir = (p - r.bounds.center).normalized;

            var maxDot = -1.0f;
            Node found = null;

            for (var i = 0; i < total; i++)
            {
                var node = GetSocketInCycle(r, i);

                var nrm = (node.position - r.bounds.center).normalized;

                var dot = Vector2.Dot(dir, nrm);

                if (!node.walkable)
                    continue;

                if (!(dot > maxDot)) continue;

                maxDot = dot;
                found = node;
            }

            return found;
        }

        public Node GetRandomSocket(Room r)
        {
            var (w, h, total) = GetSocketDetails(r);

            Node node;
            do
            {
                node = GetSocketInCycle(r, (int)(random.NextDouble() * total));
            } while (!node.walkable);

            return node;
        }

        public (int, int, int) GetSocketDetails(Room r)
        {
            var w = Mathf.FloorToInt(r.bounds.size.x * scalar);
            var h = Mathf.FloorToInt(r.bounds.size.y * scalar);
            return (w, h, (w + h) * 2);
        }

        public Node GetSocketInCycle(Room r, int i)
        {
            var node = GetRoomNode(r);
            var (w, h, total) = GetSocketDetails(r);
            var steps = new[]
            {
                w,
                w + h,
                w + h + w,
            };

            var a = i % total;
            int x;
            int y;

            if (a < steps[0])
            {
                var k = a - 0;
                x = node.gridX + k;
                y = node.gridY - 1;
            }
            else if (a < steps[1])
            {
                var k = a - steps[0];
                x = node.gridX + w;
                y = node.gridY + k;
            }
            else if (a < steps[2])
            {
                var k = a - steps[1];
                x = node.gridX + (w - 1 - k);
                y = node.gridY + h;
            }
            else
            {
                var k = a - steps[2];
                x = node.gridX - 1;
                y = node.gridY + (h - 1 - k);
            }

            return nodes[x, y];
        }

        public void DebugDrawNode(Node node, Vector3 pos, bool opaque = false)
        {
            var sizeVec = new Vector3(1f, 1, 1f);
            var pos3d = new Vector3(node.position.x, 0, node.position.y);
            var p = (pos + pos3d) + (sizeVec / scalar) * 0.5f;
            var s = sizeVec / scalar;

            if (opaque)
                Gizmos.DrawCube(p, s);
            else
                Gizmos.DrawWireCube(p, s);
        }

        public void DebugDraw(Vector3 pos)
        {
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    var node = nodes[i, j];

                    Gizmos.color = node.walkable
                        ? (
                            node.occupied ? Color.blue : Color.white
                        )
                        : Color.red;
                    DebugDrawNode(node, pos - Vector3.up, node.occupied);
                }
            }
        }
    }
}