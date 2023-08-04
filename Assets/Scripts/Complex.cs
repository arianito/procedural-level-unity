using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Dungeon
{
    public class Complex
    {

        private Level _level;
        private List<Room> _rooms;
        private int _seed;
        private Random _random;

        public List<Room> rooms => this._rooms;

        public Complex(Level level, int seed)
        {
            _level = level;
            _seed = seed;
            _rooms = new List<Room>();
            _random = new Random(_seed);
        }

        public void Generate()
        {
            _rooms.Clear();
            _rooms.AddRange(GenerateRooms(_level));
        }

        private List<Room> GenerateRooms(Level level)
        {
            Room room = new Room(level);

            var lst = new List<Room>();
            Stack<Room> stack = new Stack<Room>();
            stack.Push(room);

            while (stack.Count > 0)
            {
                var first = stack.Pop();
                var spl = Split(first, level.offset);
                if (spl.Length == 0) continue;

                foreach (var space in spl)
                {
                    var area = Mathf.Clamp((float)(_random.NextDouble() * level.range), 1, 400);
                    if (space.Area() > area)
                    {
                        stack.Push(space);
                    }
                    else if (space.Area() > 0)
                    {
                        lst.Add(space);
                    }
                }
            }

            var i = 0;

            if (lst.Count > 3)
            {
                lst.Sort((a, b) =>
                {
                    var ratioDiff = Mathf.Sign((a.Ratio() - b.Ratio()) * 5);
                    var distDiff = Vector2.Distance(room.bounds.center, a.bounds.center) -
                                   Vector3.Distance(room.bounds.center, b.bounds.center);

                    return (int)(Mathf.Max(distDiff, -ratioDiff));
                });
            }

            if (lst.Count > level.count)
            {
                lst.RemoveAll(r =>
                {
                    i++;
                    return i > level.count;
                });
            }

            return lst;
        }


        private Room[] Split(Room room, int offset)
        {
            var isHorizontal = room.bounds.width >= room.bounds.height;

            var portion = Mathf.Max(room.bounds.width, room.bounds.height) / _level.segments;
            var rangeDown = Mathf.FloorToInt(portion + ((_level.segments - 2) * portion) * (float)_random.NextDouble());

            
            var splits = new Room[2];
            if (isHorizontal)
            {
                splits[0] =
                    new Room(room.bounds.x, room.bounds.y, rangeDown, room.bounds.height);
                splits[1] =
                    new Room(room.bounds.x + rangeDown + offset, room.bounds.y, room.bounds.width - rangeDown - offset,
                        room.bounds.height);
            }
            else
            {
                splits[0] =
                    new Room(room.bounds.x, room.bounds.y, room.bounds.width, rangeDown);
                splits[1] =
                    new Room(room.bounds.x, room.bounds.y + rangeDown + offset, room.bounds.width,
                        room.bounds.height - rangeDown - offset);
            }


            return splits;
        }
    }
}