using Lab1.Primitives;
using System;
using System.Collections.Generic;

namespace Lab1.Rasterization
{
    public class DDALine : IRasterization
    {
        public IEnumerable<Pixel> Rasterize(float xStart, float yStart, float xEnd, float yEnd)
        {
            float xDelta = xEnd - xStart;
            float yDelta = yEnd - yStart;
            int steps = (int)MathF.Round(MathF.MaxMagnitude(Math.Abs(xDelta), Math.Abs(yDelta)));

            // Return initial pixel
            yield return new Pixel((int)MathF.Round(xStart), (int)MathF.Round(yStart));

            // Return the next pixels
            for (int step = 1; step < steps; step++)
            {
                xStart += xDelta / steps;
                yStart += yDelta / steps;
                yield return new Pixel((int)MathF.Round(xStart), (int)MathF.Round(yStart));
            }
        }
    }
}
