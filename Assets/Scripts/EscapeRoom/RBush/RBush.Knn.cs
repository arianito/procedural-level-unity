using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RBush
{
    public static class RBushExtensions
    {
        public static IReadOnlyList<T> KNearestNeighbors<T>(
            this ISpatialIndex<T> tree,
            int k,
            float x,
            float y,
            float? maxDistance = null,
            Func<T, bool> predicate = null)
            where T : ISpatialData
        {
            var items = maxDistance == null
                ? tree.Search()
                : tree.Search(
                    new Envelope(
                        x - maxDistance.Value,
                        y - maxDistance.Value,
                        x + maxDistance.Value,
                        y + maxDistance.Value));

            var distances = items
                .Select(i => new { Item = i, Distance = i.Envelope.DistanceTo(x, y) })
                .OrderBy(i => i.Distance)
                .AsEnumerable();

            if (maxDistance.HasValue)
                distances = distances.TakeWhile(i => i.Distance <= maxDistance.Value);

            if (predicate != null)
                distances = distances.Where(i => predicate(i.Item));

            if (k > 0)
                distances = distances.Take(k);

            return distances
                .Select(i => i.Item)
                .ToList();
        }

        public static float DistanceTo(this Envelope envelope, float x, float y)
        {
            var dX = AxisDistance(x, envelope.MinX, envelope.MaxX);
            var dY = AxisDistance(y, envelope.MinY, envelope.MaxY);
            return Mathf.Sqrt((dX * dX) + (dY * dY));

            static float AxisDistance(float p, float min, float max) =>
                p < min ? min - p :
                p > max ? p - max :
                0;
        }
    }
}