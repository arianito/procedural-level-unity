using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Dungeon
{
    public class LevelGenerator : MonoBehaviour
    {
        public GameObject unitCube;
        public GameObject hallwayCube;
        public LevelConfig levelConfig;
        public int seed = 1;
        public int levels = 5;

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

        private void Generate()
        {
            try
            {
                Stopwatch sw = new Stopwatch();

                sw.Start();

                for (var i = 0; i < transform.childCount; i++)
                    Destroy(transform.GetChild(i).gameObject);

                for (var i = 0; i < levels; i++)
                {
                    var gm = new GameObject($"level{i}")
                    {
                        transform =
                        {
                            parent = transform,
                            position = Vector3.up * (i * 1.5f)
                        }
                    };
                    var pc = new ProceduralLevel(levelConfig, seed + i);
                    RenderRooms(pc, gm.transform);
                }


                sw.Stop();
            }
            catch
            {
                // ignored
            }
        }

        private void RenderRooms(ProceduralLevel pc, Transform superParent)
        {
            var hallwayId = 0;
            pc.finder?.ForEach(f =>
            {
                var parent = superParent;
                var hallwayIndex = hallwayId++;
                f.path?.ForEach(node =>
                {
                    var go = PlaceCube(
                        $"hallway{hallwayIndex++}",
                        hallwayCube,
                        parent,
                        Vector3.zero,
                        Vector3.one / f.meshGrid.scalar
                    );
                    go.position = superParent.position + new Vector3(node.position.x, 0, node.position.y);
                    if (parent != superParent) return;
                    hallwayIndex = 0;
                    parent = go;
                });
            });
            var roomIndex = 0;
            pc.rooms.ForEach(r =>
            {
                PlaceCube(
                    $"room{roomIndex++}",
                    unitCube,
                    superParent,
                    new Vector3(r.bounds.position.x, 0, r.bounds.position.y),
                    new Vector3(r.bounds.size.x, 1.5f, r.bounds.size.y)
                );
            });
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
    }
}