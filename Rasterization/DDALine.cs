using Lab1.Primitives;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Lab1.Rasterization
{
    public class DDALine : IRasterization
    {
        public IEnumerable<Pixel> Rasterize(float xStart, float yStart, float xEnd, float yEnd)
        {
            float xDelta = xEnd - xStart;
            float yDelta = yEnd - yStart;
            int steps = (int)MathF.Round(MathF.MaxMagnitude(Math.Abs(xDelta), Math.Abs(yDelta)));

            float xStep = xDelta / steps;
            float yStep = yDelta / steps;

            // Return initial pixel
            yield return new Pixel((int)MathF.Round(xStart), (int)MathF.Round(yStart));

            // Return the next pixels
            for (int step = 1; step < steps; step++)
            {
                xStart += xStep;
                yStart += yStep;
                yield return new Pixel((int)MathF.Round(xStart), (int)MathF.Round(yStart));
            }
        }

        public IEnumerable<Pixel> Rasterize(Vector4 start, Vector4 end)
        {
            float xDelta = end.X - start.X;
            float yDelta = end.Y - start.Y;
            float zDelta = end.Z - start.Z;
            int steps = (int)MathF.Round(MathF.MaxMagnitude(Math.Abs(xDelta), Math.Abs(yDelta)));
            float xStep = xDelta / steps;
            float yStep = yDelta / steps;
            float zStep = zDelta / MathF.MaxMagnitude(xDelta, yDelta);

            // Return initial pixel
            yield return new Pixel((int)MathF.Round(start.X), (int)MathF.Round(start.Y), start.Z);

            // Return the next pixels
            for (int step = 1; step < steps; step++)
            {
                start.X += xStep;
                start.Y += yStep;
                start.Z += zStep;
                yield return new Pixel((int)MathF.Round(start.X), (int)MathF.Round(start.Y), start.Z);
            }
        }
    }
}
