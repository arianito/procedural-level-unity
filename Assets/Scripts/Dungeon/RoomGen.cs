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
        public Vector3Int Boundaries { get; }
        public List<Room> Rooms { get; }
        public BoundingBox BBox => _bush.BBox;

        private readonly Random _random;

        private RBush<Room> _bush;

        public RoomGen(Vector3Int boundaries, Random random, LevelConfig config)
        {
            Rooms = new List<Room>();
            Boundaries = boundaries;
            _random = random;

            _bush = new RBush<Room>();
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

                var room = new Room(pos, scale);

                if (!_bush.Collides(room.BBox))
                {
                    _bush.Add(room);
                    Rooms.Add(room);
                }
            }
        }
        
        public Room GetRoom(Vector3 center)
        {
            var found = _bush.Search(new BoundingBox(center, center));
            if (found.Count == 0) return null;
            return found[0];
        }
    }
}