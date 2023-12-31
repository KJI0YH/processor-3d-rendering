﻿using Rendering.Primitives;
using System.Collections.Generic;
using System.Numerics;

namespace Rendering.Rasterisation
{
    public interface IRasterisation
    {
        public IEnumerable<Pixel> Rasterise(Vector2 start, Vector2 end);
    }
}
