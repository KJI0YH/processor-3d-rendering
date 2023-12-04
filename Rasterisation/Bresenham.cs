using Rendering.Primitives;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Rendering.Rasterisation;

public class Bresenham : IRasterisation
{
    public IEnumerable<Pixel> Rasterise(Vector2 start, Vector2 end)
    {
        var x1 = (int)Math.Floor(start.X);
        var y1 = (int)Math.Floor(start.Y);
        var x2 = (int)Math.Floor(end.X);
        var y2 = (int)Math.Floor(end.Y);

        var dx = x2 - x1;
        var dy = y2 - y1;

        var width = Math.Abs(dx);
        var height = Math.Abs(dy);

        // Rotation matrix
        var m11 = Math.Sign(dx);
        var m12 = 0;
        var m21 = 0;
        var m22 = Math.Sign(dy);

        if (width < height)
        {
            (m11, m12) = (m12, m11);
            (m21, m22) = (m22, m21);
        }

        var stepCount = Math.Max(width, height);
        var y = 0;
        var error = 0;
        var dError = 2 * int.Min(width, height);


        for (var x = 0; x <= stepCount; x++)
        {
            var xPixel = x1 + m11 * x + m12 * y;
            var yPixel = y1 + m21 * x + m22 * y;
            yield return new Pixel(xPixel, yPixel);
            if ((error += dError) > stepCount)
            {
                error -= 2 * stepCount;
                y++;
            }
        }
    }

    public override string ToString()
    {
        return "Bresenham";
    }
}