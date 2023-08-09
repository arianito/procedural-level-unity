using System.Collections.Generic;
using UnityEngine;

namespace Dungeon
{
    public class AStar
    {
        // https://www.youtube.com/@SebastianLague

        public static List<MeshNode> FindPath(MeshGrid meshGrid, Room a, Room b)
        {
            var sockets = GetNearestSockets(meshGrid, a, b);
            var startNode = sockets[0];
            var endNode = sockets[1];

            if (startNode == null || endNode == null)
                return new List<MeshNode>();

            var openSet = new Heap<MeshNode>(meshGrid.MaxSize);
            var closeSet = new HashSet<MeshNode>();
            openSet.Add(startNode);

            while (!openSet.Empty)
            {
                var currentNode = openSet.RemoveFirst();
                closeSet.Add(currentNode);

                if (currentNode == endNode)
                    return Retrace(currentNode, startNode);

                foreach (var neighbour in meshGrid.GetNeighbors(currentNode))
                {
                    if (!neighbour.IsWalkable || closeSet.Contains(neighbour))
                        continue;

                    var newMoveCostToNeighbour = currentNode.GCost + GetMovementCost(currentNode, neighbour);
                    if (!(newMoveCostToNeighbour < currentNode.GCost) && openSet.Contains(neighbour)) continue;


                    neighbour.GCost = newMoveCostToNeighbour;
                    neighbour.HCost = GetMovementCost(endNode, neighbour);
                    neighbour.Previous = currentNode;
                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                    else
                        openSet.UpdateItem(neighbour);
                }
            }

            return new List<MeshNode>();
        }

        private static float GetMovementCost(MeshNode a, MeshNode b)
        {
            var dx = Mathf.Abs(a.Center.x - b.Center.x);
            var dy = Mathf.Abs(a.Center.y - b.Center.y);
            var dz = Mathf.Abs(a.Center.z - b.Center.z);

            var xy = Mathf.Sqrt(2) * Mathf.Min(dx, dy) + Mathf.Abs(dx - dy);
            var zy = Mathf.Sqrt(2) * Mathf.Min(dz, dy) + Mathf.Abs(dz - dy);


            var cost = Mathf.Min((xy + dz), (zy + dx));
            if (b.Type == NodeType.Room)
                cost += 10;
            else if (b.Type == NodeType.Empty)
                cost -= 5;
            else if (b.Type == NodeType.Hallway)
                cost += 1;

            return cost;
        }

        public static List<MeshNode> GetAllSockets(MeshGrid grid, Room a)
        {
            var nodes = new List<MeshNode>();
            var pos = grid.WorldToNodeSpace(a.WorldPosition);

            int nx, nz;
            for (var x = 0; x < a.WorldScale.x; x++)
            {
                nx = pos.x + x;
                nz = pos.z - 1;
                if (
                    nx >= 0 && nx < grid.GridSize.x &&
                    nz >= 0 && nz < grid.GridSize.z
                )
                {
                    var node = grid.Nodes[nx, pos.y, nz];
                    if (node.IsWalkable)
                        nodes.Add(node);
                }

                nx = pos.x + x;
                nz = pos.z + a.WorldScaleInt.z;
                if (
                    nx >= 0 && nx < grid.GridSize.x &&
                    nz >= 0 && nz < grid.GridSize.z
                )
                {
                    var node = grid.Nodes[nx, pos.y, nz];
                    if (node.IsWalkable)
                        nodes.Add(node);
                }
            }

            for (var z = 0; z < a.WorldScale.z; z++)
            {
                nx = pos.x - 1;
                nz = pos.z + z;
                if (
                    nx >= 0 && nx < grid.GridSize.x &&
                    nz >= 0 && nz < grid.GridSize.z
                )
                {
                    var node = grid.Nodes[nx, pos.y, nz];
                    if (node.IsWalkable)
                        nodes.Add(node);
                }

                nx = pos.x + a.WorldScaleInt.x;
                nz = pos.z + z;
                if (
                    nx >= 0 && nx < grid.GridSize.x &&
                    nz >= 0 && nz < grid.GridSize.z
                )
                {
                    var node = grid.Nodes[nx, pos.y, nz];
                    if (node.IsWalkable)
                        nodes.Add(node);
                }
            }

            return nodes;
        }

        public static MeshNode[] GetNearestSockets(MeshGrid grid, Room a, Room b)
        {
            var nodesA = GetAllSockets(grid, a);
            var nodesB = GetAllSockets(grid, b);

            var nodes = new MeshNode[2];
            var minDist = float.PositiveInfinity;

            foreach (var nodeA in nodesA)
            {
                foreach (var nodeB in nodesB)
                {
                    var distance = Vector3.Distance(nodeA.WorldPosition, nodeB.WorldPosition);
                    if (!(distance < minDist)) continue;

                    minDist = distance;
                    nodes[0] = nodeA;
                    nodes[1] = nodeB;
                }
            }

            return nodes;
        }

        private static List<MeshNode> Retrace(MeshNode end, MeshNode startNode)
        {
            var nodes = new List<MeshNode>();
            var node = end;
            while (true)
            {
                node.SetWalkable(false);
                node.Type = NodeType.Hallway;
                nodes.Add(node);
                if (node == startNode) break;

                node = node.Previous;
            }

            nodes.Reverse();
            return nodes;
        }

        public static void DefineMeshGrid(MeshGrid grid, List<Room> rooms)
        {
            foreach (var room in rooms)
            {
                var size = room.WorldScaleInt;
                var pos = grid.WorldToNodeSpace(room.WorldPosition);

                for (var i = 0; i < size.x; i++)
                for (var j = 0; j < size.y; j++)
                for (var k = 0; k < size.z; k++)
                {
                    var node = grid.Nodes[pos.x + i, pos.y + j, pos.z + k];
                    node.SetWalkable(false);
                }
            }
        }
    }
}