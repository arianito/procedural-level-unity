using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dungeon;
using UnityEngine;
using Random = System.Random;

namespace Dungeon
{
    public class LevelGen : MonoBehaviour
    {
        public GameObject unitCube;
        public GameObject hallwayCube;
        public GameObject stairsCube;

        public Vector3Int boundaries = new Vector3Int(10, 5, 10);
        public int seed = 1;
        public LevelConfig level;
        public bool useKruscals;

        private List<Edge> _graph;

        private List<List<MeshNode>> _hallways;

        private MeshGrid _meshGrid;
        private Random _random;
        private RoomGen _roomGen;
        private HashSet<Edge> _edges;


        private void Start()
        {
            Generate();
            StartCoroutine(Loop());
        }

        private void Update()
        {
            if (Input.GetButtonDown("Jump"))
            {
                seed++;
                Generate();
            }
        }

        private void Generate()
        {
            _random = new Random(seed);

            _roomGen = new RoomGen(boundaries, _random, level);
            
            if(_roomGen.Rooms.Count == 0)
                return;

            _meshGrid = new MeshGrid(_roomGen.BBox.Expand(2), _random);

            AStar.DefineMeshGrid(_meshGrid, _roomGen.Rooms);

            _edges = Triangulation.Triangulate(
                _roomGen.Rooms.Select(r => r.WorldPosition).ToList()
            );
            

            var startRoom = _roomGen.Rooms[0];

            _graph = useKruscals
                ? Triangulation.FindMinimumSpanningTreeKruscals(_edges)
                : Triangulation.MinimumSpanningTreePrim(_edges, startRoom.WorldPosition);

            foreach (var edge in _edges)
            {
                if (_random.NextDouble() < (level.loopChance / 100.0f))
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

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            //
            // if (_meshGrid != null)
            // {
            //     foreach (var node in _meshGrid.Nodes)
            //     {
            //         Gizmos.color = node.IsWalkable ? Color.gray : Color.red;
            //         node.DebugDraw(0.8f, !node.IsWalkable);
            //     }
            // }
            // Gizmos.color = Color.white;
            // if (_edges != null)
            // {
            //     foreach (var edge in _edges)
            //     {
            //         edge.DebugDraw();
            //     }
            // }

            if (_graph != null)
            {
                Gizmos.color = Color.red;
                foreach (var edge in _graph)
                {
                    edge.DebugDraw();
                }
            }
        }

        private void RenderRooms(Transform superParent)
        {
            var roomIndex = 0;
            foreach (var r in _roomGen.Rooms)
                if(_graph.Any(e => e.HasVertex(r.WorldPosition)))
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

                for (var i = 0; i < hallway.Count; i++)
                {
                    var meshNode = hallway[i];
                    
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