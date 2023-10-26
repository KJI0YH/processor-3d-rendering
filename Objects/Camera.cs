using Rendering.Primitives;
using System;
using System.Linq;
using System.Numerics;

namespace Rendering.Objects;

public class Camera
{
    public VectorSpherical SphericalPosition { get; private set; } = new(-1, 0, MathF.PI / 2);
    public Vector3 Position => SphericalPosition.ToCartesian();
    public Vector3 Target { get; private set; } = new(0, 0, 0);
    public Vector3 Up { get; private set; } = new(0, 1, 0);
    public float ZoomStep = 0.1f;
    public float AngleDelta = MathF.PI / 360;
    public float KeyRotation = MathF.PI / 36;
    public float FovStep = MathF.PI / 180;
    public float PlaneDistanceStep = 1;

    public Matrix4x4 View { get; private set; }
    public Matrix4x4 Projection { get; private set; }
    public Matrix4x4 ViewPort { get; private set; }

    private float screenWidth = 16;
    private float screenHeight = 9;
    private float fov = MathF.PI / 3;
    private float zNear = 0.1f;
    private float zFar = 1000;

    public float ScreenWidth
    {
        get => screenWidth;
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
        get => screenHeight;
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
        get => fov;
        set
        {
            if (fov != value)
            {
                if (value < FovStep) fov = FovStep;
                else if (value >= MathF.PI) fov = MathF.PI - FovStep;
                else fov = value;
                UpdateProjectionMatrix();
            }
        }
    }

    public float ZNear
    {
        get => zNear;
        set
        {
            if (ZNear != value)
            {
                if (value <= 0) zNear = 0.1f;
                else zNear = value;
                if (zNear >= zFar) zNear = zFar - 0.1f;
                UpdateProjectionMatrix();
            }
        }
    }

    public float ZFar
    {
        get => zFar;
        set
        {
            if (ZFar != value)
            {
                if (value <= 0) zFar = 0.1f;
                else zFar = value;
                if (zFar <= zNear) zFar = zNear + 0.1f;
                UpdateProjectionMatrix();
            }
        }
    }

    public float AzimuthAngle
    {
        get => SphericalPosition.AzimuthAngle;
        set
        {
            SphericalPosition.AzimuthAngle = value;
            UpdateViewMatrix();
        }
    }

    public float ZenithAngle
    {
        get => SphericalPosition.ZenithAngle;
        set
        {
            SphericalPosition.ZenithAngle = value;
            var upX = -MathF.Cos(SphericalPosition.ZenithAngle) * MathF.Sin(SphericalPosition.AzimuthAngle);
            var upY = MathF.Sin(SphericalPosition.ZenithAngle);
            var upZ = -MathF.Cos(SphericalPosition.ZenithAngle) * MathF.Cos(SphericalPosition.AzimuthAngle);
            Up = new Vector3(upX, upY, upZ);
            UpdateViewMatrix();
        }
    }

    public Camera()
    {
        UpdateViewMatrix();
        UpdateProjectionMatrix();
        UpdateViewPortMatrix();
    }

    private void UpdateProjectionMatrix()
    {
        Projection = Matrix4x4.CreatePerspectiveFieldOfView(fov, screenWidth / screenHeight, zNear, zFar);
    }

    private void UpdateViewMatrix()
    {
        View = Matrix4x4.CreateLookAt(SphericalPosition.ToCartesian(), Target, Up);
    }

    private void UpdateViewPortMatrix()
    {
        var halfWidth = (screenWidth - 1) / 2;
        var halfHeight = (screenHeight - 1) / 2;
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

    public void IncreasePlaneDistanceStep()
    {
        if (PlaneDistanceStep < 1) PlaneDistanceStep += 0.1f;
        else if (PlaneDistanceStep < 10) PlaneDistanceStep += 1;
        else if (PlaneDistanceStep < 100) PlaneDistanceStep += 5;
        else PlaneDistanceStep += 10;
    }

    public void DecreasePlaneDistanceStep()
    {
        if (PlaneDistanceStep > 100) PlaneDistanceStep -= 10;
        else if (PlaneDistanceStep > 10) PlaneDistanceStep -= 5;
        else if (PlaneDistanceStep > 1) PlaneDistanceStep -= 1;
        else PlaneDistanceStep -= 0.1f;
        if (PlaneDistanceStep < 0.1f) PlaneDistanceStep = 0.1f;
    }

    public void SetInitialPosition(Model model)
    {
        float xMax = model.Positions.Max(v => v.Original.X);
        float xMin = model.Positions.Min(v => v.Original.X);
        float yMax = model.Positions.Max(v => v.Original.Y);
        float yMin = model.Positions.Min(v => v.Original.Y);
        float zMax = model.Positions.Max(v => v.Original.Z);
        float zMin = model.Positions.Min(v => v.Original.Z);
        var radiuses = new float[]
        {
            xMax - xMin,
            yMax - yMin,
            zMax - zMin
        };
        var distance = radiuses.Max() + zMax - zMin;
        SphericalPosition = new VectorSpherical(distance, 0, MathF.PI / 2);
        ZoomStep = distance * 0.05f;
        Target = new Vector3(0, 0, 0);
        Up = new Vector3(0, 1, 0);
        zFar = 3 * distance;
        zNear = 0.1f;
        fov = MathF.PI / 3;
        UpdateViewMatrix();
        UpdateProjectionMatrix();
    }
}