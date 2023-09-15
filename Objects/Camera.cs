using Lab1.Primitives;
using System;

namespace Lab1.Objects
{
    public class Camera
    {
        private VectorSpherical sphericalPosition = new VectorSpherical(5, 0, MathF.PI / 2);
        private Vector3 target = new Vector3(0, 0, 0);
        private Vector3 up = new Vector3(0, 1, 0);
        private const float speed = 0.5f;
        private const float angleDelta = MathF.PI / 360;

        public Camera()
        {

        }

        public Matrix4 View()
        {
            Matrix4 matrix = Matrix4.One();
            Vector3 cartesianPosition = sphericalPosition.ToCartesian();
            Vector3 zAxis = (cartesianPosition - target).Normalize();
            Vector3 xAxis = up.Cross(zAxis).Normalize();
            Vector3 yAxis = up;

            for (int col = 0; col < Vector3.Size; col++)
            {
                matrix[0, col] = xAxis[col];
                matrix[1, col] = yAxis[col];
                matrix[2, col] = zAxis[col];
            }
            matrix[0, 3] = -(xAxis.Dot(cartesianPosition));
            matrix[1, 3] = -(yAxis.Dot(cartesianPosition));
            matrix[2, 3] = -(zAxis.Dot(cartesianPosition));
            return matrix;
        }

        public void ZoomIn()
        {
            sphericalPosition.R += speed;
        }

        public void ZoomOut()
        {
            sphericalPosition.R -= speed;
        }

        public void MoveAzimuth(double deltaX)
        {
            sphericalPosition.AzimuthAngle += (float)deltaX * angleDelta;
        }

        public void MoveZenith(double deltaY)
        {
            sphericalPosition.ElevationAngle += (float)deltaY * angleDelta;
            Vector3 position = sphericalPosition.ToCartesian();
            up.X = -MathF.Cos(sphericalPosition.ElevationAngle) * MathF.Sin(sphericalPosition.AzimuthAngle);
            up.Y = MathF.Sin(sphericalPosition.ElevationAngle);
            up.Z = -MathF.Cos(sphericalPosition.ElevationAngle) * MathF.Cos(sphericalPosition.AzimuthAngle);
        }
    }
}
