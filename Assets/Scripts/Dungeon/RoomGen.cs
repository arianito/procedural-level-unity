using System;
using System.Collections.Generic;
using Dungeon;
using UnityEngine;
using Random = System.Random;

namespace Dungeon
{
    [Serializable]
    public class LevelConfig
    {
        public int size = 3;
        public int variation = 3;

        public int height = 4;
        public float heightVariation = 1;

        public int count = 7;
        public int countVariation = 3;

        public float loopChance = 2;
    }

    public class RoomGen
    {
        private readonly Random _random;

        public RoomGen(Vector3Int boundaries, Random random, LevelConfig config)
        {
            Rooms = new List<Room>();
            Boundaries = boundaries;
            _random = random;

            var bush = new RBush();
            var n = config.count + (int)(random.NextDouble() * config.countVariation);
            for (var i = 0; i < n; i++)
            {
                var scale = new Vector3(
                    config.size + (int)(random.NextDouble() * config.variation),
                    config.height + (float)(random.NextDouble() * config.heightVariation),
                    config.size + (int)(random.NextDouble() * config.variation)
                );
                var pos = new Vector3(
                    (int)(random.NextDouble() * (boundaries.x - scale.x - 1)),
                    (int)(random.NextDouble() * (boundaries.y - scale.y - 1)),
                    (int)(random.NextDouble() * (boundaries.z - scale.z - 1))
                );

                var b = new BoundingBox(pos, pos + scale);

                if (!bush.Collides(b))
                {
                    bush.Add(b);
                    Rooms.Add(new Room(new Vector3Int((int)pos.x, (int)pos.y, (int)pos.z), scale));
                }
            }

            BBox = bush.BBox;
        }

        public Vector3Int Boundaries { get; }
        public List<Room> Rooms { get; }
        public BoundingBox BBox { get; set; }

        private void CreateLevel(LevelConfig config, float height, int level)
        {
            var bush = new RBush();
            var maxAttempts = (int)(Boundaries.x * Boundaries.z / Mathf.Pow(config.size + 1, 2));

            var count = config.count + _random.NextDouble() * config.countVariation;
            for (int i = 0, j = 0; i < maxAttempts && j < count; i++)
            {
                var bbox = CreateRandomRoom(
                    level,
                    Boundaries.x,
                    Boundaries.z,
                    config.size,
                    height,
                    config.size + (int)(_random.NextDouble() * config.variation)
                );
                if (!bush.Collides(bbox))
                {
                    bush.Add(bbox);
                    var room = new Room(
                        new Vector3Int((int)bbox.Min.x, (int)bbox.Min.y, (int)bbox.Min.z),
                        bbox.Size
                    );
                    Rooms.Add(room);
                    j++;
                }
            }
        }

        public Room GetRoom(Vector3 center)
        {
            foreach (var room in Rooms)
                if (room.BBox.Contains(center))
                    return room;

            return null;
        }

        private BoundingBox CreateRandomRoom(int level, int maxWidth, int maxDepth, float width, float height,
            float depth)
        {
            var x = Mathf.CeilToInt((float)(_random.NextDouble() * (maxWidth - width - 1)));
            var z = Mathf.CeilToInt((float)(_random.NextDouble() * (maxDepth - height - 1)));

            var pos = new Vector3(x, level, z);
            var scale = new Vector3(width, height, depth);

            return new BoundingBox(
                pos,
                pos + scale
            );
        }

        public void DebugDraw()
        {
            foreach (var room in Rooms)
            {
                var scale = new Vector3(room.WorldScaleInt.x, room.WorldScaleInt.y, room.WorldScaleInt.z);
                var bnd = new Vector3(Boundaries.x, Boundaries.y, Boundaries.z);
                Gizmos.color = Color.Lerp(
                    Color.red,
                    Color.blue,
                    Vector3.Distance(room.WorldPosition, bnd / 2.0f) / scale.sqrMagnitude
                );
                const float ex = 0.2f;
                Gizmos.DrawCube(
                    room.WorldPosition + scale / 2.0f,
                    new Vector3(scale.x - ex, scale.y, scale.z - ex)
                );
            }
        }


        public (Room, Room) FindFurthestRooms()
        {
            var n = Rooms.Count;
            var maxDistance = -1.0f;
            Room room1 = null;
            Room room2 = null;

            for (var i = 0; i < n; i++)
            for (var j = i + 1; j < n; j++)
            {
                var distance = Vector3.Distance(Rooms[i].Center, Rooms[j].Center);
                if (distance <= maxDistance) continue;

                maxDistance = distance;
                room1 = Rooms[i];
                room2 = Rooms[j];
            }

            return (room1, room2);
        }
    }
}