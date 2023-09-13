using Lab1.Primitives;
using System.Collections.Generic;

namespace Lab1.Rasterization
{
    public interface IRasterization
    {
        public IEnumerable<Pixel> Rasterize(float xStart, float yStart, float xEnd, float yEnd);
    }
}
