using Lab1.Primitives;
using System;
using System.Collections.Generic;

namespace Lab1.Rasterization
{
    public class Bresenham : IRasterization
    {
        public IEnumerable<Pixel> Rasterize(float x1, float y1, float x2, float y2)
        {
            bool reverce = false;
            if (Math.Abs(x2 - x1) < Math.Abs(y2 - y1))
            {
                float buffer = x1;
                x1 = y1;
                y1 = buffer;
                buffer = x2;
                x2 = y2;
                y2 = buffer;
                reverce = true;
            }

            int x = (int)MathF.Round(x1);
            int y = (int)MathF.Round(y1);
            int xEnd = (int)MathF.Round(x2);
            int xDelta = (int)Math.Abs(x2 - x1);
            int yDelta = (int)Math.Abs(y2 - y1);
            int xChange = x2 - x1 > 0 ? 1 : -1;
            int yChange = y2 - y1 > 0 ? 1 : -1;
            int error = 0;

            do
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
            } while (x * xChange < xEnd * xChange);
        }
    }
}
