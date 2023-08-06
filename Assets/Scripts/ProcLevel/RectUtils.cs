using System.Linq;
using UnityEngine;

namespace Dungeon
{
    public static class RectUtils
    {
        public static Vector2[] GetVertices(this Rect bounds, float offset, int subW, int subH)
        {
            var pw = bounds.width / subW;
            var ph = bounds.height / subH;
            var n = (subW + subH) * 2;
            var arr = new Vector2[n];
            var k = 0;
            arr[k++] = bounds.position + new Vector2(-offset, -offset);
            arr[k++] = bounds.position + new Vector2(bounds.width + offset, -offset);
            arr[k++] = bounds.position + new Vector2(bounds.width + offset, bounds.height + offset);
            arr[k++] = bounds.position + new Vector2(-offset, bounds.height + offset);

            for (var i = 1; i < subW; i++)
            {
                arr[k++] = bounds.position + new Vector2(i * pw, -offset);
                arr[k++] = bounds.position + new Vector2(i * pw, bounds.height + offset);
            }

            for (var i = 1; i < subH; i++)
            {
                arr[k++] = bounds.position + new Vector2(-offset, i * ph);
                arr[k++] = bounds.position + new Vector2(bounds.width + offset, i * ph);
            }

            return arr;
        }


        public static bool Contains(this Rect rect, Triangle t, float offset)
        {
            var verts = rect.GetVertices(offset, 1, 1);
            return verts.Contains(t.A) && verts.Contains(t.B) && verts.Contains(t.C);
        }
    }
}