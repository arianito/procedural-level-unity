using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace Dungeon
{
    public class LevelGen : MonoBehaviour
    {
        public GameObject unitCube;
        public GameObject hallwayCube;

        public Vector3Int boundaries = new Vector3Int(10, 5, 10);
        public int seed = 1;
        public LevelConfig level;
        public bool useKruscals;

        private List<Edge> _graph;

        private List<List<MeshNode>> _hallways;

        private MeshGrid _meshGrid;
        private Random _random;
        private RoomGen _roomGen;


        private void Start()
        {
            Generate();
            StartCoroutine(Loop());
        }

        private void Update()
        {
            if (Input.GetButtonDown("Jump"))
            {
                // seed++;
                Generate();
            }
        }

        private void Generate()
        {
            _random = new Random(seed);

            _roomGen = new RoomGen(boundaries, _random, level);

            _meshGrid = new MeshGrid(_roomGen.BBox, _random);

            AStar.DefineMeshGrid(_meshGrid, _roomGen.Rooms);
            
            var edges = Triangulation.Triangulate(
                _roomGen.Rooms.Select(r => r.Center).ToList()
            );

            var (room1, room2) = _roomGen.FindFurthestRooms();

            _graph = useKruscals
                ? Triangulation.FindMinimumSpanningTreeKruscals(edges)
                : Triangulation.MinimumSpanningTreePrim(edges, room1.Center);
            
            foreach (var edge in edges)
            {
                if(_random.NextDouble() < (level.loopChance / 100.0f))
                    if (!_graph.Contains(edge))
                        _graph.Add(edge);
            }

            _hallways = new List<List<MeshNode>>();
            foreach (var edge in _graph)
            {
                var roomA = _roomGen.GetRoom(edge.A);
                var roomB = _roomGen.GetRoom(edge.B);
                if (roomA == null || roomB == null) continue;

                var path = AStar.FindPath(_meshGrid, roomA, roomB);
                if (path.Count > 0) _hallways.Add(path);
            }


            for (var i = 0; i < transform.childCount; i++)
                Destroy(transform.GetChild(i).gameObject);

            RenderRooms(transform);
        }


        private void RenderRooms(Transform superParent)
        {
            var roomIndex = 0;
            foreach (var r in _roomGen.Rooms)
                PlaceCube(
                    $"room{roomIndex++}",
                    unitCube,
                    superParent,
                    new Vector3(r.WorldPosition.x, r.WorldPosition.y, r.WorldPosition.z),
                    new Vector3(r.WorldScale.x, r.WorldScale.y, r.WorldScale.z)
                );

            var hallwayId = 0;
            foreach (var hallway in _hallways)
            {
                var parent = superParent;
                var hallwayIndex = hallwayId++;
                foreach (var meshNode in hallway)
                {
                    var go = PlaceCube(
                        $"hallway{hallwayIndex++}",
                        hallwayCube,
                        parent,
                        Vector3.zero,
                        meshNode.WorldScale
                    );
                    go.position = superParent.position + meshNode.WorldPosition;
                    if (parent != superParent) continue;
                    hallwayIndex = 0;
                    parent = go;
                }
            }
        }

        private Transform PlaceCube(
            string objName,
            GameObject instance,
            Transform parent,
            Vector3 location,
            Vector3 size)
        {
            var go = Instantiate(
                instance,
                parent.position + location,
                Quaternion.identity,
                parent
            );
            go.name = objName;
            go.transform.localScale = size;
            return go.transform;
        }

        private IEnumerator Loop()
        {
            while (true)
            {
                yield return new WaitForSeconds(5);
                Generate();
            }
        }
    }
}