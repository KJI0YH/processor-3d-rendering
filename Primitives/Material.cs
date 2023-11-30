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
}