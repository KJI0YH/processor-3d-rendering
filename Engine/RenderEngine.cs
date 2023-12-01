using System;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Rendering.Information;
using Rendering.Objects;
using Rendering.Primitives;
using Rendering.Rasterisation;

namespace Rendering.Engine;

public class RenderEngine
{
    private readonly int _bytesPerPixel;
    private readonly int _height;
    private readonly byte[] _pixelData;
    private readonly int _stride;
    private readonly int _width;
    private readonly float[,] _zBuffer;
    public readonly WriteableBitmap RenderBuffer;

    public IRasterisation Rasterisation = new Bresenham();
    public DrawMode DrawMode = DrawMode.PhongLighting;

    public Color Light = Colors.White;
    public Color Background = Colors.Black;
    public Color Edge = Colors.Black;
    public Color Surface = Colors.White;
    public ColorComponent Ambient = new(0.5f, 0.5f, 0.5f);
    public ColorComponent Diffuse = new(1f, 1f, 1f);
    public ColorComponent Specular = new(1f, 1f, 1f);

    public float kAmbient = 0.3f;
    public float kDiffuse = 0.8f;
    public float kSpecular = 1.0f;
    public float kShininess = 10f;
    public const float K_STEP = 0.1f;
    public const float COLOR_STEP = 0.1f;

    public RenderEngine(int pixelWidth, int pixelHeight)
    {
        _width = pixelWidth;
        _height = pixelHeight;
        RenderBuffer = new WriteableBitmap(pixelWidth, pixelHeight, 96, 96, PixelFormats.Bgr32, null);
        _zBuffer = new float[_width, _height];
        _bytesPerPixel = (RenderBuffer.Format.BitsPerPixel + 7) / 8;
        _stride = _width * _bytesPerPixel;
        _pixelData = new byte[_width * _height * _bytesPerPixel];
    }

    public void FillRenderBuffer(Color fillColor)
    {
        for (var i = 0; i < _pixelData.Length; i += _bytesPerPixel)
        {
            _pixelData[i + 2] = fillColor.R;
            _pixelData[i + 1] = fillColor.G;
            _pixelData[i + 0] = fillColor.B;
            _pixelData[i + 3] = 0;
        }

        RenderBuffer.WritePixels(new Int32Rect(0, 0, _width, _height), _pixelData, _stride, 0);
    }

    private void DrawLine(Vector2 start, Vector2 end, Vector3 color)
    {
        var colorData = GetColorData(color);
        try
        {
            // Reserve the back buffer for updates
            RenderBuffer.Lock();

            foreach (var pixel in Rasterisation.Rasterise(start, end))
                if (pixel.X >= 0 && pixel.X < _width && pixel.Y >= 0 && pixel.Y < _height)
                    SetPixelData(pixel.X, pixel.Y, colorData);
        }
        finally
        {
            // Release the back buffer and make it available for display
            RenderBuffer.Unlock();
        }
    }

    private void SetPixelData(int x, int y, int colorData)
    {
        unsafe
        {
            // Get a pointer to the back buffer
            var pBackBuffer = RenderBuffer.BackBuffer;

            // Find the address of the pixel to draw
            pBackBuffer += y * RenderBuffer.BackBufferStride;
            pBackBuffer += x * 4;

            // Assign the color data to the pixel
            *(int*)pBackBuffer = colorData;
        }

        // Specify the area of the bitmap that changed
        RenderBuffer.AddDirtyRect(new Int32Rect(x, y, 1, 1));
    }

    private void DrawPixel(float x, float y, Vector3 color)
    {
        try
        {
            // Reserve the back buffer for updates
            RenderBuffer.Lock();
            SetPixelData((int)MathF.Round(x), (int)MathF.Round(y), GetColorData(color));
        }
        finally
        {
            // Release the back buffer and make it available for display
            RenderBuffer.Unlock();
        }
    }

