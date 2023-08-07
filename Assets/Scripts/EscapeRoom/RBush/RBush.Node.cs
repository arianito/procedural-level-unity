using System.Collections.Generic;

namespace RBush
{
    public partial class RBush<T>
    {
        public class Node : ISpatialData
        {
            private Envelope _envelope;

            internal Node(List<ISpatialData> items, int height)
            {
                this.Height = height;
                this.Items = items;
                ResetEnvelope();
            }

            internal void Add(ISpatialData node)
            {
                Items.Add(node);
                _envelope = Envelope.Extend(node.Envelope);
            }

            internal void Remove(ISpatialData node)
            {
                Items.Remove(node);
                ResetEnvelope();
            }

            internal void RemoveRange(int index, int count)
            {
                Items.RemoveRange(index, count);
                ResetEnvelope();
            }

            internal void ResetEnvelope()
            {
                _envelope = GetEnclosingEnvelope(Items);
            }

            internal readonly List<ISpatialData> Items;

            public IReadOnlyList<ISpatialData> Children => Items;
            public int Height { get; }
            public bool IsLeaf => Height == 1;
            public ref readonly Envelope Envelope => ref _envelope;
        }
    }
}