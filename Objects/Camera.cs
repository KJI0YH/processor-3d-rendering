using Lab1.Primitives;
using System;
using System.Numerics;

namespace Lab1.Objects
{
    public class Camera
    {
        public VectorSpherical SphericalPosition { get; private set; } = new VectorSpherical(1, 0, MathF.PI / 2);
        public Vector3 Target { get; private set; } = new Vector3(0, 0, 0);
        public Vector3 Up { get; private set; } = new Vector3(0, 1, 0);
        public float ZoomStep = 0.1f;
        public const float angleDelta = MathF.PI / 360;
        public float fovStep = MathF.PI / 180;

        public Matrix4x4 View { get; private set; }
        public Matrix4x4 Projection { get; private set; }
        public Matrix4x4 ViewPort { get; private set; }

        private float screenWidth = 16;
        private float screenHeight = 9;
        private float fov = MathF.PI / 2;
        private float zNear = 0.1f;
        private float zFar = 10000;

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
                    if (value < fovStep) fov = fovStep;
                    else if (value >= MathF.PI) fov = MathF.PI - fovStep;
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
            Projection = Matrix4x4.CreatePerspectiveFieldOfView(fov, screenWidth / screenHeight, zNear, zFar); ;
        }

        private void UpdateViewMatrix()
        {
            View = Matrix4x4.CreateLookAt(SphericalPosition.ToCartesian(), Target, Up);
        }

        private void UpdateViewPortMatrix()
        {
            float halfWidth = (screenWidth - 1) / 2;
            float halfHeight = (screenHeight - 1) / 2;
            ViewPort = new Matrix4x4(
                halfWidth, 0, 0, 0,
                0, -halfHeight, 0, 0,
                0, 0, 1, 0,
                halfWidth, halfHeight, 0, 1);
        }

        public void ZoomIn()
        {
            SphericalPosition.R += ZoomStep;
            UpdateViewMatrix();
        }

        public void ZoomOut()
        {
            if (SphericalPosition.R > ZoomStep)
            {
                SphericalPosition.R -= ZoomStep;
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
            float upX = -MathF.Cos(SphericalPosition.ElevationAngle) * MathF.Sin(SphericalPosition.AzimuthAngle);
            float upY = MathF.Sin(SphericalPosition.ElevationAngle);
            float upZ = -MathF.Cos(SphericalPosition.ElevationAngle) * MathF.Cos(SphericalPosition.AzimuthAngle);
            Up = new Vector3(upX, upY, upZ);
            UpdateViewMatrix();
        }

        public void IncreaseZoomStep()
        {
            if (ZoomStep < 1) ZoomStep += 0.1f;
            else if (ZoomStep < 10) ZoomStep += 1;
            else if (ZoomStep < 100) ZoomStep += 5;
            else ZoomStep += 10;
        }

        public void DecreaseZoomStep()
        {
            if (ZoomStep > 100) ZoomStep -= 10;
            else if (ZoomStep > 10) ZoomStep -= 5;
            else if (ZoomStep > 1) ZoomStep -= 1;
            else ZoomStep -= 0.1f;
            if (ZoomStep < 0.1f) ZoomStep = 0.1f;
        }
    }
}
