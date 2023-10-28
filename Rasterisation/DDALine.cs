using Rendering.Primitives;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Rendering.Rasterisation;

public class DDALine : IRasterisation
{
    public IEnumerable<Pixel> Rasterise(Vector2 start, Vector2 end)
    {
        var xDelta = end.X - start.X;
        var yDelta = end.Y - start.Y;
        var steps = (int)MathF.Round(MathF.MaxMagnitude(Math.Abs(xDelta), Math.Abs(yDelta)));

        var xStep = xDelta / steps;
        var yStep = yDelta / steps;

        for (var step = 0; step < steps; step++)
        {
            yield return new Pixel((int)MathF.Round(start.X), (int)MathF.Round(start.Y));
            start.X += xStep;
            start.Y += yStep;
        }
    }

    public override string ToString()
    {
        return "DDA line";
    }
}