using Lab1.Objects;
using Lab1.Parser;
using Lab1.Primitives;
using Lab1.Rasterization;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Lab1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const Key CLOSE_APP_KEY = Key.Escape;
        private const Key OPEN_FILE_KEY = Key.O;
        private const Key INVERT_COLORS_KEY = Key.C;
        private const Key X_AXIS_ROTATION_KEY = Key.X;
        private const Key Y_AXIS_ROTATION_KEY = Key.Y;
        private const Key Z_AXIS_ROTATION_KEY = Key.Z;
        private const Key LINES_TOGGLE_KEY = Key.L;
        private const Key FOV_CHANGE_KEY = Key.F;
        private const Key RASTERIZATION_CHANGE_KEY = Key.R;
        private const Key INFORMATION_KEY = Key.I;
        private const Key HELP_KEY = Key.F1;

        private const float scaleDelta = 0.5f;
        private const float rotationDelta = MathF.PI / 36;
        private const float fovDelta = MathF.PI / 36;
        private bool drawLines = true;

        private OpenFileDialog openFileDialog;
        private ObjParser parser = new ObjParser();
        private WriteableBitmap renderBuffer;
        private Model model;
        private Camera camera = new Camera();

        private Point mouseClickPosition;
        private Color fillColor = Colors.Black;
        private Color drawColor = Colors.White;
        private Color errorColor = Colors.Red;

        private int rasterizationMethodIndex = 0;
        private IRasterization[] rasterizationMethods = new IRasterization[]
        {
            new Bresenham(),
            new DDALine(),
        };
        private IRasterization rasterizationMethod;

        public MainWindow()
        {
            InitializeComponent();
            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Wavefront files (.obj)|*.obj";
            rasterizationMethod = rasterizationMethods[rasterizationMethodIndex];
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            InitializeRenderBuffer();
            FillRenderBuffer(fillColor);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitializeRenderBuffer();
            camera.ScreenWidth = (float)ActualWidth;
            camera.ScreenHeight = (float)ActualHeight;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case OPEN_FILE_KEY:
                    if (openFileDialog.ShowDialog() == true)
                    {
                        model = parser.Parse(openFileDialog.FileName);
                        camera.ResetPosition();
                    }
                    break;
                case INVERT_COLORS_KEY:
                    InvertColors();
                    break;
                case LINES_TOGGLE_KEY:
                    drawLines = !drawLines;
                    break;
                case RASTERIZATION_CHANGE_KEY:
                    ChangeRasterization();
                    break;
                case CLOSE_APP_KEY:
                    Application.Current.Shutdown();
                    break;
                default:
                    break;
            }
            DrawModel(model, camera);
        }

        private void InitializeRenderBuffer()
        {
            renderBuffer = new WriteableBitmap((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Bgr32, null);
            imgScreen.Source = renderBuffer;
        }

        private void DrawModel(Model model, Camera camera)
        {
            FillRenderBuffer(fillColor);
            List<Vector3> projectedVertices = new List<Vector3>();
            int start = Environment.TickCount;
            foreach (var vertex in model.Vertices)
            {
                Vector3 projectedVertex = camera.Projection * (camera.View * (model.Transformation * vertex));
                projectedVertex.Update(projectedVertex / projectedVertex.W);
                projectedVertices.Add(projectedVertex);
            }

            Vector3?[] viewPortVertices = new Vector3?[projectedVertices.Count];
            foreach (var polygon in model.Polygons)
            {
                for (int i = 0; i < polygon.Indices.Count; i++)
                {
                    int startVertexIndex = polygon.Indices[i];
                    int endVertexIndex = polygon.Indices[(i + 1) % polygon.Indices.Count];
                    Vector3 startVertex = projectedVertices[startVertexIndex];
                    Vector3 endVertex = projectedVertices[endVertexIndex];

                    if (startVertex.X < -1 || startVertex.X > 1 || startVertex.Y < -1 || startVertex.Y > 1 || startVertex.Z < -1 || startVertex.Z > 1) continue;
                    if (endVertex.X < -1 || endVertex.X > 1 || endVertex.Y < -1 || endVertex.Y > 1 || endVertex.Z < -1 || endVertex.Z > 1) continue;

                    if (viewPortVertices[startVertexIndex] == null)
                        viewPortVertices[startVertexIndex] = camera.ViewPort * startVertex;
                    if (viewPortVertices[endVertexIndex] == null)
                        viewPortVertices[endVertexIndex] = camera.ViewPort * endVertex;

                    if (drawLines)
                    {
                        DrawLine(viewPortVertices[startVertexIndex].X, viewPortVertices[startVertexIndex].Y, viewPortVertices[endVertexIndex].X, viewPortVertices[endVertexIndex].Y, drawColor);
                    }
                    else
                    {
                        DrawPixel((int)viewPortVertices[startVertexIndex].X, (int)viewPortVertices[startVertexIndex].Y, drawColor);
                        DrawPixel((int)viewPortVertices[endVertexIndex].X, (int)viewPortVertices[endVertexIndex].Y, drawColor);
                    }
                }
            }
            int stop = Environment.TickCount;
        }

        private void FillRenderBuffer(Color fillColor)
        {
            if (renderBuffer == null)
            {
                return;
            }

            int width = renderBuffer.PixelWidth;
            int height = renderBuffer.PixelHeight;
            int bytesPerPixel = (renderBuffer.Format.BitsPerPixel + 7) / 8;
            int stride = width * bytesPerPixel;
            byte[] pixelData = new byte[width * height * bytesPerPixel];

            for (int i = 0; i < pixelData.Length; i += bytesPerPixel)
            {
                pixelData[i + 2] = fillColor.R;
                pixelData[i + 1] = fillColor.G;
                pixelData[i + 0] = fillColor.B;
                pixelData[i + 3] = 0;
            }
            renderBuffer.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);
        }

        private void DrawPixel(int x, int y, Color color)
        {
            try
            {
                // Reserve the back buffer for updates
                renderBuffer.Lock();

                unsafe
                {
                    // Get a pointer to the back buffer
                    IntPtr pBackBuffer = renderBuffer.BackBuffer;

                    // Find the address of the pixel to draw
                    pBackBuffer += y * renderBuffer.BackBufferStride;
                    pBackBuffer += x * 4;

                    // Compute the pixel's color
                    int colorData = color.R << 16;
                    colorData |= color.G << 8;
                    colorData |= color.B << 0;

                    // Assign the color data to the pixel
                    *((int*)pBackBuffer) = colorData;
                }

                // Specify the area of the bitmap that changed
                renderBuffer.AddDirtyRect(new Int32Rect(x, y, 1, 1));
            }
            finally
            {
                // Release the back buffer and make it available for display
                renderBuffer.Unlock();
            }
        }

        private void DrawLine(float xStart, float yStart, float xEnd, float yEnd, Color color)
        {
            foreach (Pixel pixel in rasterizationMethod.Rasterize(xStart, yStart, xEnd, yEnd))
            {
                DrawPixel(pixel.X, pixel.Y, color);
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mouseClickPosition = e.GetPosition(this);
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point clickPosition = e.GetPosition(this);
                double deltaX = clickPosition.X - mouseClickPosition.X;
                double deltaY = clickPosition.Y - mouseClickPosition.Y;
                mouseClickPosition = clickPosition;
                camera.MoveAzimuth(-deltaX);
                camera.MoveZenith(-deltaY);
                DrawModel(model, camera);
            }
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                if (Keyboard.IsKeyDown(X_AXIS_ROTATION_KEY)) model.XAxisRotate -= rotationDelta;
                else if (Keyboard.IsKeyDown(Y_AXIS_ROTATION_KEY)) model.YAxisRotate -= rotationDelta;
                else if (Keyboard.IsKeyDown(Z_AXIS_ROTATION_KEY)) model.ZAxisRotate -= rotationDelta;
                else if (Keyboard.IsKeyDown(FOV_CHANGE_KEY)) camera.FOV += fovDelta;
                else if (Keyboard.IsKeyDown(Key.LeftCtrl)) model.Scale -= scaleDelta;
                else camera.ZoomIn();
            }
            else
            {
                if (Keyboard.IsKeyDown(X_AXIS_ROTATION_KEY)) model.XAxisRotate += rotationDelta;
                else if (Keyboard.IsKeyDown(Y_AXIS_ROTATION_KEY)) model.YAxisRotate += rotationDelta;
                else if (Keyboard.IsKeyDown(Z_AXIS_ROTATION_KEY)) model.ZAxisRotate += rotationDelta;
                else if (Keyboard.IsKeyDown(FOV_CHANGE_KEY)) camera.FOV -= fovDelta;
                else if (Keyboard.IsKeyDown(Key.LeftCtrl)) model.Scale += scaleDelta;
                else camera.ZoomOut();
            }
            DrawModel(model, camera);
        }

        private void InvertColors()
        {
            Color buffer = fillColor;
            fillColor = drawColor;
            drawColor = buffer;
            Brush labelBrush = new SolidColorBrush(drawColor);
            //lblMs.Foreground = labelBrush;
        }

        private void ChangeRasterization()
        {
            rasterizationMethod = rasterizationMethods[++rasterizationMethodIndex % rasterizationMethods.Length];
        }
    }
}
