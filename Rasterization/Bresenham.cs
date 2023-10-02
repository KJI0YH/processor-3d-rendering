using Lab1.Primitives;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Lab1.Rasterization
{
    public class Bresenham : IRasterization
    {
        public IEnumerable<Pixel> Rasterize(float x1, float y1, float x2, float y2)
        {
            bool reverce = false;
            if (Math.Abs(x2 - x1) < Math.Abs(y2 - y1))
            {
                (x1, y1) = (y1, x1);
                (x2, y2) = (y2, x2);
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

        public IEnumerable<Pixel> Rasterize(Vector4 start, Vector4 end)
        {
            bool reverce = false;
            if (Math.Abs(end.X - start.X) < Math.Abs(end.Y - start.Y))
            {
                (start.X, start.Y) = (start.Y, start.X);
                (end.X, end.Y) = (end.Y, end.X);
                reverce = true;
            }

            int x = (int)MathF.Round(start.X);
            int y = (int)MathF.Round(start.Y);
            float z = start.Z;
            int xEnd = (int)MathF.Round(end.X);
            int xDelta = (int)Math.Abs(end.X - start.X);
            int yDelta = (int)Math.Abs(end.Y - start.Y);
            float dxz = (end.Z - start.Z) / (end.X - start.X);
            float dyz = (end.Z - start.Z) / (end.Y - start.Y);
            int xChange = end.X - start.X > 0 ? 1 : -1;
            int yChange = end.Y - start.Y > 0 ? 1 : -1;
            int error = 0;

            do
            {
                if (reverce) yield return new Pixel(y, x, z);
                else yield return new Pixel(x, y, z);

                x += xChange;

                if (reverce) z += dyz;
                else z += dxz;
                error += yDelta;
                if (2 * error > xDelta)
                {
                    y += yChange;

                    if (reverce) z += dxz;
                    else z += dyz;
                    error -= xDelta;
                }
            } while (x * xChange < xEnd * xChange);
        }
    }
}
