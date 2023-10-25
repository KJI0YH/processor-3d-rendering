using Rendering.Primitives;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Rendering.Rasterisation
{
    public class DDALine : IRasterisation
    {
        public IEnumerable<Pixel> Rasterise(Vector2 start, Vector2 end)
        {
            float xDelta = end.X - start.X;
            float yDelta = end.Y - start.Y;
            int steps = (int)MathF.Round(MathF.MaxMagnitude(Math.Abs(xDelta), Math.Abs(yDelta)));

            float xStep = xDelta / steps;
            float yStep = yDelta / steps;

            for (int step = 0; step < steps; step++)
            {
                yield return new Pixel((int)MathF.Round(start.X), (int)MathF.Round(start.Y));
                start.X += xStep;
                start.Y += yStep;
            }
        }
    }
}
