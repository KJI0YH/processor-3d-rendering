using System.Numerics;

namespace Rendering.Primitives;

public class Position
{
    public Vector4 Original { get; }
    public Vector4 Transform;
    public Vector4 CameraView;
    public Vector4 Projected;
    public Vector4 Perspective;
    public Vector4 ViewPort;
    public float W;

    public Position(Vector4 original)
    {
        Original = original;
        Transform = original;
    }
}