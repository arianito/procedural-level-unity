using System;
using UnityEngine;

namespace Dungeon
{
    public class CameraOrbit : MonoBehaviour
    {


        public float y = 10;
        public float distance = 20;
        
        
        private void Update()
        {
            var t = transform;
            t.position = new Vector3(Mathf.Cos(Time.time) * distance, y, Mathf.Sin(Time.time) * distance);
            t.rotation = Quaternion.LookRotation(Vector3.zero-t.position, Vector3.up);
        }
    }
}