using System;
using UnityEngine;

namespace Dungeon
{

    [Serializable]
    public class Level
    {
        public RectInt bounds;
        public int range = 10;
        public int count = 8;
        public int offset = 1;
        public float segments = 10.0f;
    }
}