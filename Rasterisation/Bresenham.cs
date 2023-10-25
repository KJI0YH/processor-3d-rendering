using Rendering.Primitives;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Rendering.Rasterisation
{
    public class Bresenham : IRasterisation
    {
        public IEnumerable<Pixel> Rasterise(Vector2 start, Vector2 end)
        {
            int x1 = (int)Math.Floor(start.X);
            int y1 = (int)Math.Floor(start.Y);
            int x2 = (int)Math.Floor(end.X);
            int y2 = (int)Math.Floor(end.Y);

            int dx = x2 - x1;
            int dy = y2 - y1;

            int width = Math.Abs(dx);
            int height = Math.Abs(dy);

            // Rotation matrix
            int m11 = Math.Sign(dx);
            int m12 = 0;
            int m21 = 0;
            int m22 = Math.Sign(dy);

            if (width < height)
            {
                (m11, m12) = (m12, m11);
                (m21, m22) = (m22, m21);
            }

            int stepCount = Math.Max(width, height);
            int y = 0;
            int error = 0;
            int dError = 2 * int.Min(width, height);

            for (int x = 0; x <= stepCount; x++)
            {
                int xPixel = x1 + m11 * x + m12 * y;
                int yPixel = y1 + m21 * x + m22 * y;
                yield return new Pixel(xPixel, yPixel);
                if ((error += dError) > stepCount)
                {
                    error -= 2 * stepCount;
                    y++;
                }
            }
        }
    }
}