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
        var x = (int)MathF.Floor(_width * u);
        var y = (int)MathF.Floor(_height * v);
        return _values[x, y];
    }
}