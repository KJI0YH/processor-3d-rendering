using System.Numerics;

namespace simple_3d_rendering.Primitives
{
    public class Vertex
    {
        public int Index { get; }
        public Vector4 Original { get; }
        public Vector4 Transform;
        public Vector4 CameraView;
        public Vector4 Projected;
        public Vector4 Perspective;
        public Vector4 ViewPort;

        public Vertex(Vector4 original, int index)
        {
            Original = original;
            Index = index;
        }
    }
}
