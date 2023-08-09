using UnityEngine;

namespace Dungeon
{
    public class Room
    {
        public Room(Vector3Int worldPosition, Vector3 worldScale)
        {
            WorldPosition = worldPosition;
            WorldScale = worldScale;
        }

        public Vector3Int WorldPosition { get; }
        public Vector3 WorldScale { get; }

        public Vector3Int Min => WorldPosition;


        public Vector3Int WorldScaleInt => new Vector3Int(
            Mathf.CeilToInt(WorldScale.x),
            Mathf.CeilToInt(WorldScale.y),
            Mathf.CeilToInt(WorldScale.z)
        );

        public Vector3Int Max => WorldPosition + WorldScaleInt;

        public Vector3 Center => WorldPosition + new Vector3(WorldScale.x, WorldScale.y, WorldScale.z) / 2.0f;


        public BoundingBox3D BBox => new BoundingBox3D(Min, Max);

        public override bool Equals(object obj)
        {
            return obj is Room other && other.WorldPosition.Equals(WorldPosition);
        }

        public override int GetHashCode()
        {
            return WorldPosition.GetHashCode();
        }

        public bool Intersects(Room other)
        {
            return Max.x > other.Min.x &&
                   Max.y > other.Min.y &&
                   Max.z > other.Min.z &&
                   Min.x < other.Max.x &&
                   Min.y < other.Max.y &&
                   Min.z < other.Max.z;
        }
    }
}