    private static int GetColorData(Vector3 color)
    {
        var colorData = (int)(255 * color.X) << 16;
        colorData |= (int)(255 * color.Y) << 8;
        colorData |= (int)(255 * color.Z);
        return colorData;
    }

    private static Vector3 GetPointColor(Vector3 normal, Vector3 lightColor, Vector3 lightPosition,
        Vector3 surfaceColor)
    {
        var intensity = Math.Max(Vector3.Dot(lightPosition, normal), 0);
        return new Vector3(
            intensity * lightColor.X * surfaceColor.X,
            intensity * lightColor.Y * surfaceColor.Y,
            intensity * lightColor.Z * surfaceColor.Z
        );
    }

    private void DrawPolygon(Polygon polygon, Vector3 lightColor, Vector3 lightPosition, Vector3 surfaceColor)
    {
        var vertex0 = polygon.Vertices[0];
        var vertex1 = polygon.Vertices[1];
        var vertex2 = polygon.Vertices[2];

        Vector3 value0, value1, value2;

        if (DrawMode == DrawMode.Texture && polygon.Material == null)
            DrawMode = DrawMode.PhongLighting;

        switch (DrawMode)
        {
            case DrawMode.PhongShading:
                value0 = GetPointColor(vertex0.GetNormal(), lightColor, lightPosition, surfaceColor);
                value1 = GetPointColor(vertex1.GetNormal(), lightColor, lightPosition, surfaceColor);
                value2 = GetPointColor(vertex2.GetNormal(), lightColor, lightPosition, surfaceColor);
                break;
            case DrawMode.PhongLighting:
                value0 = vertex0.GetNormal();
                value1 = vertex1.GetNormal();
                value2 = vertex2.GetNormal();
                break;
            case DrawMode.Texture:
                value0 = vertex0.Texture;
                value1 = vertex1.Texture;
                value2 = vertex2.Texture;
                break;
            default:
                value0 = value1 = value2 = GetPointColor(polygon.Normal, lightColor, lightPosition, surfaceColor);
                break;
        }

        ScanLineTriangle(vertex0.GetViewPort(), vertex1.GetViewPort(), vertex2.GetViewPort(),
            value0, value1, value2, lightPosition, polygon.Material);
    }

    private void ScanLineTriangle(Vector3 point0, Vector3 point1, Vector3 point2,
        Vector3 value0, Vector3 value1, Vector3 value2, Vector3 lightPosition, Material? material)
    {
        // Vertex sort
        if (point0.Y > point2.Y)
        {
            (point0, point2) = (point2, point0);
            (value0, value2) = (value2, value0);
        }

        if (point0.Y > point1.Y)
        {
            (point0, point1) = (point1, point0);
            (value0, value1) = (value1, value0);
        }

        if (point1.Y > point2.Y)
        {
            (point1, point2) = (point2, point1);
            (value1, value2) = (value2, value1);
        }

        var kPoint0 = (point2 - point0) / (point2.Y - point0.Y);
        var kPoint1 = (point1 - point0) / (point1.Y - point0.Y);
        var kPoint2 = (point2 - point1) / (point2.Y - point1.Y);
        var kValue0 = (value2 - value0) / (point2.Y - point0.Y);
        var kValue1 = (value1 - value0) / (point1.Y - point0.Y);
        var kValue2 = (value2 - value1) / (point2.Y - point1.Y);
        var top = Math.Max(0, (int)Math.Ceiling(point0.Y));
        var bottom = Math.Min(_height, (int)Math.Ceiling(point2.Y));
        try
        {
            // Reserve the back buffer for updates
            RenderBuffer.Lock();

            for (var y = top; y < bottom; y++)
            {
                var leftCross = point0 + (y - point0.Y) * kPoint0;
                var leftValue = value0 + (y - point0.Y) * kValue0;
                var rightCross = y < point1.Y ? point0 + (y - point0.Y) * kPoint1 : point1 + (y - point1.Y) * kPoint2;
                var rightValue = y < point1.Y ? value0 + (y - point0.Y) * kValue1 : value1 + (y - point1.Y) * kValue2;
                if (leftCross.X > rightCross.X)
                {
                    (leftCross, rightCross) = (rightCross, leftCross);
                    (leftValue, rightValue) = (rightValue, leftValue);
                }

                var kValue = (rightValue - leftValue) / (rightCross.X - leftCross.X);
                var kPoint = (rightCross - leftCross) / (rightCross.X - leftCross.X);
                var left = Math.Max(0, (int)Math.Ceiling(leftCross.X));
                var right = Math.Min(_width, (int)Math.Ceiling(rightCross.X));
                for (var x = left; x < right; x++)
                {
                    Vector3 color;
                    switch (DrawMode)
                    {
                        case DrawMode.PhongLighting:
                            var normal = leftValue + (x - leftCross.X) * kValue;
                            color = GetPhongColor(normal, lightPosition, -lightPosition);
                            break;
                        case DrawMode.Texture:
                            color = GetTextureColor(leftValue + (x - leftCross.X) * kValue, material, lightPosition,
                                -lightPosition);
                            break;
                        default:
                            color = leftValue + (x - leftCross.X) * kValue;
                            break;
                    }

                    var zDepth = point0.Z + (x - leftCross.X) * kPoint.Z;
                    if (zDepth < _zBuffer[x, y])
                    {
                        _zBuffer[x, y] = zDepth;
                        SetPixelData(x, y, GetColorData(color));
                    }
                }
            }
        }
        finally
        {
            // Release the back buffer and make it available for display
            RenderBuffer.Unlock();
        }
    }

