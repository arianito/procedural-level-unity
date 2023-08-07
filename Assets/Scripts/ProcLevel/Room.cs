using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon
{
    public class Room
    {
        public Rect bounds;

        public Room(LevelConfig l)
        {
            bounds = new Rect(-l.width / 2.0f, -l.height / 2.0f, l.width, l.height);
        }

        public Room(float x, float y, float w, float h)
        {
            bounds = new Rect(x, y, w, h);
        }

        public float Area()
        {
            var a = bounds.width * bounds.height;
            return a < 0 ? 0 : a;
        }

        public float Ratio()
        {
            return Math.Min(bounds.width / bounds.height, bounds.height / bounds.width);
        }

        public bool Intersect(Vector2 point)
        {
            return point.x > bounds.xMin && point.x < bounds.xMax &&
                   point.y > bounds.yMin && point.y < bounds.yMax;
        }
    }
}