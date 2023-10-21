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

        // Line drawing without pixel depth
        public void DrawLine(Vector2 start, Vector2 end, Color color)
        {
            int colorData = GetColorData(color);
            try
            {
                // Reserve the back buffer for updates
                RenderBuffer.Lock();

                foreach (Pixel pixel in Rasterisation.Rasterise(start, end))
                {
                    SetPixelData(pixel.X, pixel.Y, colorData);
                }
            }
            finally
            {
                // Release the back buffer and make it available for display
                RenderBuffer.Unlock();
            }
        }

        // Line drawing with pixel depth
        public void DrawLine(Vector3 start, Vector3 end, Color color)
        {
            int colorData = GetColorData(color);
            try
            {
                // Reserve the back buffer for updates
                RenderBuffer.Lock();

                foreach (Pixel pixel in Rasterisation.Rasterise(start, end))
                {
                    if (zBuffer[pixel.X, pixel.Y] > pixel.Depth)
                    {
                        zBuffer[pixel.X, pixel.Y] = pixel.Depth;
                        SetPixelData(pixel.X, pixel.Y, colorData);
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

        public void DrawPolygon(Polygon polygon, Color surfaceColor, Color edgeColor)
        {
            Vector4 vertex0 = polygon.Vertices[0].ViewPort;
            Vector4 vertex1 = polygon.Vertices[1].ViewPort;
            Vector4 vertex2 = polygon.Vertices[2].ViewPort;

            // Select the top vertex (0)
            if (vertex0.Y > vertex1.Y) (vertex0, vertex1) = (vertex1, vertex0);
            if (vertex0.Y > vertex2.Y) (vertex0, vertex2) = (vertex2, vertex0);
            // Select the middle (1) and bottom (2) vertex
            if (vertex1.Y > vertex2.Y) (vertex1, vertex2) = (vertex2, vertex1);

            // Draw polygon shape
            DrawLine(new Vector3(vertex0.X, vertex0.Y, vertex0.Z), new Vector3(vertex1.X, vertex1.Y, vertex1.Z), surfaceColor);
            DrawLine(new Vector3(vertex0.X, vertex0.Y, vertex0.Z), new Vector3(vertex2.X, vertex2.Y, vertex2.Z), surfaceColor);
            DrawLine(new Vector3(vertex1.X, vertex1.Y, vertex1.Z), new Vector3(vertex2.X, vertex2.Y, vertex2.Z), surfaceColor);

            float crossX1, crossX2, crossZ1, crossZ2;
            float dx1 = vertex1.X - vertex0.X;
            float dy1 = vertex1.Y - vertex0.Y;
            float dx2 = vertex2.X - vertex0.X;
            float dy2 = vertex2.Y - vertex0.Y;
            float dz1 = vertex1.Z - vertex0.Z;
            float dz2 = vertex2.Z - vertex0.Z;

            float currentY = vertex0.Y;
            float currentZ = vertex0.Z;
            float dz = dy1 == 0 ? 0 : dz1 / dy1;

            // Draw first part of triangle
            while (currentY < vertex1.Y)
            {
                crossX1 = vertex0.X + dx1 / dy1 * (currentY - vertex0.Y);
                crossX2 = vertex0.X + dx2 / dy2 * (currentY - vertex0.Y);
                crossZ1 = vertex0.Z + dz1 / dy1 * (currentZ - vertex0.Z);
                crossZ2 = vertex0.Z + dz2 / dy2 * (currentZ - vertex0.Z);
                DrawLine(new Vector3(crossX1, currentY, crossZ1), new Vector3(crossX2, currentY, crossZ2), surfaceColor);
                currentY++;
                currentZ += dz;
            }

            dx1 = vertex2.X - vertex1.X;
            dy1 = vertex2.Y - vertex1.Y;
            dz1 = vertex2.Z - vertex1.Z;
            dz = dy1 == 0 ? 0 : dz1 / dy1;

            // Draw second part of triangle
            while (currentY <= vertex2.Y)
            {
                crossX1 = vertex1.X + dx1 / dy1 * (currentY - vertex1.Y);
                crossX2 = vertex0.X + dx2 / dy2 * (currentY - vertex0.Y);
                crossZ1 = vertex1.Z + dz1 / dy1 * (currentZ - vertex1.Z);
                crossZ2 = vertex0.Z + dz2 / dy2 * (currentZ - vertex0.Z);
                DrawLine(new Vector3(crossX1, currentY, crossZ1), new Vector3(crossX2, currentY, crossZ2), surfaceColor);
                currentY++;
                currentZ += dz;
            }

            //DrawPolygonEdge(new Vector3(vertex0.X, vertex0.Y, vertex0.Z), new Vector3(vertex2.X, vertex2.Y, vertex2.Z), edgeColor);
            //DrawPolygonEdge(new Vector3(vertex1.X, vertex1.Y, vertex1.Z), new Vector3(vertex2.X, vertex2.Y, vertex2.Z), edgeColor);
            //DrawPolygonEdge(new Vector3(vertex0.X, vertex0.Y, vertex0.Z), new Vector3(vertex1.X, vertex1.Y, vertex1.Z), edgeColor);
        }

        // Polygon edge line drawing with pixel depth
        public void DrawPolygonEdge(Vector3 start, Vector3 end, Color color)
        {
            int xStart = (int)MathF.Floor(start.X);
            int yStart = (int)MathF.Floor(start.Y);
            int xEnd = (int)MathF.Ceiling(end.X);
            int yEnd = (int)MathF.Ceiling(end.Y);

            if (zBuffer[xStart, yStart] < start.Z || zBuffer[xEnd, yEnd] < end.Z) return;

            DrawLine(new Vector2(start.X, start.Y), new Vector2(end.X, end.Y), color);
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
                        if (IsVertexVisible(vertexA.Perspective) && IsVertexVisible(vertexB.Perspective))
                            DrawLine(new Vector2(vertexA.ViewPort.X, vertexA.ViewPort.Y), new Vector2(vertexB.ViewPort.X, vertexB.ViewPort.Y), edgeColor);
                        if (IsVertexVisible(vertexA.Perspective) && IsVertexVisible(vertexC.Perspective))
                            DrawLine(new Vector2(vertexA.ViewPort.X, vertexA.ViewPort.Y), new Vector2(vertexC.ViewPort.X, vertexC.ViewPort.Y), edgeColor);
                        if (IsVertexVisible(vertexB.Perspective) && IsVertexVisible(vertexC.Perspective))
                            DrawLine(new Vector2(vertexB.ViewPort.X, vertexB.ViewPort.Y), new Vector2(vertexC.ViewPort.X, vertexC.ViewPort.Y), edgeColor);
                        break;

                    // Draw only visible rasterizated polygons
                    case DrawMode.Rasterisation:

                        // Check visibility of polygon
                        Vector3 target = new(vertexA.Transform.X - camera.Position.X, vertexA.Transform.Y - camera.Position.Y, vertexA.Transform.Z - camera.Position.Z);
                        if (Vector3.Dot(polygon.Normal, Vector3.Normalize(target)) > 0)
                            if (IsVertexVisible(vertexA.Perspective) && IsVertexVisible(vertexB.Perspective) && IsVertexVisible(vertexC.Perspective))
                            {
                                Color polygonColor = GetPolygonColor(lightColor, surfaceColor, -camera.Position, polygon);
                                DrawPolygon(polygon, polygonColor, edgeColor);
                            }
                        break;
                }
            }
        }

        private bool IsVertexVisible(Vector4 perspectiveVertex)
        {
            return perspectiveVertex.X >= -1 && perspectiveVertex.X <= 1 && perspectiveVertex.Y >= -1 && perspectiveVertex.Y <= 1 && perspectiveVertex.Z >= -1 && perspectiveVertex.Z <= 1;
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
