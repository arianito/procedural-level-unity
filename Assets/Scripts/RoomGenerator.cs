using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = System.Random;

namespace Dungeon
{
    public class RoomGenerator : MonoBehaviour
    {
        public GameObject unitCube;
        public Level level;
        public int seed = 1;

        private Random _rand;


        private Complex _complex;
        private Delaunay _trig;

        private void Start()
        {
            Generate();
        }

        private void Update()
        {
            if (Input.GetButtonDown("Jump"))
            {
                Stopwatch sw = new Stopwatch();

                sw.Start();
                Generate();
                sw.Stop();
                Debug.Log($"elapsed: {sw.ElapsedMilliseconds}ms");
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            _trig?.MST.ForEach(edge => edge.Debug(0));

            Gizmos.color = Color.black;

            _trig?.Outline.ForEach(edge => edge.Debug(1));
        }


        private void Generate()
        {
            _rand = new Random(seed);
            _complex = new Complex(level, seed);
            _complex.Generate();

            RenderRooms();

            _trig = new Delaunay(_complex.rooms);
            _trig.Triangulate();
        }


        private void RenderRooms()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            _complex.rooms.ForEach(r =>
                PlaceCube(r.bounds.position, r.bounds.size, 0));
        }

        private void PlaceCube(Vector2Int location, Vector2Int size, float z)
        {
            var go = Instantiate(
                unitCube,
                new Vector3(location.x, z, location.y),
                Quaternion.identity,
                transform
            );
            go.transform.localScale = new Vector3(size.x, 1 + (float)_rand.NextDouble(), size.y);
        }
    }
}