namespace Lab1.Primitives
{
    public class Vector3
    {
        public float X { get; } = 0;
        public float Y { get; } = 0;
        public float Z { get; } = 0;
        public float W { get; } = 1;

        public Vector3() { }

        public Vector3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public Vector3(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }
    }
}