    private Vector3 GetPhongColor(Vector3 normal, Vector3 light, Vector3 view)
    {
        var ambient = kAmbient * Ambient.Normalized;
        ambient = Vector3.Clamp(ambient, Vector3.Zero, Vector3.One);
        var diffuse = kDiffuse * MathF.Max(Vector3.Dot(normal, light), 0) * Diffuse.Normalized;
        diffuse = Vector3.Clamp(diffuse, Vector3.Zero, Vector3.One);
        var r = Vector3.Normalize(light - 2 * Vector3.Dot(light, normal) * normal);
        var specular = kSpecular * MathF.Pow(MathF.Max(Vector3.Dot(r, view), 0), kShininess) * Specular.Normalized;
        specular = Vector3.Clamp(specular, Vector3.Zero, Vector3.One);
        return Vector3.Clamp(ambient + diffuse + specular, Vector3.Zero, Vector3.One);
    }

    private Vector3 GetTextureColor(Vector3 texture, Material? material, Vector3 light, Vector3 view)
    {
        var u = texture.X;
        var v = texture.Y;
        Vector3 ambient = Vector3.Zero, diffuse = Vector3.Zero, specular = Vector3.Zero;
        if (material is { Diffuse: not null })
            ambient = diffuse = material.GetDiffuseValue(u, v);
        var normal = material.GetNormalValue(u, v);
        var r = Vector3.Normalize(light - 2 * Vector3.Dot(light, normal) * normal);
        var kS = material.GetMirrorValue(u, v);
        specular = kS * MathF.Pow(MathF.Max(Vector3.Dot(r, view), 0), kShininess) * Specular.Normalized;
        specular = Vector3.Clamp(specular, Vector3.Zero, Vector3.One);
        return Vector3.Clamp(ambient + diffuse + specular, Vector3.Zero, Vector3.One);
    }

