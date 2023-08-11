// Credits: https://github.com/viceroypenguin/RBush

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dungeon
{
    public class RBush : RBush<BoundingBox>
    {
    }

    public class RBush<T> where T : ISpatialData
    {
        private const int DefaultMaxEntries = 9;
        private const int MinimumMaxEntries = 4;
        private const int MinimumMinEntries = 2;
        private const float DefaultFillFactor = 0.4f;
        public const float Tolerance = 0.001f;

        private readonly int _maxEntries;
        private readonly int _minEntries;

        private readonly IComparer<ISpatialData> _sCompareMinX =
            Comparer<ISpatialData>.Create((x, y) => Comparer<float>.Default.Compare(x.BBox.Min.x, y.BBox.Min.x));

        private readonly IComparer<ISpatialData> _sCompareMinY =
            Comparer<ISpatialData>.Create((x, y) => Comparer<float>.Default.Compare(x.BBox.Min.y, y.BBox.Min.y));

        private readonly IComparer<ISpatialData> _sCompareMinZ =
            Comparer<ISpatialData>.Create((x, y) => Comparer<float>.Default.Compare(x.BBox.Min.z, y.BBox.Min.z));

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

        private RBushNode Root { get; set; }
        public int Count { get; private set; }


        public BoundingBox BBox => Root.BBox;

        public void Clear()
        {
            Root = new RBushNode(new List<ISpatialData>(), 1);
            Count = 0;
        }

        public IReadOnlyList<T> Search()
        {
            return GetAllChildren(new List<T>(), Root);
        }

        public IReadOnlyList<T> Search(BoundingBox boundingBox)
        {
            return DoSearch(boundingBox);
        }

        public bool Collides(BoundingBox boundingBox)
        {
            return DoExists(boundingBox);
        }

        public void Add(T item)
        {
            InsertInternal(item, Root.Height);
            Count++;
        }

        public bool Remove(T item)
        {
            return RemoveInternal(Root, item);
        }

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


        private List<T> DoSearch(BoundingBox boundingBox)
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
                        if (i.BBox.Intersects(boundingBox))
                            intersections.Add((T)i);
                }
                else
                {
                    foreach (var i in item.Items)
                        if (i.BBox.Intersects(boundingBox))
                            queue.Enqueue((RBushNode)i);
                }
            }

            return intersections;
        }


        private bool DoExists(BoundingBox boundingBox)
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
                        if (i.BBox.Intersects(boundingBox))
                            return true;
                }
                else
                {
                    foreach (var i in item.Items)
                        if (i.BBox.Intersects(boundingBox))
                            queue.Enqueue((RBushNode)i);
                }
            }

            return false;
        }


        private List<RBushNode> FindCoveringArea(BoundingBox area, int depth)
        {
            var path = new List<RBushNode>();
            var node = Root;

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
                if (path[depth].Items.Count > _maxEntries)
                {
                    var newNode = SplitNode(path[depth]);
                    if (depth == 0)
                        SplitRoot(newNode);
                    else
                        path[depth - 1].Add(newNode);
                }
                else
                {
                    path[depth].ResetBBox();
                }
        }


        private void SplitRoot(RBushNode newNode)
        {
            Root = new RBushNode(new List<ISpatialData> { Root, newNode }, Root.Height + 1);
        }

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
            node.Items.Sort(_sCompareMinZ);
            var splitsByZ = GetPotentialSplitMargins(node.Items);
            
            if (splitsByZ < splitsByX && splitsByZ < splitsByY)
            {
                return;
            }
            
            if (splitsByX < splitsByY && splitsByX < splitsByZ)
            {
                node.Items.Sort(_sCompareMinX);
            }
            else if (splitsByY < splitsByX && splitsByY < splitsByZ)
            {
                node.Items.Sort(_sCompareMinY);
            }
        }

        private float GetPotentialSplitMargins(List<ISpatialData> children)
        {
            return GetPotentialEnclosingMargins(children) +
                   GetPotentialEnclosingMargins(children.AsEnumerable().Reverse().ToList());
        }

        private float GetPotentialEnclosingMargins(List<ISpatialData> children)
        {
            var bbox = BoundingBox.EmptyBounds;
            var i = 0;
            for (; i < _minEntries; i++) bbox = bbox.Extend(children[i].BBox);

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
                list.AddRange(
                    n.Items.Cast<T>());
            else
                foreach (var node in n.Items.Cast<RBushNode>())
                    GetAllChildren(list, node);

            return list;
        }
    }
}