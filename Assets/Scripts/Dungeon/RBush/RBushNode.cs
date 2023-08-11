// Credits: https://github.com/viceroypenguin/RBush

using System.Collections.Generic;

namespace Dungeon
{
    public class RBushNode : ISpatialData
    {
        public RBushNode(List<ISpatialData> items, int height)
        {
            Height = height;
            Items = items;
            ResetBBox();
        }

        public int Height { get; }
        public bool IsLeaf => Height == 1;

        public List<ISpatialData> Items { get; }
        public BoundingBox BBox { get; private set; }

        public void Add(ISpatialData node)
        {
            Items.Add(node);
            BBox = BBox.Extend(node.BBox);
        }

        public void Remove(ISpatialData node)
        {
            Items.Remove(node);
            ResetBBox();
        }

        public void RemoveRange(int index, int count)
        {
            Items.RemoveRange(index, count);
            ResetBBox();
        }

        public void ResetBBox()
        {
            BBox = GetEnclosingBBox(Items);
        }


        public static BoundingBox GetEnclosingBBox(IEnumerable<ISpatialData> items)
        {
            var bbox = BoundingBox.EmptyBounds;
            foreach (var data in items) bbox = bbox.Extend(data.BBox);

            return bbox;
        }
    }
}