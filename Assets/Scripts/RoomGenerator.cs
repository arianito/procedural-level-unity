using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
            StartCoroutine(Loop());
        }

        private void OnDrawGizmos()
        {
            var i = 0;
            _trig?.MST.ForEach(
                edge =>
                {
                    edge.Debug(1 + (i++) / (float)level.count / 2.0f);
                }
            );
        }


        private IEnumerator Loop()
        {
            while (true)
            {
                _rand = new Random(seed);
                _complex = new Complex(level, seed ++);
                _complex.Generate();

                RenderRooms();

                _trig = new Delaunay(_complex.rooms);
                _trig.Triangulate();

                yield return new WaitForSeconds(1.0f);
            }
        }


        private void PlaceCube(Vector2Int location, Vector2Int size, float z, Color color)
        {
            GameObject go =
                Instantiate<GameObject>(unitCube, new Vector3(location.x, z, location.y), Quaternion.identity,
                    transform);
            go.GetComponent<Transform>().localScale = new Vector3(size.x,  1 + (int)_rand.NextDouble(), size.y);
            go.GetComponent<MeshRenderer>().material.color = color;
        }

        private void RenderRooms()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            var max = _complex.rooms.Max(r => r.bounds.center.magnitude);

            _complex.rooms.ForEach(r =>
                PlaceCube(r.bounds.position, r.bounds.size, 0,
                    Color.Lerp(Color.red, Color.blue,
                        Mathf.Clamp01(r.bounds.center.magnitude / max))));
        }
    }
}