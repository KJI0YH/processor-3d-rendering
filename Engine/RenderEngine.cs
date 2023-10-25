using Rendering.Objects;
using Rendering.Primitives;
using Rendering.Rasterisation;
using System;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Rendering.Engine
{
    public enum DrawMode
    {
        VertexOnly,
        Wire,
        Rasterisation
    }

    public class RenderEngine
    {
        public readonly WriteableBitmap RenderBuffer;
        private readonly float[,] zBuffer;
        private readonly int _width;
        private readonly int _height;
        private readonly int _bytesPerPixel;
        private readonly int _stride;
        private readonly byte[] _pixelData;

        public IRasterisation Rasterisation = new Bresenham();

        public RenderEngine(int pixelWidth, int pixelHeight)
        {
            _width = pixelWidth;
            _height = pixelHeight;
            RenderBuffer = new WriteableBitmap(pixelWidth, pixelHeight, 96, 96, PixelFormats.Bgr32, null);
            zBuffer = new float[_width, _height];
            _bytesPerPixel = (RenderBuffer.Format.BitsPerPixel + 7) / 8;
            _stride = _width * _bytesPerPixel;
            _pixelData = new byte[_width * _height * _bytesPerPixel];
        }

        public void FillRenderBuffer(Color fillColor)
        {
            for (int i = 0; i < _pixelData.Length; i += _bytesPerPixel)
            {
                _pixelData[i + 2] = fillColor.R;
                _pixelData[i + 1] = fillColor.G;
                _pixelData[i + 0] = fillColor.B;
                _pixelData[i + 3] = 0;
            }
            RenderBuffer.WritePixels(new Int32Rect(0, 0, _width, _height), _pixelData, _stride, 0);
        }

        public void DrawLine(Vector2 start, Vector2 end, Color color)
        {
            int colorData = GetColorData(color);
            try
            {
                // Reserve the back buffer for updates
                RenderBuffer.Lock();

                foreach (Pixel pixel in Rasterisation.Rasterise(start, end))
                {
                    if (pixel.X >= 0 && pixel.X < _width && pixel.Y >= 0 && pixel.Y < _height)
                    {
                        SetPixelData(pixel.X, pixel.Y, colorData);
                    }
                }
            }
            finally
            {
                // Release the back buffer and make it available for display
                RenderBuffer.Unlock();
            }
        }

        public void DrawPixel(float x, float y, Color color)
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
                nint pBackBuffer = RenderBuffer.BackBuffer;

                // Find the address of the pixel to draw
                pBackBuffer += y * RenderBuffer.BackBufferStride;
                pBackBuffer += x * 4;

                // Assign the color data to the pixel
                *(int*)pBackBuffer = colorData;
            }

            // Specify the area of the bitmap that changed
            RenderBuffer.AddDirtyRect(new Int32Rect(x, y, 1, 1));
        }

        private int GetColorData(Color color)
        {
            int colorData = color.R << 16;
            colorData |= color.G << 8;
            colorData |= color.B << 0;
            return colorData;
        }

        private int GetColorData(Vector3 color)
        {
            int colorData = (int)(255 * color.X) << 16;
            colorData |= (int)(255 * color.Y) << 8;
            colorData |= ((int)(255 * color.Z));
            return colorData;
        }

        private Color GetPolygonColor(Color lightColor, Color surfaceColor, Vector3 lightPosition, Polygon polygon)
        {
            Color color = new();
            float intensity = Math.Max(Vector3.Dot(Vector3.Normalize(lightPosition), Vector3.Normalize(polygon.Normal)), 0);
            Vector3 light = new(lightColor.R / 255, lightColor.G / 255, lightColor.B / 255);
            Vector3 surface = new(surfaceColor.R / 255, surfaceColor.G / 255, surfaceColor.B / 255);
            color.R = (byte)MathF.Round(intensity * light.X * surface.X * 255);
            color.G = (byte)MathF.Round(intensity * light.Y * surface.Y * 255);
            color.B = (byte)MathF.Round(intensity * light.Z * surface.Z * 255);
            return color;
        }

        public void DrawPolygon(Polygon polygon, Color surfaceColor)
        {
            Vector4 vertex0 = polygon.Vertices[0].ViewPort;
            Vector4 vertex1 = polygon.Vertices[1].ViewPort;
            Vector4 vertex2 = polygon.Vertices[2].ViewPort;

            Vector3 point0 = new Vector3(vertex0.X, vertex0.Y, vertex0.Z);
            Vector3 point1 = new Vector3(vertex1.X, vertex1.Y, vertex1.Z);
            Vector3 point2 = new Vector3(vertex2.X, vertex2.Y, vertex2.Z);

            Vector3 color0 = new Vector3(surfaceColor.R / 255.0f, surfaceColor.G / 255.0f, surfaceColor.B / 255.0f);
            Vector3 color1 = new Vector3(surfaceColor.R / 255.0f, surfaceColor.G / 255.0f, surfaceColor.B / 255.0f);
            Vector3 color2 = new Vector3(surfaceColor.R / 255.0f, surfaceColor.G / 255.0f, surfaceColor.B / 255.0f);

            ScanLineTriangle(point0, point1, point2, color0, color1, color2);
        }

        private void ScanLineTriangle(Vector3 point0, Vector3 point1, Vector3 point2, Vector3 color0, Vector3 color1, Vector3 color2)
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

            Vector3 kPoint0 = (point2 - point0) / (point2.Y - point0.Y);
            Vector3 kPoint1 = (point1 - point0) / (point1.Y - point0.Y);
            Vector3 kPoint2 = (point2 - point1) / (point2.Y - point1.Y);
            Vector3 kColor0 = (color1 - color0) / (point2.Y - point0.Y);
            Vector3 kColor1 = (color1 - color0) / (point1.Y - point0.Y);
            Vector3 kColor2 = (color2 - color1) / (point2.Y - point1.Y);
            int top = Math.Max(0, (int)Math.Ceiling(point0.Y));
            int bottom = Math.Min(_height - 1, (int)Math.Ceiling(point2.Y));
            try
            {
                // Reserve the back buffer for updates
                RenderBuffer.Lock();

                for (int y = top; y < bottom; y++)
                {
                    Vector3 leftCross = point0 + (y - point0.Y) * kPoint0;
                    Vector3 leftColor = color0 + (y - point0.Y) * kColor0;
                    Vector3 rightCross = y < point1.Y ? point0 + (y - point0.Y) * kPoint1 : point1 + (y - point1.Y) * kPoint2;
                    Vector3 rightColor = y < point1.Y ? color0 + (y - point0.Y) * kColor1 : color1 + (y - point1.Y) * kColor2;
                    if (leftCross.X > rightCross.X)
                    {
                        (leftCross, rightCross) = (rightCross, leftCross);
                        (leftColor, rightColor) = (rightColor, leftColor);
                    }
                    Vector3 kColor = (rightColor - leftColor) / (rightCross.X - leftCross.X);
                    int left = Math.Max(0, (int)Math.Ceiling(leftCross.X));
                    int right = Math.Min(_width - 1, (int)Math.Ceiling(rightCross.X));
                    for (int x = left; x < right; x++)
                    {
                        Vector3 color = leftColor + (x - leftCross.X) * kColor;
                        float zDepth = point0.Z + (x - leftCross.X) * kColor.Z;
                        if (zDepth < zBuffer[x, y])
                        {
                            zBuffer[x, y] = zDepth;
                            SetPixelData(x, y, GetColorData(color));
                        }
                    }
                }
            }
            catch (Exception) { }
            finally
            {
                // Release the back buffer and make it available for display
                RenderBuffer.Unlock();
            }
        }

        public void DrawModel(Model model, Camera camera, Color backColor, Color surfaceColor, Color edgeColor, Color lightColor, DrawMode drawMode)
        {
            // Fill background
            FillRenderBuffer(backColor);
            ResetZBuffer();

            // Projection of each vertex of the model
            foreach (var vertex in model.Vertices)
            {
                vertex.Transform = Vector4.Transform(vertex.Original, model.Transformation);
                vertex.CameraView = Vector4.Transform(vertex.Transform, camera.View);
                vertex.Projected = Vector4.Transform(vertex.CameraView, camera.Projection);
                vertex.Perspective = vertex.Projected / vertex.Projected.W;
                vertex.ViewPort = Vector4.Transform(vertex.Perspective, camera.ViewPort);
            }

            // Drawing of each visible object
            foreach (var polygon in model.Polygons)
            {
                Vertex vertexA = polygon.Vertices[0];
                Vertex vertexB = polygon.Vertices[1];
                Vertex vertexC = polygon.Vertices[2];

                switch (drawMode)
                {
                    // Draw only visible vertices
                    case DrawMode.VertexOnly:
                        if (IsVertexVisible(vertexA.Perspective))
                            DrawPixel(vertexA.ViewPort.X, vertexA.ViewPort.Y, edgeColor);
                        if (IsVertexVisible(vertexB.Perspective))
                            DrawPixel(vertexB.ViewPort.X, vertexB.ViewPort.Y, edgeColor);
                        if (IsVertexVisible(vertexC.Perspective))
                            DrawPixel(vertexC.ViewPort.X, vertexC.ViewPort.Y, edgeColor);
                        break;

                    // Draw only visible lines
                    case DrawMode.Wire:
                        Vector2 v1 = new Vector2(vertexA.ViewPort.X, vertexA.ViewPort.Y);
                        Vector2 v2 = new Vector2(vertexB.ViewPort.X, vertexB.ViewPort.Y);
                        Vector2 v3 = new Vector2(vertexC.ViewPort.X, vertexC.ViewPort.Y);
                        DrawLine(v1, v2, edgeColor);
                        DrawLine(v1, v3, edgeColor);
                        DrawLine(v2, v3, edgeColor);
                        break;

                    // Draw only visible rasterizated polygons
                    case DrawMode.Rasterisation:
                        if (IsPolygonVisible(polygon, camera.Position))
                        {
                            Color polygonColor = GetPolygonColor(lightColor, surfaceColor, -camera.Position, polygon);
                            DrawPolygon(polygon, polygonColor);
                        }
                        break;
                }
            }
        }

        private bool IsVertexVisible(Vector4 perspectiveVertex)
        {
            return perspectiveVertex.X >= -1 && perspectiveVertex.X <= 1 && perspectiveVertex.Y >= -1 && perspectiveVertex.Y <= 1 && perspectiveVertex.Z >= -1 && perspectiveVertex.Z <= 1;
        }

        private bool IsPolygonVisible(Polygon polygon, Vector3 cameraPosition)
        {
            Vector3 target = Vector3.Normalize(new(
                polygon.Vertices[0].Transform.X - cameraPosition.X,
                polygon.Vertices[0].Transform.Y - cameraPosition.Y,
                polygon.Vertices[0].Transform.Z - cameraPosition.Z
            ));
            return (Vector3.Dot(polygon.Normal, target) > 0);
        }

        private void ResetZBuffer()
        {
            for (int i = 0; i < zBuffer.GetLength(0); i++)
            {
                for (int j = 0; j < zBuffer.GetLength(1); j++)
                {
                    zBuffer[i, j] = 1.0f;
                }
            }
        }
    }
}
