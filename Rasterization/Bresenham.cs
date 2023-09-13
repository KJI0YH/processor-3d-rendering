using Lab1.Primitives;
using System;
using System.Collections.Generic;

namespace Lab1.Rasterization
{
    public class Bresenham : IRasterization
    {
        public IEnumerable<Pixel> Rasterize(float xStart, float yStart, float xEnd, float yEnd)
        {
            bool reverce = false;
            if (Math.Abs(xEnd - xStart) < Math.Abs(yEnd - yStart))
            {
                float buffer = xStart;
                xStart = yStart;
                yStart = buffer;
                buffer = xEnd;
                xEnd = yEnd;
                yEnd = buffer;
                reverce = true;
            }

            int x = (int)MathF.Round(xStart);
            int y = (int)MathF.Round(yStart);
            float xDelta = Math.Abs(xEnd - xStart);
            float yDelta = Math.Abs(yEnd - yStart);
            int xChange = xEnd - xStart > 0 ? 1 : -1;
            int yChange = yEnd - yStart > 0 ? 1 : -1;
            float error = 0;

            while (x != (int)MathF.Round(xEnd))
            {
                if (reverce) yield return new Pixel(y, x);
                else yield return new Pixel(x, y);

                x += xChange;
                error += yDelta;
                if (2 * error > xDelta)
                {
                    y += yChange;
                    error -= xDelta;
                }

            }
        }
    }
}
