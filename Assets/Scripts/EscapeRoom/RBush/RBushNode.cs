using System.Collections.Generic;

namespace RTree
{
    public class RBushNode : ISpatialData
    {
        public int Height { get; }
        public bool IsLeaf => Height == 1;
        public BoundingBox BBox => _bbox;

        public List<ISpatialData> Items { get; }


        private BoundingBox _bbox;
        
        public RBushNode(List<ISpatialData> items, int height)
        {
            Height = height;
            Items = items;
            ResetBBox();
        }

        public void Add(ISpatialData node)
        {
            Items.Add(node);
            _bbox = BBox.Extend(node.BBox);
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
            _bbox = GetEnclosingBBox(Items);
        }
        
        
        public static BoundingBox GetEnclosingBBox(IEnumerable<ISpatialData> items)
        {
            var bbox = BoundingBox.EmptyBounds;
            foreach (var data in items)
            {
                bbox = bbox.Extend(data.BBox);
            }

            return bbox;
        }
    }
}