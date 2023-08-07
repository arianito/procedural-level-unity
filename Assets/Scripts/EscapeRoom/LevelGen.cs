using System.Collections;
using UnityEngine;
using Random = System.Random;

namespace EscapeRoom
{
    public class LevelGen : MonoBehaviour
    {
        public Vector3Int boundaries = new Vector3Int(10, 5, 10);
        public int seed = 1;
        public int offset = 1;

        private MeshGrid _meshGrid;
        private RoomGen _roomGen;
        private Random _random;

        private void Generate()
        {
            _random = new Random(seed);
            
            _roomGen = new RoomGen(boundaries, _random, offset);
            
            _meshGrid = new MeshGrid(boundaries);
        }

        private void OnDrawGizmos()
        {
            _meshGrid?.DebugDraw();
            _roomGen?.DebugDraw();
        }

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

        private IEnumerator Loop()
        {
            while (true)
            {
                yield return new WaitForSeconds(2);
                Generate();
            }
        }
    }
}