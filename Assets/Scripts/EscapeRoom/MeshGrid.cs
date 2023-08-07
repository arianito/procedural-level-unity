using UnityEngine;

namespace EscapeRoom
{
    public class MeshNode
    {
        public MeshGrid Owner;
        public Vector3Int GridIndex;
        public bool IsWalkable;
        public MeshNode Previous;

        public Vector3 WorldPosition;
        public Vector3 WorldScale;
    }

    public class MeshGrid
    {
        public readonly MeshNode[,,] Nodes;

        public MeshGrid(Vector3Int gridSize)
        {
            Nodes = new MeshNode[gridSize.x, gridSize.y, gridSize.z];

            for (var i = 0; i < gridSize.x; i++)
            {
                for (var j = 0; j < gridSize.y; j++)
                {
                    for (var k = 0; k < gridSize.z; k++)
                    {
                        var scale = new Vector3(1, 1, 1);
                        var position = new Vector3(i * scale.x, j * scale.y, k * scale.z);

                        Nodes[i, j, k] = new MeshNode()
                        {
                            Owner = this,
                            GridIndex = new Vector3Int(i, j, k),
                            WorldPosition = position,
                            WorldScale = scale,
                        };
                    }
                }
            }
        }

        public void DebugDraw()
        {
            Gizmos.color = Color.grey;
            foreach (var node in Nodes)
            {
                Gizmos.DrawWireCube(node.WorldPosition + node.WorldScale / 2.0f, node.WorldScale);
            }
        }
    }
}