using Lab1.Primitives;

namespace Lab1.Objects
{
    public class Camera
    {
        private Vector3 position = new Vector3(0, 0.25f, -10);
        private Vector3 target = new Vector3(0, 0.25f, 0);
        private Vector3 up = new Vector3(0, 1, 0);

        public Camera()
        {

        }

        public Matrix4 View()
        {
            Matrix4 matrix = Matrix4.One();
            Vector3 zAxis = (position - target).Normalize();
            Vector3 xAxis = up.Cross(zAxis).Normalize();
            Vector3 yAxis = up;

            for (int col = 0; col < Vector3.Size; col++)
            {
                matrix[0, col] = xAxis[col];
                matrix[1, col] = yAxis[col];
                matrix[2, col] = zAxis[col];
            }
            matrix[0, 3] = -(xAxis.Dot(position));
            matrix[1, 3] = -(yAxis.Dot(position));
            matrix[2, 3] = -(zAxis.Dot(position));
            return matrix;
        }
    }
}
