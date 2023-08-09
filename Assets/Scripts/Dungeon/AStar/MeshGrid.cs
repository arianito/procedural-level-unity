using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace Dungeon
{
    public enum NodeType
    {
        Empty,
        Room,
        Hallway
    }

    public class MeshNode : IHeapNode<MeshNode>
    {
        private MeshNode _value;
        public MeshGrid Owner { get; set; }
        public Vector3Int GridIndex { get; set; }
        public bool IsWalkable { get; internal set; }

        public NodeType Type { get; set; }
        public MeshNode Previous { get; set; }

        public Vector3 WorldPosition { get; set; }
        public Vector3 WorldScale { get; set; }

        public float GCost { get; set; }
        public float HCost { get; set; }

        public float FCost => GCost + HCost;

        public Vector3 Center => WorldPosition + WorldScale / 2.0f;


        public int HeapIndex { get; set; }


        public void SetWalkable(bool walkable)
        {
            IsWalkable = walkable;
        }

        public int CompareTo(MeshNode nodeToCompare)
        {
            var compare = FCost.CompareTo(nodeToCompare.FCost);
            if (compare == 0) compare = HCost.CompareTo(nodeToCompare.HCost);
            return compare;
        }


        public void DebugDraw(float scale = 0.8f, bool opaque = false)
        {
            if (opaque || !IsWalkable)
                Gizmos.DrawCube(WorldPosition + WorldScale / 2.0f, WorldScale - Vector3.one * scale);
            else Gizmos.DrawWireCube(WorldPosition + WorldScale / 2.0f, WorldScale - Vector3.one * scale);
        }
    }

    public class MeshGrid
    {
        private readonly BoundingBox3D _bbox;
        public readonly MeshNode[,,] Nodes;

        public MeshGrid(BoundingBox3D bbox, Random random)
        {
            Random = random;
            _bbox = bbox;
            GridSize = bbox.SizeInt;

            Nodes = new MeshNode[GridSize.x, GridSize.y, GridSize.z];

            for (var i = 0; i < GridSize.x; i++)
            for (var j = 0; j < GridSize.y; j++)
            for (var k = 0; k < GridSize.z; k++)
            {
                var scale = new Vector3(1, 1, 1);
                var position = bbox.Min + new Vector3(i * scale.x, j * scale.y, k * scale.z);

                Nodes[i, j, k] = new MeshNode
                {
                    Owner = this,
                    IsWalkable = true,
                    GridIndex = new Vector3Int(i, j, k),
                    WorldPosition = position,
                    WorldScale = scale,
                };
            }
        }

        public Vector3Int GridSize { get; }

        public Random Random { get; }

        public int MaxSize => GridSize.x * GridSize.y * GridSize.z;


        public Vector3Int WorldToNodeSpace(Vector3 position)
        {
            var pos = position - _bbox.Min;
            return new Vector3Int(
                (int)pos.x,
                (int)pos.y,
                (int)pos.z
            );
        }


        public List<MeshNode> GetNeighbors(MeshNode node)
        {
            var offsets = new[]
            {
                new Vector3Int(-1, 0, 0),
                new Vector3Int(1, 0, 0),
                new Vector3Int(0, -1, 0),
                new Vector3Int(0, 1, 0),
                new Vector3Int(0, 0, -1),
                new Vector3Int(0, 0, 1)
            };

            return (
                from offset in offsets
                select node.GridIndex + offset
                into final
                where final.x >= 0 &&
                      final.x < GridSize.x &&
                      final.y >= 0 &&
                      final.y < GridSize.y &&
                      final.z >= 0 &&
                      final.z < GridSize.z
                select Nodes[final.x, final.y, final.z]
            ).ToList();
        }
    }
}