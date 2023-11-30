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
}