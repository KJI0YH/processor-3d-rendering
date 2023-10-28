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
        set => _r = Math.Clamp(value, 0, 1);
    }

    private float _g;

    public float G
    {
        get => _g;
        set => _g = Math.Clamp(value, 0, 1);
    }

    private float _b;

    public float B
    {
        get => _b;
        set => _b = Math.Clamp(value, 0, 1);
    }

    public Color Color => Color.FromRgb((byte)(_r * 255), (byte)(_g * 255), (byte)(_b * 255));
    public Vector3 Normalized => new(_r, _g, _b);

    public ColorComponent(float r, float g, float b)
    {
        R = r;
        G = g;
        B = b;
    }
}