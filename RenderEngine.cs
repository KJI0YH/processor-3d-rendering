using Lab1;
using Lab1.Objects;
using Lab1.Primitives;
using Lab1.Rasterization;
using simple_3d_rendering.Primitives;
using System;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace simple_3d_rendering
{
    public enum DrawMode
    {
        VertexOnly,
        Wire,
        Rasterization
    }

    public class RenderEngine
    {
        public readonly WriteableBitmap RenderBuffer;
        private readonly double?[,] zBuffer;
        private readonly int _width;
        private readonly int _height;
        private readonly int _bytesPerPixel;
        private readonly int _stride;

        public IRasterization Rasterization = new Bresenham();

        public RenderEngine(int pixelWidth, int pixelHeight)
        {
            _width = pixelWidth;
            _height = pixelHeight;
            RenderBuffer = new WriteableBitmap(pixelWidth, pixelHeight, 96, 96, PixelFormats.Bgr32, null);
            zBuffer = new double?[_width, _height];
            _bytesPerPixel = (RenderBuffer.Format.BitsPerPixel + 7) / 8;
            _stride = _width * _bytesPerPixel;
        }

        public void FillRenderBuffer(Color fillColor)
        {
            byte[] pixelData = new byte[_width * _height * _bytesPerPixel];

            for (int i = 0; i < pixelData.Length; i += _bytesPerPixel)
            {
                pixelData[i + 2] = fillColor.R;
                pixelData[i + 1] = fillColor.G;
                pixelData[i + 0] = fillColor.B;
                pixelData[i + 3] = 0;
            }
            RenderBuffer.WritePixels(new Int32Rect(0, 0, _width, _height), pixelData, _stride, 0);
        }

        // Line drawing without pixel depth
        public void DrawLine(float xStart, float yStart, float xEnd, float yEnd, Color color)
        {
            int colorData = GetColorData(color);
            try
            {
                // Reserve the back buffer for updates
                RenderBuffer.Lock();

                foreach (Pixel pixel in Rasterization.Rasterize(xStart, yStart, xEnd, yEnd))
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

                foreach (Pixel pixel in Rasterization.Rasterize(start, end))
                {
                    if (zBuffer[pixel.X, pixel.Y] > pixel.Depth)
                    {
                        zBuffer[pixel.X, pixel.Y] = pixel.Depth;
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
                IntPtr pBackBuffer = RenderBuffer.BackBuffer;

                // Find the address of the pixel to draw
                pBackBuffer += y * RenderBuffer.BackBufferStride;
                pBackBuffer += x * 4;

                // Assign the color data to the pixel
                *((int*)pBackBuffer) = colorData;
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
            color.R = (byte)(MathF.Round(intensity * light.X * surface.X * 255));
            color.G = (byte)(MathF.Round(intensity * light.Y * surface.Y * 255));
            color.B = (byte)(MathF.Round(intensity * light.Z * surface.Z * 255));
            return color;
        }

        public void DrawPolygon(Polygon polygon, Color surfaceColor, Color edgeColor)
        {
            // Select the top vertex (0)
            Vector4 vertex0 = polygon.Vertices[0].ViewPort;
            Vector4 vertex1 = polygon.Vertices[1].ViewPort;
            Vector4 vertex2 = polygon.Vertices[2].ViewPort;
            if (vertex0.Y > vertex1.Y)
            {
                (vertex0, vertex1) = (vertex1, vertex0);
            }

            if (vertex0.Y > vertex2.Y)
            {
                (vertex0, vertex2) = (vertex2, vertex0);
            }

            // Select the middle (1) and bottom (2) vertex
            if (vertex1.Y > vertex2.Y)
            {
                (vertex1, vertex2) = (vertex2, vertex1);
            }

            float crossX1, crossX2, crossZ1, crossZ2;
            float dx1 = vertex1.X - vertex0.X;
            float dy1 = vertex1.Y - vertex0.Y;
            float dx2 = vertex2.X - vertex0.X;
            float dy2 = vertex2.Y - vertex0.Y;
            float dz1 = vertex1.Z - vertex0.Z;
            float dz2 = vertex2.Z - vertex0.Z;

            // Draw edges of the triangle 
            DrawLine(new Vector3(vertex0.X, vertex0.Y, vertex0.Z), new Vector3(vertex1.X, vertex1.Y, vertex1.Z), edgeColor);
            DrawLine(new Vector3(vertex0.X, vertex0.Y, vertex0.Z), new Vector3(vertex2.X, vertex2.Y, vertex2.Z), edgeColor);
            DrawLine(new Vector3(vertex1.X, vertex1.Y, vertex1.Z), new Vector3(vertex2.X, vertex2.Y, vertex2.Z), edgeColor);

            float topY = vertex0.Y;
            float topZ = vertex0.Z;
            float dz = dy1 == 0 ? 0 : dz1 / dy1;

            // Draw first part of triangle
            while (topY < vertex1.Y)
            {
                crossX1 = vertex0.X + dx1 / dy1 * (topY - vertex0.Y);
                crossX2 = vertex0.X + dx2 / dy2 * (topY - vertex0.Y);
                crossZ1 = vertex0.Z + dz1 / dy1 * (topZ - vertex0.Z);
                crossZ2 = vertex0.Z + dz2 / dy2 * (topZ - vertex0.Z);
                DrawLine(new Vector3(crossX1, topY, crossZ1), new Vector3(crossX2, topY, crossZ2), surfaceColor);
                topY++;
                topZ += dz;
            }

            dx1 = vertex2.X - vertex1.X;
            dy1 = vertex2.Y - vertex1.Y;
            dz1 = vertex2.Z - vertex1.Z;
            dz = dy1 == 0 ? 0 : dz1 / dy1;

            // Draw second part of triangle
            while (topY < vertex2.Y)
            {
                crossX1 = vertex1.X + dx1 / dy1 * (topY - vertex1.Y);
                crossX2 = vertex0.X + dx2 / dy2 * (topY - vertex0.Y);
                crossZ1 = vertex1.Z + dz1 / dy1 * (topZ - vertex1.Z);
                crossZ2 = vertex0.Z + dz2 / dy2 * (topZ - vertex0.Z);
                DrawLine(new Vector3(crossX1, topY, crossZ1), new Vector3(crossX2, topY, crossZ2), surfaceColor);
                topY++;
                topZ += dz;
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
                        if (IsVertexVisible(vertexA.Perspective) && IsVertexVisible(vertexB.Perspective))
                            DrawLine(vertexA.ViewPort.X, vertexA.ViewPort.Y, vertexB.ViewPort.X, vertexB.ViewPort.Y, edgeColor);
                        if (IsVertexVisible(vertexA.Perspective) && IsVertexVisible(vertexC.Perspective))
                            DrawLine(vertexA.ViewPort.X, vertexA.ViewPort.Y, vertexC.ViewPort.X, vertexC.ViewPort.Y, edgeColor);
                        if (IsVertexVisible(vertexB.Perspective) && IsVertexVisible(vertexC.Perspective))
                            DrawLine(vertexB.ViewPort.X, vertexB.ViewPort.Y, vertexC.ViewPort.X, vertexC.ViewPort.Y, edgeColor);
                        break;

                    // Draw only visible rasterizated polygons
                    case DrawMode.Rasterization:

                        // Check visibility of polygon
                        if (Vector3.Dot(polygon.Normal, -camera.Position) > 0)
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
            return (perspectiveVertex.X >= -1 && perspectiveVertex.X <= 1 && perspectiveVertex.Y >= -1 && perspectiveVertex.Y <= 1 && perspectiveVertex.Z >= -1 && perspectiveVertex.Z <= 1);
        }

        private void ResetZBuffer()
        {
            for (int i = 0; i < zBuffer.GetLength(0); i++)
            {
                for (int j = 0; j < zBuffer.GetLength(1); j++)
                {
                    zBuffer[i, j] = 1.0;
                }
            }
        }
    }
}
