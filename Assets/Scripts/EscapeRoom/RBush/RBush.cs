using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RBush
{
    public partial class RBush<T> where T : ISpatialData
    {
        private const int DefaultMaxEntries = 9;
        private const int MinimumMaxEntries = 4;
        private const int MinimumMinEntries = 2;
        private const float DefaultFillFactor = 0.4f;

        private readonly IEqualityComparer<T> _comparer;
        private readonly int _maxEntries;
        private readonly int _minEntries;

        public Node Root { get; private set; }

        public ref readonly Envelope Envelope => ref Root.Envelope;

        public RBush()
            : this(DefaultMaxEntries, EqualityComparer<T>.Default)
        {
        }

        public RBush(int maxEntries)
            : this(maxEntries, EqualityComparer<T>.Default)
        {
        }

        public RBush(int maxEntries, IEqualityComparer<T> comparer)
        {
            _comparer = comparer;
            _maxEntries = Mathf.Max(MinimumMaxEntries, maxEntries);
            _minEntries = Mathf.Max(MinimumMinEntries, (int)Mathf.Ceil(_maxEntries * DefaultFillFactor));

            Clear();
        }

        public int Count { get; private set; }

        public void Clear()
        {
            Root = new Node(new List<ISpatialData>(), 1);
            Count = 0;
        }

        public IReadOnlyList<T> Search() =>
            GetAllChildren(new List<T>(), Root);

        public IReadOnlyList<T> Search(in Envelope boundingBox) =>
            DoSearch(boundingBox);

        public bool Collides(in Envelope boundingBox) =>
            DoExists(boundingBox);

        public void Insert(T item)
        {
            Insert(item, Root.Height);
            Count++;
        }

        public void BulkLoad(IEnumerable<T> items)
        {
            var data = items.ToArray();
            if (data.Length == 0) return;

            if (Root.IsLeaf &&
                Root.Items.Count + data.Length < _maxEntries)
            {
                foreach (var i in data)
                    Insert(i);
                return;
            }

            if (data.Length < _minEntries)
            {
                foreach (var i in data)
                    Insert(i);
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

                Insert(dataRoot, Root.Height - dataRoot.Height);
            }
        }

        public bool Delete(T item) =>
            DoDelete(Root, item);

        private bool DoDelete(Node node, T item)
        {
            if (!node.Envelope.Contains(item.Envelope))
                return false;

            if (node.IsLeaf)
            {
                var cnt = node.Items.RemoveAll(i => _comparer.Equals((T)i, item));
                if (cnt != 0)
                {
                    Count -= cnt;
                    node.ResetEnvelope();
                    return true;
                }

                return false;
            }

            var flag = false;
            foreach (var spatialData in node.Items)
            {
                var n = (Node)spatialData;
                flag |= DoDelete(n, item);
            }

            if (flag)
                node.ResetEnvelope();
            return flag;
        }
    }
}