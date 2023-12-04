using System;
using System.Collections.Generic;
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
    private int _bytesPerPixel;
    private int _height;
    private byte[] _pixelData;
    private int _stride;
    private int _width;
    private float[,] _zBuffer;
    public WriteableBitmap RenderBuffer;

    private static readonly IEnumerable<IRasterisation> RasterisationMethods = new List<IRasterisation>()
    {
        new Bresenham(),
        new DDALine()
    };

    private static readonly IEnumerator<IRasterisation> RasterisationEnumerator =
        RasterisationMethods.GetEnumerator();

    public IRasterisation Rasterisation { get; private set; }


    public DrawMode DrawMode = DrawMode.Texture;

    public readonly ColorComponent Ambient = new(0.5f, 0.5f, 0.5f);
    public readonly ColorComponent Diffuse = new(1f, 1f, 1f);
    public readonly ColorComponent Specular = new(1f, 1f, 1f);
    public readonly ColorComponent Background = new(0, 0, 0);

    public float KAmbient = 0.3f;
    public float KDiffuse = 0.7f;
    public float KSpecular = 1.0f;
    public float KShininess = 20f;
    public const float K_STEP = 0.1f;
    public const float COLOR_STEP = 0.1f;

    public MaterialMap? CustomTexture;

    public RenderEngine(int pixelWidth = 1920, int pixelHeight = 1080)
    {
        ChangeSize(pixelWidth, pixelHeight);
        NextRasterisation();
    }

    public void ChangeSize(int pixelWidth, int pixelHeight)
    {
        _width = pixelWidth;
        _height = pixelHeight;
        RenderBuffer = new WriteableBitmap(pixelWidth, pixelHeight, 96, 96, PixelFormats.Bgr32, null);
        _zBuffer = new float[_width, _height];
        _bytesPerPixel = (RenderBuffer.Format.BitsPerPixel + 7) / 8;
        _stride = _width * _bytesPerPixel;
        _pixelData = new byte[_width * _height * _bytesPerPixel];
        Clear();
    }

    public void Clear()
    {
        FillRenderBuffer(Background.Color);
    }

    private void FillRenderBuffer(Color fillColor)
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

    public void DrawModel(Model model, Camera camera)
    {
        // Fill background
        FillRenderBuffer(Background.Color);
        ResetZBuffer();

        var edgeColor = Background.InvertNormalized;

        model.Update(camera);

        // Drawing of each visible object
        foreach (var polygon in model.Polygons)
        {
            if (DrawMode > DrawMode.Wire && !IsPolygonVisible(polygon, camera.Position)) continue;

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
                case DrawMode.Lambert:
                default:
                    DrawPolygonLambert(polygon, camera);
                    break;
                case DrawMode.PhongShading:
                case DrawMode.PhongLighting:
                    DrawPolygonPhong(polygon, camera);
                    break;
                case DrawMode.Texture:
                case DrawMode.Custom:
                    DrawPolygonTexture(polygon, camera);
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

    private void DrawPolygonLambert(Polygon polygon, Camera camera)
    {
        var point0 = polygon.Vertices[0].GetViewPort();
        var point1 = polygon.Vertices[1].GetViewPort();
        var point2 = polygon.Vertices[2].GetViewPort();

        // Vertex sort
        if (point0.Y > point2.Y) (point0, point2) = (point2, point0);
        if (point0.Y > point1.Y) (point0, point1) = (point1, point0);
        if (point1.Y > point2.Y) (point1, point2) = (point2, point1);

        var k02 = (point2 - point0) / (point2.Y - point0.Y);
        var k01 = (point1 - point0) / (point1.Y - point0.Y);
        var k12 = (point2 - point1) / (point2.Y - point1.Y);

        var colorData = GetColorData(
            GetLambertColor(polygon.Normal, Vector3.Normalize(camera.Position))
        );

        var top = Math.Max(0, (int)Math.Ceiling(point0.Y));
        var bottom = Math.Min(_height - 1, (int)Math.Ceiling(point2.Y));

        try
        {
            // Reserve the back buffer for updates
            RenderBuffer.Lock();

            for (var y = top; y < bottom; y++)
            {
                var leftPoint = point0 + (y - point0.Y) * k02;
                var rightPoint = y < point1.Y ? point0 + (y - point0.Y) * k01 : point1 + (y - point1.Y) * k12;
                if (leftPoint.X > rightPoint.X) (leftPoint, rightPoint) = (rightPoint, leftPoint);

                var kLine = (rightPoint.Z - leftPoint.Z) / (rightPoint.X - leftPoint.X);

                var left = Math.Max(0, (int)Math.Ceiling(leftPoint.X));
                var right = Math.Min(_width - 1, (int)Math.Ceiling(rightPoint.X));
                for (var x = left; x < right; x++)
                {
                    var zDepth = leftPoint.Z + (x - leftPoint.X) * kLine;
                    if (zDepth < _zBuffer[x, y])
                    {
                        _zBuffer[x, y] = zDepth;
                        SetPixelData(x, y, colorData);
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

    private void DrawPolygonPhong(Polygon polygon, Camera camera)
    {
        var vertex0 = polygon.Vertices[0];
        var vertex1 = polygon.Vertices[1];
        var vertex2 = polygon.Vertices[2];

        // Vertex sort
        if (vertex0.Position.ViewPort.Y > vertex2.Position.ViewPort.Y) (vertex0, vertex2) = (vertex2, vertex0);
        if (vertex0.Position.ViewPort.Y > vertex1.Position.ViewPort.Y) (vertex0, vertex1) = (vertex1, vertex0);
        if (vertex1.Position.ViewPort.Y > vertex2.Position.ViewPort.Y) (vertex1, vertex2) = (vertex2, vertex1);

        var point0 = vertex0.GetViewPort();
        var point1 = vertex1.GetViewPort();
        var point2 = vertex2.GetViewPort();

        var normal0 = vertex0.GetNormal();
        var normal1 = vertex1.GetNormal();
        var normal2 = vertex2.GetNormal();

        var world0 = vertex0.Position.Transform;
        var world1 = vertex1.Position.Transform;
        var world2 = vertex2.Position.Transform;

        var kPoint02 = (point2 - point0) / (point2.Y - point0.Y);
        var kPoint01 = (point1 - point0) / (point1.Y - point0.Y);
        var kPoint12 = (point2 - point1) / (point2.Y - point1.Y);

        var kNormal02 = (normal2 - normal0) / (point2.Y - point0.Y);
        var kNormal01 = (normal1 - normal0) / (point1.Y - point0.Y);
        var kNormal12 = (normal2 - normal1) / (point2.Y - point1.Y);

        var kWorld02 = (world2 - world0) / (point2.Y - point0.Y);
        var kWorld01 = (world1 - world0) / (point1.Y - point0.Y);
        var kWorld12 = (world2 - world1) / (point2.Y - point1.Y);

        var top = Math.Max(0, (int)Math.Ceiling(point0.Y));
        var bottom = Math.Min(_height - 1, (int)Math.Ceiling(point2.Y));

        try
        {
            // Reserve the back buffer for updates
            RenderBuffer.Lock();

            for (var y = top; y < bottom; y++)
            {
                var leftPoint = point0 + (y - point0.Y) * kPoint02;
                var rightPoint = y < point1.Y ? point0 + (y - point0.Y) * kPoint01 : point1 + (y - point1.Y) * kPoint12;

                var leftNormal = normal0 + (y - point0.Y) * kNormal02;
                var rightNormal = y < point1.Y
                    ? normal0 + (y - point0.Y) * kNormal01
                    : normal1 + (y - point1.Y) * kNormal12;

                var leftWorld = world0 + (y - point0.Y) * kWorld02;
                var rightWorld = y < point1.Y
                    ? world0 + (y - point0.Y) * kWorld01
                    : world1 + (y - point1.Y) * kWorld12;

                if (leftPoint.X > rightPoint.X)
                {
                    (leftPoint, rightPoint) = (rightPoint, leftPoint);
                    (leftNormal, rightNormal) = (rightNormal, leftNormal);
                    (leftWorld, rightWorld) = (rightWorld, leftWorld);
                }

                var kLine = (rightPoint.Z - leftPoint.Z) / (rightPoint.X - leftPoint.X);
                var kNormal = (rightNormal - leftNormal) / (rightPoint.X - leftPoint.X);
                var kWorld = (rightWorld - leftWorld) / (rightPoint.X - leftPoint.X);

                var left = Math.Max(0, (int)Math.Ceiling(leftPoint.X));
                var right = Math.Min(_width - 1, (int)Math.Ceiling(rightPoint.X));
                for (var x = left; x < right; x++)
                {
                    var zDepth = leftPoint.Z + (x - leftPoint.X) * kLine;
                    if (zDepth < _zBuffer[x, y])
                    {
                        _zBuffer[x, y] = zDepth;
                        var normal = leftNormal + (x - leftPoint.X) * kNormal;
                        var world = leftWorld + (x - leftPoint.X) * kWorld;

                        var light = camera.Position;
                        light -= new Vector3(world.X, world.Y, world.Z);
                        light = Vector3.Normalize(light);

                        var color = DrawMode == DrawMode.PhongShading
                            ? GetLambertColor(normal, light)
                            : GetPhongColor(normal, light, Vector3.Normalize(-camera.Position));
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

    private void DrawPolygonTexture(Polygon polygon, Camera camera)
    {
        var vertex0 = polygon.Vertices[0];
        var vertex1 = polygon.Vertices[1];
        var vertex2 = polygon.Vertices[2];

        // Vertex sort
        if (vertex0.Position.ViewPort.Y > vertex2.Position.ViewPort.Y) (vertex0, vertex2) = (vertex2, vertex0);
        if (vertex0.Position.ViewPort.Y > vertex1.Position.ViewPort.Y) (vertex0, vertex1) = (vertex1, vertex0);
        if (vertex1.Position.ViewPort.Y > vertex2.Position.ViewPort.Y) (vertex1, vertex2) = (vertex2, vertex1);

        var point0 = vertex0.GetViewPort();
        var point1 = vertex1.GetViewPort();
        var point2 = vertex2.GetViewPort();

        var depth0 = 1 / vertex0.Position.Projected.W;
        var depth1 = 1 / vertex1.Position.Projected.W;
        var depth2 = 1 / vertex2.Position.Projected.W;

        var normal0 = vertex0.GetNormal() * depth0;
        var normal1 = vertex1.GetNormal() * depth1;
        var normal2 = vertex2.GetNormal() * depth2;

        var world0 = vertex0.Position.Transform * depth0;
        var world1 = vertex1.Position.Transform * depth1;
        var world2 = vertex2.Position.Transform * depth2;

        var texture0 = vertex0.Texture * depth0;
        var texture1 = vertex1.Texture * depth1;
        var texture2 = vertex2.Texture * depth2;

        var kPoint02 = (point2 - point0) / (point2.Y - point0.Y);
        var kPoint01 = (point1 - point0) / (point1.Y - point0.Y);
        var kPoint12 = (point2 - point1) / (point2.Y - point1.Y);

        var kNormal02 = (normal2 - normal0) / (point2.Y - point0.Y);
        var kNormal01 = (normal1 - normal0) / (point1.Y - point0.Y);
        var kNormal12 = (normal2 - normal1) / (point2.Y - point1.Y);

        var kWorld02 = (world2 - world0) / (point2.Y - point0.Y);
        var kWorld01 = (world1 - world0) / (point1.Y - point0.Y);
        var kWorld12 = (world2 - world1) / (point2.Y - point1.Y);

        var kTexture02 = (texture2 - texture0) / (point2.Y - point0.Y);
        var kTexture01 = (texture1 - texture0) / (point1.Y - point0.Y);
        var kTexture12 = (texture2 - texture1) / (point2.Y - point1.Y);

        var kDepth02 = (depth2 - depth0) / (point2.Y - point0.Y);
        var kDepth01 = (depth1 - depth0) / (point1.Y - point0.Y);
        var kDepth12 = (depth2 - depth1) / (point2.Y - point1.Y);

        var top = Math.Max(0, (int)Math.Ceiling(point0.Y));
        var bottom = Math.Min(_height - 1, (int)Math.Ceiling(point2.Y));

        try
        {
            // Reserve the back buffer for updates
            RenderBuffer.Lock();

            for (var y = top; y < bottom; y++)
            {
                var leftPoint = point0 + (y - point0.Y) * kPoint02;
                var rightPoint = y < point1.Y ? point0 + (y - point0.Y) * kPoint01 : point1 + (y - point1.Y) * kPoint12;

                var leftNormal = normal0 + (y - point0.Y) * kNormal02;
                var rightNormal = y < point1.Y
                    ? normal0 + (y - point0.Y) * kNormal01
                    : normal1 + (y - point1.Y) * kNormal12;

                var leftWorld = world0 + (y - point0.Y) * kWorld02;
                var rightWorld = y < point1.Y
                    ? world0 + (y - point0.Y) * kWorld01
                    : world1 + (y - point1.Y) * kWorld12;

                var leftDepth = depth0 + (y - point0.Y) * kDepth02;
                var rightDepth = y < point1.Y
                    ? depth0 + (y - point0.Y) * kDepth01
                    : depth1 + (y - point1.Y) * kDepth12;

                var leftTexture = texture0 + (y - point0.Y) * kTexture02;
                var rightTexture = y < point1.Y
                    ? texture0 + (y - point0.Y) * kTexture01
                    : texture1 + (y - point1.Y) * kTexture12;

                if (leftPoint.X > rightPoint.X)
                {
                    (leftPoint, rightPoint) = (rightPoint, leftPoint);
                    (leftNormal, rightNormal) = (rightNormal, leftNormal);
                    (leftWorld, rightWorld) = (rightWorld, leftWorld);
                    (leftTexture, rightTexture) = (rightTexture, leftTexture);
                    (leftDepth, rightDepth) = (rightDepth, leftDepth);
                }

                var kLine = (rightPoint.Z - leftPoint.Z) / (rightPoint.X - leftPoint.X);
                var kNormal = (rightNormal - leftNormal) / (rightPoint.X - leftPoint.X);
                var kWorld = (rightWorld - leftWorld) / (rightPoint.X - leftPoint.X);
                var kTexture = (rightTexture - leftTexture) / (rightPoint.X - leftPoint.X);
                var kDepth = (rightDepth - leftDepth) / (rightPoint.X - leftPoint.X);

                var left = Math.Max(0, (int)Math.Ceiling(leftPoint.X));
                var right = Math.Min(_width - 1, (int)Math.Ceiling(rightPoint.X));
                for (var x = left; x < right; x++)
                {
                    var zDepth = leftPoint.Z + (x - leftPoint.X) * kLine;
                    if (zDepth < _zBuffer[x, y])
                    {
                        _zBuffer[x, y] = zDepth;
                        var normal = leftNormal + (x - leftPoint.X) * kNormal;
                        var world = leftWorld + (x - leftPoint.X) * kWorld;
                        var texture = leftTexture + (x - leftPoint.X) * kTexture;
                        var depth = leftDepth + (x - leftPoint.X) * kDepth;
                        normal /= depth;
                        world /= depth;
                        texture /= depth;

                        var light = camera.Position;
                        light -= new Vector3(world.X, world.Y, world.Z);
                        light = Vector3.Normalize(light);

                        var color = GetTextureColor(polygon.Material, texture, normal, light,
                            Vector3.Normalize(-camera.Position));

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
        return Vector3.Dot(polygon.Normal, target) < 0 &&
               IsVertexVisible(polygon.Vertices[0].Position.Perspective) &&
               IsVertexVisible(polygon.Vertices[1].Position.Perspective) &&
               IsVertexVisible(polygon.Vertices[2].Position.Perspective);
    }

    private void ResetZBuffer()
    {
        for (var i = 0; i < _zBuffer.GetLength(0); i++)
        for (var j = 0; j < _zBuffer.GetLength(1); j++)
            _zBuffer[i, j] = 1.0f;
    }

    public void NextRasterisation()
    {
        if (!RasterisationEnumerator.MoveNext())
        {
            RasterisationEnumerator.Reset();
            RasterisationEnumerator.MoveNext();
        }

        Rasterisation = RasterisationEnumerator.Current;
    }


    private static int GetColorData(Vector3 color)
    {
        var colorData = (int)(255 * color.X) << 16;
        colorData |= (int)(255 * color.Y) << 8;
        colorData |= (int)(255 * color.Z);
        return colorData;
    }

    private Vector3 GetLambertColor(Vector3 normal, Vector3 light)
    {
        var lightColor = Specular.Normalized;
        var surfaceColor = Diffuse.Normalized;
        var intensity = Math.Max(Vector3.Dot(light, normal), 0);
        return new Vector3(
            intensity * lightColor.X * surfaceColor.X,
            intensity * lightColor.Y * surfaceColor.Y,
            intensity * lightColor.Z * surfaceColor.Z
        );
    }

    private Vector3 GetPhongColor(Vector3 normal, Vector3 light, Vector3 view)
    {
        var ambient = KAmbient * Ambient.Normalized;
        var diffuse = KDiffuse * Diffuse.Normalized * MathF.Max(Vector3.Dot(normal, light), 0);
        var reflected = Vector3.Normalize(light - 2 * Vector3.Dot(light, normal) * normal);
        var specular = KSpecular * Specular.Normalized *
                       MathF.Pow(MathF.Max(Vector3.Dot(reflected, view), 0), KShininess);
        return Vector3.Clamp(ambient + diffuse + specular, Vector3.Zero, Vector3.One);
    }

    private Vector3 GetTextureColor(Material? material, Vector3 texture, Vector3 normal, Vector3 light, Vector3 view)
    {
        var u = texture.X;
        var v = texture.Y;
        var ambient = new Vector3(KAmbient);
        var diffuse = new Vector3(KDiffuse);
        var specular = new Vector3(KSpecular);
        var diffuseTexture = DrawMode == DrawMode.Custom ? CustomTexture : material?.Diffuse;
        if (diffuseTexture != null) ambient = diffuse = diffuseTexture.GetValue(u, v);

        if (material is { Normal: not null })
            normal = material.GetNormalValue(u, v);

        ambient *= Ambient.Normalized;
        diffuse *= Diffuse.Normalized * MathF.Max(Vector3.Dot(normal, light), 0);
        var reflected = Vector3.Normalize(light - 2 * Vector3.Dot(light, normal) * normal);
        if (material is { Mirror: not null }) specular = new Vector3(material.GetMirrorValue(u, v));
        specular *= MathF.Pow(MathF.Max(Vector3.Dot(reflected, view), 0), KShininess) * Specular.Normalized;
        return Vector3.Clamp(ambient + diffuse + specular, Vector3.Zero, Vector3.One);
    }
}