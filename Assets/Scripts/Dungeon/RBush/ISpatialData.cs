// Credits: https://github.com/viceroypenguin/RBush

namespace RTree
{
    public interface ISpatialData
    {
        BoundingBox BBox { get; }
    }
}