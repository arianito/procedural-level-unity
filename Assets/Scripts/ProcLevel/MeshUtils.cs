using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dungeon
{
    public static class MeshUtils
    {
        
        public static List<Triangle> GetMesh(List<Room> rooms, float bboxOffset, float offset)
        {
            var verts = RoomUtils.GetGridVertices(rooms, bboxOffset, offset);
            var tri = Delaunay.Triangulate(verts);
            
            rooms.ForEach(room =>
            {
                tri.RemoveAll(t => room.bounds.Contains(t, offset));
            });
            return tri;
        }

    }
}