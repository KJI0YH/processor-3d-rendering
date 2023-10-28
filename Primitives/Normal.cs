using System.Numerics;

namespace Rendering.Primitives;

public class Normal
{
    public Vector4 Original { get; }
    public Vector4 Transform;

    public Normal(Vector4 original)
    {
        Original = original;
    }
}