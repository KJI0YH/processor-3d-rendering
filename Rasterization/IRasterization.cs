using Lab1.Primitives;
using System.Collections.Generic;
using System.Numerics;

namespace Lab1.Rasterization
{
    public interface IRasterization
    {
        public IEnumerable<Pixel> Rasterize(float xStart, float yStart, float xEnd, float yEnd);
        public IEnumerable<Pixel> Rasterize(Vector4 start, Vector4 end);
    }
}
