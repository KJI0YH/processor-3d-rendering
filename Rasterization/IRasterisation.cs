using Rendering.Primitives;
using System.Collections.Generic;
using System.Numerics;

namespace Rendering.Rasterization
{
    public interface IRasterisation
    {
        public IEnumerable<Pixel> Rasterise(Vector2 start, Vector2 end);
        public IEnumerable<Pixel> Rasterise(Vector3 start, Vector3 end);
    }
}
