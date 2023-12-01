using System.Drawing;
using System.Numerics;

namespace Rendering.Primitives;

public class Material
{
    public string Name { get; }
    public MaterialMap? Diffuse;
    public MaterialMap? Normal;
    public MaterialMap? Mirror;

    public Material(string name)
    {
        Name = name;
    }

    public Vector3 GetDiffuseValue(float u, float v)
    {
        return Diffuse?.GetValue(u, v) ?? Vector3.Zero;
    }

    public Vector3 GetNormalValue(float u, float v)
    {
        return Vector3.Normalize(Normal?.GetValue(u, v) * 2 - Vector3.One ?? Vector3.Zero);
    }

    public float GetMirrorValue(float u, float v)
    {
        return Mirror?.GetValue(u, v).Z ?? 1f;
    }
}