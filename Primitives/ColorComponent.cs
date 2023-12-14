using System;
using System.Numerics;
using System.Windows.Media;

namespace Rendering.Primitives;

public class ColorComponent
{
    private float _r;

    public float R
    {
        get => _r;
        set
        {
            _r = value;
            _vector.X = _r;
            _color.R = (byte)(Math.Clamp(value, 0, 1) * 255);
            _invertNormalized.X = 1 - _r;
            _invertColor.R = (byte)(255 - _color.R);
        }
    }

    private float _g;

    public float G
    {
        get => _g;
        set
        {
            _g = value;
            _vector.Y = _g;
            _color.G = (byte)(Math.Clamp(value, 0, 1) * 255);
            _invertNormalized.Y = 1 - _g;
            _invertColor.G = (byte)(255 - _color.G);
        }
    }

    private float _b;

    public float B
    {
        get => _b;
        set
        {
            _b = value;
            _vector.Z = _b;
            _color.B = (byte)(Math.Clamp(value, 0, 1) * 255);
            _invertNormalized.Z = 1 - _b;
            _invertColor.B = (byte)(255 - _color.B);
        }
    }

    public Color Color => _color;
    private Color _color;

    public Vector3 Vector => _vector;
    private Vector3 _vector;

    public Color InvertColor => _invertColor;
    private Color _invertColor;

    public Vector3 InvertNormalized => _invertNormalized;
    private Vector3 _invertNormalized;

    public ColorComponent(float r, float g, float b)
    {
        R = r;
        G = g;
        B = b;
        _vector = new Vector3(_r, _g, _b);
        _color = Color.FromRgb((byte)(_r * 255), (byte)(_g * 255), (byte)(_b * 255));
        _invertNormalized = new Vector3(1 - _r, 1 - _g, 1 - _b);
        _invertColor = Color.FromRgb((byte)((1 - _r) * 255), (byte)((1 - _g) * 255), (byte)((1 - _b) * 255));
    }

    public void Invert()
    {
        (_vector, _invertNormalized) = (_invertNormalized, _vector);
        (_color, _invertColor) = (_invertColor, _color);
    }
}