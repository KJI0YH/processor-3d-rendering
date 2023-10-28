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
    public DrawMode DrawMode = DrawMode.Rasterisation;

    public Color Light = Colors.White;
    public Color Background = Colors.Black;
    public Color Edge = Colors.Black;
    public Color Surface = Colors.White;
    public ColorComponent Ambient = new(1, 0, 1);
    public ColorComponent Diffuse = new(0, 0.5f, 0);
    public ColorComponent Specular = new(0.5f, 0.5f, 0);

    public float kAmbient = 0.5f;
    public float kDiffuse = 0.5f;
    public float kSpecular = 1.0f;
    public float kShininess = 0.5f;
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

    private int GetColorData(Vector3 color)
    {
        var colorData = (int)(255 * color.X) << 16;
        colorData |= (int)(255 * color.Y) << 8;
        colorData |= (int)(255 * color.Z);
        return colorData;
    }

    private Vector3 GetPointColor(Vector3 normal, Vector3 lightColor, Vector3 lightPosition, Vector3 surfaceColor)
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

        Vector3 color0, color1, color2;

        switch (DrawMode)
        {
            case DrawMode.PhongShading:
                color0 = GetPointColor(vertex0.GetNormal(), lightColor, lightPosition, surfaceColor);
                color1 = GetPointColor(vertex1.GetNormal(), lightColor, lightPosition, surfaceColor);
                color2 = GetPointColor(vertex2.GetNormal(), lightColor, lightPosition, surfaceColor);
                break;
            case DrawMode.PhongLighting:
                color0 = vertex0.GetNormal();
                color1 = vertex1.GetNormal();
                color2 = vertex2.GetNormal();
                break;
            default:
                color0 = color1 = color2 = GetPointColor(polygon.Normal, lightColor, lightPosition, surfaceColor);
                break;
        }

        ScanLineTriangle(vertex0.GetViewPort(), vertex1.GetViewPort(), vertex2.GetViewPort(),
            color0, color1, color2, lightPosition);
    }

    private void ScanLineTriangle(Vector3 point0, Vector3 point1, Vector3 point2,
        Vector3 color0, Vector3 color1, Vector3 color2, Vector3 lightPosition)
    {
        // Vertex sort
        if (point0.Y > point2.Y)
        {
            (point0, point2) = (point2, point0);
            (color0, color2) = (color2, color0);
        }

        if (point0.Y > point1.Y)
        {
            (point0, point1) = (point1, point0);
            (color0, color1) = (color1, color0);
        }

        if (point1.Y > point2.Y)
        {
            (point1, point2) = (point2, point1);
            (color1, color2) = (color2, color1);
        }

        var kPoint0 = (point2 - point0) / (point2.Y - point0.Y);
        var kPoint1 = (point1 - point0) / (point1.Y - point0.Y);
        var kPoint2 = (point2 - point1) / (point2.Y - point1.Y);
        var kColor0 = (color2 - color0) / (point2.Y - point0.Y);
        var kColor1 = (color1 - color0) / (point1.Y - point0.Y);
        var kColor2 = (color2 - color1) / (point2.Y - point1.Y);
        var top = Math.Max(0, (int)Math.Ceiling(point0.Y));
        var bottom = Math.Min(_height, (int)Math.Ceiling(point2.Y));
        try
        {
            // Reserve the back buffer for updates
            RenderBuffer.Lock();

            for (var y = top; y < bottom; y++)
            {
                var leftCross = point0 + (y - point0.Y) * kPoint0;
                var leftColor = color0 + (y - point0.Y) * kColor0;
                var rightCross = y < point1.Y ? point0 + (y - point0.Y) * kPoint1 : point1 + (y - point1.Y) * kPoint2;
                var rightColor = y < point1.Y ? color0 + (y - point0.Y) * kColor1 : color1 + (y - point1.Y) * kColor2;
                if (leftCross.X > rightCross.X)
                {
                    (leftCross, rightCross) = (rightCross, leftCross);
                    (leftColor, rightColor) = (rightColor, leftColor);
                }

                var kColor = (rightColor - leftColor) / (rightCross.X - leftCross.X);
                var kPoint = (rightCross - leftCross) / (rightCross.X - leftCross.X);
                var left = Math.Max(0, (int)Math.Ceiling(leftCross.X));
                var right = Math.Min(_width, (int)Math.Ceiling(rightCross.X));
                for (var x = left; x < right; x++)
                {
                    Vector3 color;
                    switch (DrawMode)
                    {
                        case DrawMode.PhongLighting:
                            var normal = leftColor + (x - leftCross.X) * kColor;
                            color = GetPointColor(normal, lightPosition, -lightPosition);
                            break;
                        default:
                            color = leftColor + (x - leftCross.X) * kColor;
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

    private Vector3 GetPointColor(Vector3 normal, Vector3 light, Vector3 view)
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

    public void DrawModel(Model model, Camera camera)
    {
        // Fill background
        FillRenderBuffer(Background);
        ResetZBuffer();

        var surfaceColor = NormalizeColor(Surface);
        var edgeColor = NormalizeColor(Edge);
        var lightColor = NormalizeColor(Light);
        var lightPosition = Vector3.Normalize(camera.Position);

        // Projection of each vertex of the model
        foreach (var vertex in model.Positions)
        {
            vertex.Transform = Vector4.Transform(vertex.Original, model.Transformation);
            vertex.CameraView = Vector4.Transform(vertex.Transform, camera.View);
            vertex.Projected = Vector4.Transform(vertex.CameraView, camera.Projection);
            vertex.Perspective = vertex.Projected / vertex.Projected.W;
            vertex.ViewPort = Vector4.Transform(vertex.Perspective, camera.ViewPort);
        }

        // Transform each vertex normal of the model
        foreach (var normal in model.Normals)
            normal.Transform = Vector4.Transform(normal.Original, model.Transformation);

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
                    if (IsVertexVisible(vertex0.Perspective))
                        DrawPixel(vertex0.ViewPort.X, vertex0.ViewPort.Y, edgeColor);
                    if (IsVertexVisible(vertex1.Perspective))
                        DrawPixel(vertex1.ViewPort.X, vertex1.ViewPort.Y, edgeColor);
                    if (IsVertexVisible(vertex2.Perspective))
                        DrawPixel(vertex2.ViewPort.X, vertex2.ViewPort.Y, edgeColor);
                    break;

                // Draw only visible lines
                case DrawMode.Wire:
                    var v0 = new Vector2(vertex0.ViewPort.X, vertex0.ViewPort.Y);
                    var v1 = new Vector2(vertex1.ViewPort.X, vertex1.ViewPort.Y);
                    var v2 = new Vector2(vertex2.ViewPort.X, vertex2.ViewPort.Y);
                    if (IsVertexVisible(vertex0.Perspective) && IsVertexVisible(vertex1.Perspective))
                        DrawLine(v0, v1, edgeColor);
                    if (IsVertexVisible(vertex0.Perspective) && IsVertexVisible(vertex2.Perspective))
                        DrawLine(v0, v2, edgeColor);
                    if (IsVertexVisible(vertex1.Perspective) && IsVertexVisible(vertex2.Perspective))
                        DrawLine(v1, v2, edgeColor);
                    break;

                // Draw only visible rasterizer polygons
                case DrawMode.Rasterisation:
                case DrawMode.PhongShading:
                case DrawMode.PhongLighting:
                    if (IsPolygonVisible(polygon, camera.Position))
                        if (IsVertexVisible(vertex0.Perspective) &&
                            IsVertexVisible(vertex1.Perspective) &&
                            IsVertexVisible(vertex2.Perspective))
                            DrawPolygon(polygon, lightColor, lightPosition, surfaceColor);
                    break;
            }
        }
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