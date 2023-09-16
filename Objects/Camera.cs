using Lab1.Primitives;
using System;

namespace Lab1.Objects
{
    public class Camera
    {
        public VectorSpherical SphericalPosition { get; private set; } = new VectorSpherical(5, 0, MathF.PI / 2);
        public Vector3 Target { get; private set; } = new Vector3(0, 0, 0);
        public Vector3 Up { get; private set; } = new Vector3(0, 1, 0);
        private const float speed = 0.5f;
        private const float angleDelta = MathF.PI / 360;

        public Matrix4 View { get; private set; }
        public Matrix4 Projection { get; private set; }
        public Matrix4 ViewPort { get; private set; }

        private float screenWidth = 16;
        private float screenHeight = 9;
        private float fov = MathF.PI / 2;
        private float zNear = 0;
        private float zFar = 100;

        public float ScreenWidth
        {
            get { return screenWidth; }
            set
            {
                if (screenWidth != value)
                {
                    screenWidth = value;
                    UpdateProjectionMatrix();
                    UpdateViewPortMatrix();
                }
            }
        }

        public float ScreenHeight
        {
            get { return screenHeight; }
            set
            {
                if (screenHeight != value)
                {
                    screenHeight = value;
                    UpdateProjectionMatrix();
                    UpdateViewPortMatrix();
                }
            }
        }

        public float FOV
        {
            get { return fov; }
            set
            {
                if (fov != value)
                {
                    if (value < 0) fov = 0;
                    else if (value > MathF.PI) fov = MathF.PI;
                    else fov = value;
                    UpdateProjectionMatrix();
                }
            }
        }

        public Camera()
        {
            UpdateViewMatrix();
            UpdateProjectionMatrix();
            UpdateViewPortMatrix();
        }

        public void ResetPosition()
        {
            SphericalPosition = new VectorSpherical(5, 0, MathF.PI / 2);
            Target = new Vector3(0, 0, 0);
            Up = new Vector3(0, 1, 0);
            UpdateViewMatrix();
        }

        private void UpdateProjectionMatrix()
        {
            Projection = Matrix4.Projection(fov, screenWidth / screenHeight, zNear, zFar);
        }

        private void UpdateViewMatrix()
        {
            Matrix4 matrix = Matrix4.One();
            Vector3 cartesianPosition = SphericalPosition.ToCartesian();
            Vector3 zAxis = (cartesianPosition - Target).Normalize();
            Vector3 xAxis = Up.Cross(zAxis).Normalize();
            Vector3 yAxis = Up;

            for (int col = 0; col < Vector3.Size; col++)
            {
                matrix[0, col] = xAxis[col];
                matrix[1, col] = yAxis[col];
                matrix[2, col] = zAxis[col];
            }
            matrix[0, 3] = -(xAxis.Dot(cartesianPosition));
            matrix[1, 3] = -(yAxis.Dot(cartesianPosition));
            matrix[2, 3] = -(zAxis.Dot(cartesianPosition));
            View = matrix;
        }

        private void UpdateViewPortMatrix()
        {
            ViewPort = Matrix4.Viewport(screenWidth - 1, screenHeight - 1, 0, 0);
        }

        public void ZoomIn()
        {
            SphericalPosition.R += speed;
            UpdateViewMatrix();
        }

        public void ZoomOut()
        {
            if (SphericalPosition.R > speed)
            {
                SphericalPosition.R -= speed;
                UpdateViewMatrix();
            }
        }

        public void MoveAzimuth(double deltaX)
        {
            SphericalPosition.AzimuthAngle += (float)deltaX * angleDelta;
            UpdateViewMatrix();
        }

        public void MoveZenith(double deltaY)
        {
            SphericalPosition.ElevationAngle += (float)deltaY * angleDelta;
            Vector3 position = SphericalPosition.ToCartesian();
            Up.X = -MathF.Cos(SphericalPosition.ElevationAngle) * MathF.Sin(SphericalPosition.AzimuthAngle);
            Up.Y = MathF.Sin(SphericalPosition.ElevationAngle);
            Up.Z = -MathF.Cos(SphericalPosition.ElevationAngle) * MathF.Cos(SphericalPosition.AzimuthAngle);
            UpdateViewMatrix();
        }
    }
}
