﻿using System.Collections.Generic;

namespace RBush
{
    public interface ISpatialDatabase<T> : ISpatialIndex<T>
    {
        void Insert(T item);
        bool Delete(T item);
        void Clear();
        void BulkLoad(IEnumerable<T> items);
    }
}