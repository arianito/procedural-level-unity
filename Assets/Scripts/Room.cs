using System;
using UnityEngine;

namespace Dungeon
{

    [Serializable]
    public class Room
    {
        public RectInt bounds;

        public Room(Level l)
        {
            bounds = new RectInt(-l.width / 2, -l.height / 2, l.width, l.height);
        }

        public Room(int x, int y, int w, int h)
        {
            bounds = new RectInt(x, y, w, h);
        }

        public int Area()
        {
            var a = bounds.width * bounds.height;
            return a < 0 ? 0 : a;
        }

        public float Ratio()
        {
            return Math.Min((float)bounds.width / bounds.height, (float)bounds.height / bounds.width);
        }
    }
}