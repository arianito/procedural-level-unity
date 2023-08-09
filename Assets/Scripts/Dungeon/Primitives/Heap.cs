using System;

namespace Dungeon
{
    // https://www.youtube.com/@SebastianLague
    // https://github.com/SebLague/Pathfinding

    public interface IHeapNode<T> : IComparable<T>
    {
        int HeapIndex { get; set; }
    }


    public class Heap<T> where T : IHeapNode<T>
    {
        private readonly T[] _heap;
        private int _length;

        public Heap(int maxHeapSize)
        {
            _heap = new T[maxHeapSize];
        }

        public bool Empty => _length == 0;

        public void Add(T item)
        {
            item.HeapIndex = _length;
            _heap[_length] = item;
            SortUp(item);
            _length++;
        }

        public T RemoveFirst()
        {
            var firstItem = _heap[0];
            _length--;
            _heap[0] = _heap[_length];
            _heap[0].HeapIndex = 0;
            SortDown(_heap[0]);
            return firstItem;
        }

        public void UpdateItem(T item)
        {
            SortUp(item);
        }

        public bool Contains(T item)
        {
            return item.Equals(_heap[item.HeapIndex]);
        }

        private void SortDown(T item)
        {
            while (true)
            {
                var leftChildIndex = item.HeapIndex * 2 + 1;
                var rightChildIndex = item.HeapIndex * 2 + 2;

                if (leftChildIndex >= _length)
                    return;

                var child = _heap[leftChildIndex];

                if (rightChildIndex < _length &&
                    _heap[rightChildIndex].CompareTo(_heap[leftChildIndex]) < 0)
                    child = _heap[rightChildIndex];

                if (item.CompareTo(child) <= 0)
                    return;

                Swap(item, child);
            }
        }

        private void SortUp(T item)
        {
            var parentIndex = (item.HeapIndex - 1) / 2;

            while (true)
            {
                var parentItem = _heap[parentIndex];

                if (parentItem.CompareTo(item) <= 0)
                    return;

                Swap(item, parentItem);
                parentIndex = (item.HeapIndex - 1) / 2;
            }
        }

        private void Swap(T itemA, T itemB)
        {
            (_heap[itemA.HeapIndex], _heap[itemB.HeapIndex]) =
                (_heap[itemB.HeapIndex], _heap[itemA.HeapIndex]);
            (itemA.HeapIndex, itemB.HeapIndex) =
                (itemB.HeapIndex, itemA.HeapIndex);
        }
    }
}