using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = System.Random;

namespace Dungeon
{
    public static class RoomUtils
    {
        public static Rect CalculateBbox(List<Room> rooms, float offset)
        {
            if (rooms.Count == 0)
                return new Rect();

            var min = rooms[0].bounds.position;
            var max = rooms[0].bounds.position + rooms[0].bounds.size;

            for (var i = 1; i < rooms.Count; i++)
            {
                min = Vector2.Min(min, rooms[i].bounds.position);
                max = Vector2.Max(max, rooms[i].bounds.position + rooms[i].bounds.size);
            }

            min -= Vector2.one * offset;
            max += Vector2.one * offset;

            return new Rect(min, max - min);
        }

        public static Room FindRoom(List<Room> rooms, Vector2 pos)
        {
            return rooms.Find(room => room.bounds.center.Equals(pos));
        }
        public static void ReCenter(List<Room> rooms)
        {
            var center = CalculateBbox(rooms, 0).center;
            rooms.ForEach(room => { room.bounds.position -= new Vector2Int(Mathf.FloorToInt(center.x), Mathf.FloorToInt(center.y)); });
        }

        public static List<Vector2> GetGridVertices(List<Room> rooms, float bboxOffset, float offset)
        {
            var bbox = CalculateBbox(rooms, bboxOffset);
            var verts = bbox.GetVertices(offset, 3, 3);
            var edges = rooms.Aggregate(new List<Vector2>(verts), (acc, room) =>
            {
                acc.AddRange(room.bounds.GetVertices(offset, 1, 1));
                return acc;
            });
            return edges;
        }

        public static List<Room> Generate(Random random, LevelConfig config)
        {
            var mother = new Room(config);
            var rooms = new List<Room>();

            var stack = new Stack<Room>();
            stack.Push(mother);

            while (stack.Count > 0)
            {
                var first = stack.Pop();
                var spl = Split(random, config, first, config.offset);
                if (spl.Length == 0) continue;

                foreach (var space in spl)
                {
                    var area = Mathf.Clamp((float)(random.NextDouble() * config.range), 1, 400);
                    if (space.Area() > area)
                    {
                        stack.Push(space);
                    }
                    else if (space.Area() > 0)
                    {
                        rooms.Add(space);
                    }
                }
            }

            var i = 0;

            if (rooms.Count > 3)
            {
                rooms.Sort((a, b) =>
                {
                    var ratioDiff = Mathf.Sign((a.Ratio() - b.Ratio()) * 5);
                    var distDiff = Vector2.Distance(mother.bounds.center, a.bounds.center) -
                                   Vector2.Distance(mother.bounds.center, b.bounds.center);

                    return (int)(Mathf.Max(distDiff, -ratioDiff));
                });
            }

            if (rooms.Count > config.count)
            {
                rooms.RemoveAll(r =>
                {
                    i++;
                    return i > config.count;
                });
            }

            return rooms;
        }


        private static Room[] Split(Random random, LevelConfig config, Room room, int offset)
        {
            var isHorizontal = room.bounds.width >= room.bounds.height;

            var portion = Mathf.Max(room.bounds.width, room.bounds.height) / config.segments;
            var rangeDown = Mathf.FloorToInt(portion + ((config.segments - 2) * portion) * (float)random.NextDouble());


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


        public static (Room, Room) FindFurthestRooms(List<Room> rooms)
        {
            var n = rooms.Count;
            var maxDistance = -1.0f;
            Room room1 = null;
            Room room2 = null;

            for (var i = 0; i < n; i++)
            {
                for (var j = i + 1; j < n; j++)
                {
                    var distance = Vector2.Distance(rooms[i].bounds.center, rooms[j].bounds.center);
                    if (distance <= maxDistance) continue;

                    maxDistance = distance;
                    room1 = rooms[i];
                    room2 = rooms[j];
                }
            }

            return (room1, room2);
        }
    }
}