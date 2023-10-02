namespace Lab1.Primitives
{
    public class Pixel
    {
        public int X { get; }
        public int Y { get; }
        public float Depth { get; } = 0.0f;

        public Pixel(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Pixel(int x, int y, float depth)
        {
            X = x;
            Y = y;
            Depth = depth;
        }
    }
}
