using UnityEngine;

namespace Dungeon
{
    public class Room: ISpatialData
    {
        public Vector3 WorldPosition { get; }
        public Vector3 WorldScale { get; }

        public Vector3Int WorldScaleInt => new Vector3Int(
            Mathf.CeilToInt(WorldScale.x),
            Mathf.CeilToInt(WorldScale.y),
            Mathf.CeilToInt(WorldScale.z)
        );

        public BoundingBox BBox { get; }

        public Room(Vector3 worldPosition, Vector3 worldScale)
        {
            WorldPosition = worldPosition;
            worldPosition.x = Mathf.FloorToInt(worldPosition.x);
            worldPosition.y = Mathf.FloorToInt(worldPosition.y);
            worldPosition.z = Mathf.FloorToInt(worldPosition.z);
            WorldScale = worldScale;
            worldScale.x = Mathf.CeilToInt(worldScale.x);
            worldScale.z = Mathf.CeilToInt(worldScale.z);
            BBox = new BoundingBox(worldPosition, worldPosition + worldScale);
        }

        public override bool Equals(object obj)
        {
            return obj is Room other && other.BBox.Equals(BBox);
        }

        public override int GetHashCode()
        {
            return BBox.GetHashCode();
        }
    }
}