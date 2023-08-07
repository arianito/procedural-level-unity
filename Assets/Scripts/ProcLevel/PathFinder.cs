using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Random = System.Random;

namespace Dungeon
{
    public class PathFinder
    {
        public MeshGrid meshGrid;

        private MeshGrid.Node startNode;
        private MeshGrid.Node endNode;

        public List<MeshGrid.Node> path;

        public Room A;
        public Room B;

        public PathFinder(MeshGrid grid, Room a, Room b, Random rnd)
        {
            meshGrid = grid;
            A = a;
            B = b;


            startNode = meshGrid.GetSocketFacing(a, b.bounds.center);
            endNode = meshGrid.GetSocketFacing(b, a.bounds.center);

            var openSet = new List<MeshGrid.Node>() { startNode };
            var closeSet = new HashSet<MeshGrid.Node>();

            var tolerance = 0.1f;

            while (openSet.Count > 0)
            {
                var currentNode = openSet[0];
                foreach (var os in openSet)
                {
                    if (
                        os.fCost < currentNode.fCost ||
                        (Math.Abs(os.fCost - currentNode.fCost) < tolerance && os.hCost < currentNode.hCost)
                    )
                    {
                        currentNode = os;
                    }
                }

                openSet.Remove(currentNode);
                closeSet.Add(currentNode);

                if (currentNode == endNode)
                {
                    path = Retrace(currentNode);
                    return;
                }

                foreach (var neighbor in meshGrid.GetNeighbors(currentNode))
                {
                    if (!neighbor.walkable || closeSet.Contains(neighbor))
                        continue;

                    var newCost = currentNode.gCost + GetDistance(currentNode, neighbor);
                    if (newCost < currentNode.gCost || !openSet.Contains(neighbor))
                    {
                        neighbor.gCost = newCost;
                        neighbor.hCost = GetDistance(endNode, neighbor);
                        neighbor.prev = currentNode;
                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }
        }

        public List<MeshGrid.Node> Retrace(MeshGrid.Node end)
        {
            var nodes = new List<MeshGrid.Node>();
            var node = end;
            while (node != startNode)
            {
                node.walkable = false;
                node.type = MeshGrid.NodeType.Hallway;
                nodes.Add(node);
                node = node.prev;
            }

            node.walkable = false;
            node.type = MeshGrid.NodeType.Hallway;
            nodes.Add(node);

            return nodes;
        }

        public float GetDistance(MeshGrid.Node a, MeshGrid.Node b)
        {
            var dx = (int)Mathf.Abs(a.position.x - b.position.x);
            var dy = (int)Mathf.Abs(a.position.y - b.position.y);
            
            var cost = dx + dy;
            
            if (b.type == MeshGrid.NodeType.Room)
                cost += 20;
            else if (b.type == MeshGrid.NodeType.Empty)
                cost -= 5;
            else if (b.type == MeshGrid.NodeType.Hallway)
                cost += 1;
            
            return cost;
        }
    }
}