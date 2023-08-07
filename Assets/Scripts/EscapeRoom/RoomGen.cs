using System.Collections.Generic;
using RBush;
using UnityEngine;
using Random = System.Random;

namespace EscapeRoom
{
    public class Room
    {
        public Vector3Int WorldPosition { get; }
        public Vector3Int WorldScale { get; }

        public Vector3Int Min => WorldPosition;
        public Vector3Int Max => WorldPosition + WorldScale;
        public int Area => WorldScale.x * WorldScale.z;

        public Room(Vector3Int worldPosition, Vector3Int worldScale)
        {
            WorldPosition = worldPosition;
            WorldScale = worldScale;
        }

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

    public class Point : ISpatialData
    {
        public Point(Envelope envelope) => _envelope = envelope;

        private readonly Envelope _envelope;
        public ref readonly Envelope Envelope => ref _envelope;
    }

    public class RoomGen
    {
        public Vector3Int Boundaries { get; }
        public HashSet<Room> Rooms { get; }
        public Vector3Int BBox { get; set; }
        public int Offset { get; }

        private Random _random;


        public RoomGen(Vector3Int boundaries, Random random, int offset)
        {
            Rooms = new HashSet<Room>();
            BBox = Vector3Int.zero;
            Boundaries = boundaries;
            Offset = offset;
            _random = random;
            // CreateLevel(0, 1);


            var bush = new RBush<Point>();

            bush.Insert(new Point(new Envelope(0, 0, 9, 9)));
            bush.Insert(new Point(new Envelope(10, 10, 20, 20)));

            var p = bush.Search(new Envelope(10, 10, 20, 20));
            Debug.Log(p.Count);
            
            CalculateBBox();
        }

        private void CreateLevel(int level, int height)
        {
            for (var i = 0; i < 10; i++)
            {
                for (var a = 0; a < 5; a++)
                {
                    var room = CreateRandomRoom(level, height, 2, 6);
                }
            }
        }


        private void CalculateBBox()
        {
            BBox = new Vector3Int(0, 0, 0);
        }

        private Room CreateRandomRoom(int level, int height, int min, int max)
        {
            var width = Mathf.FloorToInt(min + (float)(_random.NextDouble() * (max - min)));
            var depth = Mathf.FloorToInt(min + (float)(_random.NextDouble() * (max - min)));


            var x = Mathf.FloorToInt((float)(_random.NextDouble() * (Boundaries.x - width - 1)));
            var z = Mathf.FloorToInt((float)(_random.NextDouble() * (Boundaries.z - depth - 1)));


            return new Room(
                new Vector3Int(
                    (int)x,
                    level,
                    (int)z
                ),
                new Vector3Int(
                    (int)width,
                    height,
                    (int)depth
                )
            );
        }

        public void DebugDraw()
        {
            Gizmos.color = Color.red;
            foreach (var room in Rooms)
            {
                var scale = new Vector3(room.WorldScale.x, room.WorldScale.y, room.WorldScale.z);
                var ex = 0.2f;
                Gizmos.DrawCube(
                    room.WorldPosition + scale / 2.0f,
                    new Vector3(scale.x - ex, scale.y, scale.z - ex)
                );
            }
        }
    }
}