    public void DrawModel(Model model, Camera camera)
    {
        // Fill background
        FillRenderBuffer(Background);
        ResetZBuffer();

        var surfaceColor = NormalizeColor(Surface);
        var edgeColor = NormalizeColor(Edge);
        var lightColor = NormalizeColor(Light);
        var lightPosition = Vector3.Normalize(camera.Position);

        model.Update(camera);

        // Drawing of each visible object
        foreach (var polygon in model.Polygons)
        {
            var vertex0 = polygon.Vertices[0].Position;
            var vertex1 = polygon.Vertices[1].Position;
            var vertex2 = polygon.Vertices[2].Position;

            switch (DrawMode)
            {
                // Draw only visible vertices
                case DrawMode.VertexOnly:
                    DrawVertexOnly(vertex0, vertex1, vertex2, edgeColor);
                    break;

                // Draw only visible lines
                case DrawMode.Wire:
                    DrawWire(vertex0, vertex1, vertex2, edgeColor);
                    break;

                // Draw only visible rasterizer polygons
                case DrawMode.Rasterisation:
                case DrawMode.PhongShading:
                case DrawMode.PhongLighting:
                case DrawMode.Texture:
                default:
                    DrawRasterisation(polygon, camera, lightColor, lightPosition, surfaceColor);
                    break;
            }
        }
    }

    private void DrawVertexOnly(Position vertex0, Position vertex1, Position vertex2, Vector3 color)
    {
        if (IsVertexVisible(vertex0.Perspective))
            DrawPixel(vertex0.ViewPort.X, vertex0.ViewPort.Y, color);
        if (IsVertexVisible(vertex1.Perspective))
            DrawPixel(vertex1.ViewPort.X, vertex1.ViewPort.Y, color);
        if (IsVertexVisible(vertex2.Perspective))
            DrawPixel(vertex2.ViewPort.X, vertex2.ViewPort.Y, color);
    }

    private void DrawWire(Position vertex0, Position vertex1, Position vertex2, Vector3 color)
    {
        var v0 = new Vector2(vertex0.ViewPort.X, vertex0.ViewPort.Y);
        var v1 = new Vector2(vertex1.ViewPort.X, vertex1.ViewPort.Y);
        var v2 = new Vector2(vertex2.ViewPort.X, vertex2.ViewPort.Y);
        if (IsVertexVisible(vertex0.Perspective) && IsVertexVisible(vertex1.Perspective))
            DrawLine(v0, v1, color);
        if (IsVertexVisible(vertex0.Perspective) && IsVertexVisible(vertex2.Perspective))
            DrawLine(v0, v2, color);
        if (IsVertexVisible(vertex1.Perspective) && IsVertexVisible(vertex2.Perspective))
            DrawLine(v1, v2, color);
    }

    private void DrawRasterisation(Polygon polygon, Camera camera, Vector3 lightColor, Vector3 lightPosition,
        Vector3 surfaceColor)
    {
        if (!IsPolygonVisible(polygon, camera.Position)) return;
        if (IsVertexVisible(polygon.Vertices[0].Position.Perspective) &&
            IsVertexVisible(polygon.Vertices[1].Position.Perspective) &&
            IsVertexVisible(polygon.Vertices[2].Position.Perspective))
            DrawPolygon(polygon, lightColor, lightPosition, surfaceColor);
    }

    private static bool IsVertexVisible(Vector4 perspectiveVertex)
    {
        return perspectiveVertex.X is >= -1 and <= 1 &&
               perspectiveVertex.Y is >= -1 and <= 1 &&
               perspectiveVertex.Z is >= -1 and <= 1;
    }

    private static bool IsPolygonVisible(Polygon polygon, Vector3 cameraPosition)
    {
        var target = Vector3.Normalize(new Vector3(
            polygon.Vertices[0].Position.Transform.X - cameraPosition.X,
            polygon.Vertices[0].Position.Transform.Y - cameraPosition.Y,
            polygon.Vertices[0].Position.Transform.Z - cameraPosition.Z
        ));
        return Vector3.Dot(polygon.Normal, target) < 0;
    }

    private void ResetZBuffer()
    {
        for (var i = 0; i < _zBuffer.GetLength(0); i++)
        for (var j = 0; j < _zBuffer.GetLength(1); j++)
            _zBuffer[i, j] = 1.0f;
    }

    private Vector3 NormalizeColor(Color color)
    {
        return new Vector3(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);
    }
}