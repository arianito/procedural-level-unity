using System;
using UnityEngine;

namespace Dungeon
{

    [Serializable]
    public class LevelConfig
    {
        
        public int width = 100;
        public int height = 100;
        public int range = 100;
        public int count = 10;
        public int offset = 4;
        public float segments = 6;
        public int seed = 1;
        public int division = 1;
    }
}