using Rendering.Primitives;
using System.Collections.Generic;
using System.Numerics;

namespace Rendering.Rasterization
{
    public interface IRasterization
    {
        public IEnumerable<Pixel> Rasterize(float xStart, float yStart, float xEnd, float yEnd);
        public IEnumerable<Pixel> Rasterize(Vector3 start, Vector3 end);
    }
}
