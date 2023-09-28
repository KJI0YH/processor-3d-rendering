using Lab1;
using Lab1.Objects;
using Lab1.Primitives;
using Lab1.Rasterization;
using System;
using System.Collections.Generic;
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

        public void DrawPolygon(Vector4 vertex0, Vector4 vertex1, Vector4 vertex2, Color fillColor, Color edgeColor)
        {
            Vector4 buffer;
            // Select the top vertex (0)
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

            float crossX1, crossX2;
            float dx1 = vertex1.X - vertex0.X;
            float dy1 = vertex1.Y - vertex0.Y;
            float dx2 = vertex2.X - vertex0.X;
            float dy2 = vertex2.Y - vertex0.Y;

            float topY = vertex0.Y;

            // Draw first part of triangle
            while (topY < vertex1.Y)
            {
                crossX1 = vertex0.X + dx1 / dy1 * (topY - vertex0.Y);
                crossX2 = vertex0.X + dx2 / dy2 * (topY - vertex0.Y);
                DrawLine(crossX1, topY, crossX2, topY, fillColor);
                topY++;
            }

            dx1 = vertex2.X - vertex1.X;
            dy1 = vertex2.Y - vertex1.Y;

            // Draw second part of triangle
            while (topY < vertex2.Y)
            {
                crossX1 = vertex1.X + dx1 / dy1 * (topY - vertex1.Y);
                crossX2 = vertex0.X + dx2 / dy2 * (topY - vertex0.Y);
                DrawLine(crossX1, topY, crossX2, topY, fillColor);
                topY++;
            }

            // Draw edges of the triangle 
            DrawLine(vertex0.X, vertex0.Y, vertex1.X, vertex1.Y, edgeColor);
            DrawLine(vertex0.X, vertex0.Y, vertex2.X, vertex2.Y, edgeColor);
            DrawLine(vertex1.X, vertex1.Y, vertex2.X, vertex2.Y, edgeColor);
        }

        public void DrawModel(Model model, Camera camera, Color drawColor, Color backColor, DrawMode drawMode)
        {
            // Fill background
            FillRenderBuffer(backColor);

            // Projection of each vertex of the model
            List<Vector4> projectedVertices = new();
            foreach (var vertex in model.Vertices)
            {
                Vector4 projectedVertex = Vector4.Transform(Vector4.Transform(Vector4.Transform(vertex, model.Transformation), camera.View), camera.Projection);

                // Perspective model
                projectedVertex /= projectedVertex.W;
                projectedVertices.Add(projectedVertex);
            }

            // Drawing of each visible polygon
            Vector4?[] viewPortVertices = new Vector4?[projectedVertices.Count];
            foreach (var polygon in model.Polygons)
            {
                int indVertexA = polygon.Indices[0];
                int indVertexB = polygon.Indices[1];
                int indVertexC = polygon.Indices[2];
                Vector4 vertexA = projectedVertices[indVertexA];
                Vector4 vertexB = projectedVertices[indVertexB];
                Vector4 vertexC = projectedVertices[indVertexC];

                if (viewPortVertices[indVertexA] == null) viewPortVertices[indVertexA] = Vector4.Transform(projectedVertices[indVertexA], camera.ViewPort);
                if (viewPortVertices[indVertexB] == null) viewPortVertices[indVertexB] = Vector4.Transform(projectedVertices[indVertexB], camera.ViewPort);
                if (viewPortVertices[indVertexC] == null) viewPortVertices[indVertexC] = Vector4.Transform(projectedVertices[indVertexC], camera.ViewPort);

                Vector4 viewPortVertexA = viewPortVertices[indVertexA].Value;
                Vector4 viewPortVertexB = viewPortVertices[indVertexB].Value;
                Vector4 viewPortVertexC = viewPortVertices[indVertexC].Value;

                switch (drawMode)
                {
                    // Draw only visible vertices
                    case DrawMode.VertexOnly:
                        if (IsVertexVisible(vertexA))
                            DrawPixel(viewPortVertexA.X, viewPortVertexA.Y, drawColor);
                        if (IsVertexVisible(vertexB))
                            DrawPixel(viewPortVertexB.X, viewPortVertexB.Y, drawColor);
                        if (IsVertexVisible(vertexC))
                            DrawPixel(viewPortVertexC.X, viewPortVertexC.Y, drawColor);
                        break;

                    // Draw only visible lines
                    case DrawMode.Wire:
                        if (IsVertexVisible(vertexA) && IsVertexVisible(vertexB))
                            DrawLine(viewPortVertexA.X, viewPortVertexA.Y, viewPortVertexB.X, viewPortVertexB.Y, drawColor);
                        if (IsVertexVisible(vertexA) && IsVertexVisible(vertexC))
                            DrawLine(viewPortVertexA.X, viewPortVertexA.Y, viewPortVertexC.X, viewPortVertexC.Y, drawColor);
                        if (IsVertexVisible(vertexB) && IsVertexVisible(vertexC))
                            DrawLine(viewPortVertexB.X, viewPortVertexB.Y, viewPortVertexC.X, viewPortVertexC.Y, drawColor);
                        break;

                    // Draw only visible rasterizated polygons
                    case DrawMode.Rasterization:
                        if (IsVertexVisible(vertexA) && IsVertexVisible(vertexB) && IsVertexVisible(vertexC))
                            if (Vector3.Dot(polygon.Normal, camera.Position) > 0)
                                DrawPolygon(viewPortVertexA, viewPortVertexB, viewPortVertexC, drawColor, backColor);
                        break;
                }
            }
        }

        private bool IsVertexVisible(Vector4 vertex)
        {
            return (vertex.X >= -1 && vertex.X <= 1 && vertex.Y >= -1 && vertex.Y <= 1 && vertex.Z >= -1 && vertex.Z <= 1);
        }
    }
}
