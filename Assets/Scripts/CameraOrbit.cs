using System;
using UnityEngine;

namespace Dungeon
{
    public class CameraOrbit : MonoBehaviour
    {
        public float y = 10;
        public float distance = 20;
        public float speed = 0.5f;
        
        
        private void Update()
        {
            var t = transform;
            t.position = new Vector3(Mathf.Cos(Time.time * speed) * distance, y, Mathf.Sin(Time.time * speed) * distance);
            t.rotation = Quaternion.LookRotation(Vector3.zero-t.position, Vector3.up);
        }
    }
}