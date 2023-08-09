using System;
using System.Collections.Generic;
using RTree;
using UnityEngine;
using Random = System.Random;

namespace Dungeon
{
    [Serializable]
    public class LevelConfig
    {
        public int size = 3;
        public int variation = 3;

        public int offset = 1;
        public int offsetVariation;

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

            var y = 0;
            for (var i = 0; i < boundaries.y; i++)
            {
                var h = config.height + (float)random.NextDouble() * config.heightVariation;
                CreateLevel(config, h, y);
                var o = (int)(config.offset + (float)random.NextDouble() * config.offsetVariation);
                y += Mathf.CeilToInt(h) + o;
            }

            BBox = CalculateBBox(2);
        }

        public Vector3Int Boundaries { get; }
        public List<Room> Rooms { get; }
        public BoundingBox3D BBox { get; set; }

        private void CreateLevel(LevelConfig config, float height, int level)
        {
            var bush = new RBush();
            var maxAttempts = (int)(Boundaries.x * Boundaries.z / Mathf.Pow(config.size + 1, 2));

            var count = config.count + _random.NextDouble() * config.countVariation;
            for (int i = 0, j = 0; i < maxAttempts && j < count; i++)
            {
                var bbox = CreateRandomRoom(
                    Boundaries.x,
                    Boundaries.z,
                    config.size,
                    config.size + (int)(_random.NextDouble() * config.variation)
                );
                if (!bush.Collides(bbox))
                {
                    bush.Add(bbox);
                    var room = new Room(
                        new Vector3Int((int)bbox.MinX, level, (int)bbox.MinY),
                        new Vector3((int)bbox.Width, height, (int)bbox.Height)
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

        private BoundingBox3D CalculateBBox(int offset)
        {
            if (Rooms.Count == 0)
                return BoundingBox3D.Empty;

            var min = Rooms[0].Min;
            var max = Rooms[0].Max;

            for (var i = 1; i < Rooms.Count; i++)
            {
                min = Vector3Int.Min(min, Rooms[i].Min);
                max = Vector3Int.Max(max, Rooms[i].Max);
            }

            min -= Vector3Int.one * offset;
            max += Vector3Int.one * offset;

            return new BoundingBox3D(min, max);
        }

        private BoundingBox CreateRandomRoom(int maxWidth, int maxHeight, int min, int max)
        {
            var width = Mathf.FloorToInt(min + (float)(_random.NextDouble() * (max - min)));
            var height = Mathf.FloorToInt(min + (float)(_random.NextDouble() * (max - min)));
            var x = Mathf.FloorToInt((float)(_random.NextDouble() * (maxWidth - width - 1)));
            var y = Mathf.FloorToInt((float)(_random.NextDouble() * (maxHeight - height - 1)));


            return new BoundingBox(
                x,
                y,
                x + width,
                y + height
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