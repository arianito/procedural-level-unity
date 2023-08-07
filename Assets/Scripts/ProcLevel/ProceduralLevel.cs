using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace Dungeon
{
    public class ProceduralLevel
    {
        public readonly LevelConfig config;
        public readonly Random random;
        public List<Room> rooms;

        public List<Edge> mst;
        public MeshGrid meshGrid;
        public List<PathFinder> finder;


        public ProceduralLevel(LevelConfig c)
        {
            config = c;
            random = new Random(c.seed);
        }

        public void Generate()
        {
            rooms = RoomUtils.Generate(random, config);

            RoomUtils.ReCenter(rooms);

            var triangles = Delaunay.Triangulate(rooms.Select(room => room.bounds.center).ToList());

            var edges = Delaunay.GetUniqueEdges(triangles);

            var (room1, room2) = RoomUtils.FindFurthestRooms(rooms);

            mst = Delaunay.MinimumSpanningTreePrim(edges, room1.bounds.center);

            triangles.ForEach(t => t.GetEdges().ForEach(e =>
            {
                if (random.NextDouble() > 0.05f) return;
                if (!mst.Contains(e))
                    mst.Add(e);
            }));

            meshGrid = new MeshGrid(rooms, random, config.division);
            finder = new List<PathFinder>();

            foreach (var edge in mst)
            {
                finder.Add(new PathFinder(
                    meshGrid,
                    RoomUtils.FindRoom(rooms, edge.A),
                    RoomUtils.FindRoom(rooms, edge.B),
                    random
                ));
            }
        }
    }
}