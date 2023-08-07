using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RTree
{
    public class RBush : RBush<BoundingBox>
    {
    }

    public class RBush<T> where T : ISpatialData
    {
        private RBushNode Root { get; set; }
        public int Count { get; private set; }

        
        public BoundingBox BBox => Root.BBox;

        private const int DefaultMaxEntries = 9;
        private const int MinimumMaxEntries = 4;
        private const int MinimumMinEntries = 2;
        private const float DefaultFillFactor = 0.4f;
        public const float Tolerance = 0.001f;

        private readonly int _maxEntries;
        private readonly int _minEntries;


        public RBush()
            : this(DefaultMaxEntries)
        {
        }

        public RBush(int maxEntries)
        {
            _maxEntries = Mathf.Max(MinimumMaxEntries, maxEntries);
            _minEntries = Mathf.Max(MinimumMinEntries, (int)Mathf.Ceil(_maxEntries * DefaultFillFactor));

            Clear();
        }

        public void Clear()
        {
            Root = new RBushNode(new List<ISpatialData>(), 1);
            Count = 0;
        }

        public IReadOnlyList<T> Search() =>
            GetAllChildren(new List<T>(), Root);

        public IReadOnlyList<T> Search(in BoundingBox boundingBox) =>
            DoSearch(boundingBox);

        public bool Collides(in BoundingBox boundingBox) =>
            DoExists(boundingBox);

        public void Add(T item)
        {
            InsertInternal(item, Root.Height);
            Count++;
        }

        public void AddRange(IEnumerable<T> items)
        {
            var data = items.ToArray();
            if (data.Length == 0) return;

            if (Root.IsLeaf &&
                Root.Items.Count + data.Length < _maxEntries)
            {
                foreach (var i in data)
                    Add(i);
                return;
            }

            if (data.Length < _minEntries)
            {
                foreach (var i in data)
                    Add(i);
                return;
            }

            var dataRoot = BuildTree(data);
            Count += data.Length;

            if (Root.Items.Count == 0)
                Root = dataRoot;
            else if (Root.Height == dataRoot.Height)
            {
                if (Root.Items.Count + dataRoot.Items.Count <= _maxEntries)
                {
                    foreach (var isd in dataRoot.Items)
                        Root.Add(isd);
                }
                else
                    SplitRoot(dataRoot);
            }
            else
            {
                if (Root.Height < dataRoot.Height)
                {
                    (Root, dataRoot) = (dataRoot, Root);
                }

                InsertInternal(dataRoot, Root.Height - dataRoot.Height);
            }
        }

        public bool Remove(T item) =>
            RemoveInternal(Root, item);

        private bool RemoveInternal(RBushNode node, T item)
        {
            if (!node.BBox.Contains(item.BBox))
                return false;

            if (node.IsLeaf)
            {
                var cnt = node.Items.RemoveAll(i => i.Equals(item));
                if (cnt != 0)
                {
                    Count -= cnt;
                    node.ResetBBox();
                    return true;
                }

                return false;
            }

            var flag = false;
            foreach (var spatialData in node.Items)
            {
                var n = (RBushNode)spatialData;
                flag |= RemoveInternal(n, item);
            }

            if (flag)
                node.ResetBBox();
            return flag;
        }

        private readonly IComparer<ISpatialData> _sCompareMinX =
            Comparer<ISpatialData>.Create((x, y) => Comparer<float>.Default.Compare(x.BBox.MinX, y.BBox.MinX));

        private readonly IComparer<ISpatialData> _sCompareMinY =
            Comparer<ISpatialData>.Create((x, y) => Comparer<float>.Default.Compare(x.BBox.MinY, y.BBox.MinY));


        private List<T> DoSearch(in BoundingBox boundingBox)
        {
            if (!Root.BBox.Intersects(boundingBox))
                return new List<T>();

            var intersections = new List<T>();
            var queue = new Queue<RBushNode>();
            queue.Enqueue(Root);

            while (queue.Count != 0)
            {
                var item = queue.Dequeue();

                if (item.IsLeaf)
                {
                    foreach (var i in item.Items)
                    {
                        if (i.BBox.Intersects(boundingBox))
                            intersections.Add((T)i);
                    }
                }
                else
                {
                    foreach (var i in item.Items)
                    {
                        if (i.BBox.Intersects(boundingBox))
                            queue.Enqueue((RBushNode)i);
                    }
                }
            }

            return intersections;
        }


        private bool DoExists(in BoundingBox boundingBox)
        {
            if (!Root.BBox.Intersects(boundingBox))
                return false;

            var queue = new Queue<RBushNode>();
            queue.Enqueue(Root);

            while (queue.Count != 0)
            {
                var item = queue.Dequeue();

                if (item.IsLeaf)
                {
                    foreach (var i in item.Items)
                    {
                        if (i.BBox.Intersects(boundingBox))
                            return true;
                    }
                }
                else
                {
                    foreach (var i in item.Items)
                    {
                        if (i.BBox.Intersects(boundingBox))
                            queue.Enqueue((RBushNode)i);
                    }
                }
            }

            return false;
        }


        private List<RBushNode> FindCoveringArea(in BoundingBox area, int depth)
        {
            var path = new List<RBushNode>();
            var node = this.Root;

            while (true)
            {
                path.Add(node);
                if (node.IsLeaf || path.Count == depth) return path;

                var next = node.Items[0];
                var nextArea = next.BBox.Extend(area).Area;

                foreach (var i in node.Items)
                {
                    var newArea = i.BBox.Extend(area).Area;
                    if (newArea > nextArea)
                        continue;

                    if (Math.Abs(newArea - nextArea) < Tolerance
                        && i.BBox.Area >= next.BBox.Area)
                        continue;

                    next = i;
                    nextArea = newArea;
                }

                node = (next as RBushNode)!;
            }
        }

        private void InsertInternal(ISpatialData data, int depth)
        {
            var path = FindCoveringArea(data.BBox, depth);

            var insertNode = path[path.Count - 1];
            insertNode.Add(data);

            while (--depth >= 0)
            {
                if (path[depth].Items.Count > _maxEntries)
                {
                    var newNode = SplitNode(path[depth]);
                    if (depth == 0)
                        SplitRoot(newNode);
                    else
                        path[depth - 1].Add(newNode);
                }
                else
                    path[depth].ResetBBox();
            }
        }


        private void SplitRoot(RBushNode newNode) =>
            this.Root = new RBushNode(new List<ISpatialData> { this.Root, newNode }, this.Root.Height + 1);

        private RBushNode SplitNode(RBushNode node)
        {
            SortChildren(node);

            var splitPoint = GetBestSplitIndex(node.Items);
            var newChildren = node.Items.Skip(splitPoint).ToList();
            node.RemoveRange(splitPoint, node.Items.Count - splitPoint);
            return new RBushNode(newChildren, node.Height);
        }


        private void SortChildren(RBushNode node)
        {
            node.Items.Sort(_sCompareMinX);
            var splitsByX = GetPotentialSplitMargins(node.Items);
            node.Items.Sort(_sCompareMinY);
            var splitsByY = GetPotentialSplitMargins(node.Items);

            if (splitsByX < splitsByY)
                node.Items.Sort(_sCompareMinX);
        }

        private float GetPotentialSplitMargins(List<ISpatialData> children) =>
            GetPotentialEnclosingMargins(children) +
            GetPotentialEnclosingMargins(children.AsEnumerable().Reverse().ToList());

        private float GetPotentialEnclosingMargins(List<ISpatialData> children)
        {
            var bbox = BoundingBox.EmptyBounds;
            var i = 0;
            for (; i < _minEntries; i++)
            {
                bbox = bbox.Extend(children[i].BBox);
            }

            var totalMargin = bbox.Margin;
            for (; i < children.Count - _minEntries; i++)
            {
                bbox = bbox.Extend(children[i].BBox);
                totalMargin += bbox.Margin;
            }

            return totalMargin;
        }


        private int GetBestSplitIndex(List<ISpatialData> children)
        {
            return Enumerable.Range(_minEntries, children.Count - _minEntries)
                .Select(i =>
                {
                    var leftBBox = RBushNode.GetEnclosingBBox(children.Take(i));
                    var rightBBox = RBushNode.GetEnclosingBBox(children.Skip(i));

                    var overlap = leftBBox.Intersection(rightBBox).Area;
                    var totalArea = leftBBox.Area + rightBBox.Area;
                    return new { i, overlap, totalArea };
                })
                .OrderBy(x => x.overlap)
                .ThenBy(x => x.totalArea)
                .Select(x => x.i)
                .First();
        }


        private RBushNode BuildTree(T[] data)
        {
            var treeHeight = GetDepth(data.Length);
            var rootMaxEntries = (int)Mathf.Ceil(data.Length / Mathf.Pow(_maxEntries, treeHeight - 1));
            return BuildNodes(new ArraySegment<T>(data), treeHeight, rootMaxEntries);
        }

        private int GetDepth(int numNodes) =>
            (int)Math.Ceiling(Math.Log(numNodes) / Math.Log(_maxEntries));

        private RBushNode BuildNodes(ArraySegment<T> data, int height, int maxEntries)
        {
            if (data.Count <= maxEntries)
            {
                return height == 1
                    ? new RBushNode(data.Cast<ISpatialData>().ToList(), height)
                    : new RBushNode(
                        new List<ISpatialData>
                        {
                            BuildNodes(data, height - 1, _maxEntries),
                        },
                        height);
            }

            // after much testing, this is faster than using Array.Sort() on the provided array
            // in spite of the additional memory cost and copying. go figure!
            var byX = new ArraySegment<T>(data.OrderBy(i => i.BBox.MinX).ToArray());

            var nodeSize = (data.Count + (maxEntries - 1)) / maxEntries;
            var subSortLength = nodeSize * (int)Math.Ceiling(Math.Sqrt(maxEntries));

            var children = new List<ISpatialData>(maxEntries);
            foreach (var subData in Chunk(byX, subSortLength))
            {
                var byY = new ArraySegment<T>(subData.OrderBy(d => d.BBox.MinY).ToArray());

                foreach (var nodeData in Chunk(byY, nodeSize))
                {
                    children.Add(BuildNodes(nodeData, height - 1, _maxEntries));
                }
            }

            return new RBushNode(children, height);
        }

        private static IEnumerable<ArraySegment<T>> Chunk(ArraySegment<T> values, int chunkSize)
        {
            var start = 0;
            while (start < values.Count)
            {
                var len = Math.Min(values.Count - start, chunkSize);
                yield return new ArraySegment<T>(values.Array!, values.Offset + start, len);
                start += chunkSize;
            }
        }

        private List<T> GetAllChildren(List<T> list, RBushNode n)
        {
            if (n.IsLeaf)
            {
                list.AddRange(
                    n.Items.Cast<T>());
            }
            else
            {
                foreach (var node in n.Items.Cast<RBushNode>())
                    GetAllChildren(list, node);
            }

            return list;
        }
    }
}