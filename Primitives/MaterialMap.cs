using System;
using System.Numerics;

namespace Rendering.Primitives;

public class MaterialMap
{
    private readonly Vector3[,] _values;
    private readonly int _width;
    private readonly int _height;

    public MaterialMap(Vector3[,] values)
    {
        _values = values;
        _width = _values.GetLength(0);
        _height = _values.GetLength(1);
    }

    public Vector3 GetValue(float u, float v)
    {
        u = Math.Clamp(u, 0, 1);
        v = Math.Clamp(v, 0, 1);
        var x = (int)MathF.Floor((_width - 1) * u);
        var y = (int)MathF.Floor((_height - 1) * v);
        return _values[x, y];
    }
}