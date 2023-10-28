using System.Numerics;

namespace Rendering.Primitives;

public struct Vertex
{
    public Position Position = new(new Vector4(0, 0, 0, 0));
    public Normal Normal = new(new Vector4(0, 0, 0, 0));
    public Vector3 Texture = new(0, 0, 0);

    public Vertex()
    {
    }

    public Vector3 GetViewPort()
    {
        return new Vector3(Position.ViewPort.X, Position.ViewPort.Y, Position.ViewPort.Z);
    }

    public Vector3 GetNormal()
    {
        return new Vector3(Normal.Transform.X, Normal.Transform.Y, Normal.Transform.Z);
    }
}