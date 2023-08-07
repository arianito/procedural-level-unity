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

        private ProceduralLevel _proceduralLevel;

        private void Start()
        {
            Generate();
            StartCoroutine(Loop());
        }

        private void Update()
        {
            if (Input.GetButtonDown("Jump"))
            {
                levelConfig.seed++;
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
                _proceduralLevel = new ProceduralLevel(levelConfig);
                _proceduralLevel.Generate();

                RenderRooms();
                sw.Stop();
            }
            catch
            {
                // ignored
            }
        }
        
        private void RenderRooms()
        {
            for (var i = 0; i < transform.childCount; i++)
                Destroy(transform.GetChild(i).gameObject);

            

            _proceduralLevel.finder?.ForEach(f =>
            {
                var parent = transform;
                f.path?.ForEach(p =>
                {
                    
                    var go = PlaceHallway(parent, p.position, Vector2.one / f.meshGrid.scalar);
                    if (parent == transform)
                    {
                        parent = go;
                    }
                });
            });
            _proceduralLevel.rooms.ForEach(r =>
            {
                PlaceCube(transform, r.bounds.position, r.bounds.size);
            });
            
        }

        private Transform PlaceCube(Transform parent, Vector2 location, Vector2 size)
        {
            var t = transform;
            var go = Instantiate(
                unitCube,
                t.position + new Vector3(location.x, 0, location.y),
                Quaternion.identity,
                parent
            );
            go.transform.localScale = new Vector3(size.x, 1.5f, size.y);
            return go.transform;
        }
        
        private Transform PlaceHallway(Transform parent, Vector2 location, Vector2 size)
        {
            var t = transform;
            var go = Instantiate(
                hallwayCube,
                t.position + new Vector3(location.x, 0, location.y),
                Quaternion.identity,
                parent
            );
            go.transform.localScale = new Vector3(size.x, 1, size.y);
            return go.transform;
        }
    }
}