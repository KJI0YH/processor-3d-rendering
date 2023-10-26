using System.Numerics;

namespace Rendering.Primitives;

public struct Vertex
{
    public Position Position = new(new Vector4(0, 0, 0, 0));
    public Vector3 Normal = new(0, 0, 0);
    public Vector3 Texture = new(0, 0, 0);

    public Vertex()
    {
    }
}