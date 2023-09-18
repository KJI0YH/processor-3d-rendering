using Lab1.Exceptions;
using Lab1.Information;
using Lab1.Objects;
using Lab1.Parser;
using Lab1.Primitives;
using Lab1.Rasterization;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
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
        private const Key X_CONTROL_KEY = Key.X;
        private const Key Y_CONTROL_KEY = Key.Y;
        private const Key Z_CONTROL_KEY = Key.Z;
        private const Key LINES_TOGGLE_KEY = Key.L;
        private const Key FOV_CHANGE_KEY = Key.F;
        private const Key RASTERIZATION_CHANGE_KEY = Key.R;
        private const Key INFORMATION_TOGGLE_KEY = Key.I;
        private const Key HELP_TOGGLE_KEY = Key.F1;
        private const Key SCALE_KEY = Key.S;
        private const Key MOVE_KEY = Key.LeftShift;
        private const Key MOVE_STEP_KEY = Key.M;
        private const Key CONTROL_KEY = Key.LeftCtrl;

        private const float rotationDelta = MathF.PI / 36;
        private bool drawLines = true;

        private OpenFileDialog openFileDialog;
        private ObjParser parser = new ObjParser();
        private WriteableBitmap renderBuffer;
        private Model? model = null;
        private Camera camera = new Camera();
        private RenderInfo renderInfo = new RenderInfo();

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
            tbInfo.Foreground = new SolidColorBrush(drawColor);
            tbHelp.Foreground = new SolidColorBrush(drawColor);
            tbHelp.Text = renderInfo.GetHelp();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            InitializeRenderBuffer();
            FillRenderBuffer(fillColor);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            camera.ScreenWidth = (float)ActualWidth;
            camera.ScreenHeight = (float)ActualHeight;
            InitializeRenderBuffer();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case OPEN_FILE_KEY:
                    if (openFileDialog.ShowDialog() == true)
                    {
                        model = null;
                        string filename = openFileDialog.FileName;
                        try
                        {
                            // Clear backround
                            FillRenderBuffer(fillColor);
                            model = parser.Parse(filename);
                        }
                        catch (ParserException exception)
                        {
                            // Show error message
                            tbInfo.Visibility = Visibility.Visible;
                            tbInfo.Text = renderInfo.GetParseError(filename, exception.Message);
                        }

                        // Resetting the camera to initial position
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
                case INFORMATION_TOGGLE_KEY:
                    ToggleVisibility(tbInfo);
                    break;
                case HELP_TOGGLE_KEY:
                    ToggleVisibility(tbHelp);
                    tbHelp.Text = renderInfo.GetHelp();
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
            renderBuffer = new WriteableBitmap((int)camera.ScreenWidth, (int)camera.ScreenHeight, 96, 96, PixelFormats.Bgr32, null);
            imgScreen.Source = renderBuffer;
        }

        private void DrawModel(Model? model, Camera camera)
        {
            // Fill background
            FillRenderBuffer(fillColor);

            if (model == null || camera == null)
            {
                return;
            }

            // Projection of each vertex of the model
            List<Vector4> projectedVertices = new List<Vector4>();
            int start = Environment.TickCount;
            foreach (var vertex in model.Vertices)
            {
                Vector4 projectedVertex = Vector4.Transform(Vector4.Transform(Vector4.Transform(vertex, model.Transformation), camera.View), camera.Projection);
                projectedVertex /= projectedVertex.W;
                projectedVertices.Add(projectedVertex);
            }

            // Drawing of each visible polygon
            Vector4?[] viewPortVertices = new Vector4?[projectedVertices.Count];
            foreach (var polygon in model.Polygons)
            {
                for (int i = 0; i < polygon.Indices.Count; i++)
                {
                    int startVertexIndex = polygon.Indices[i];
                    int endVertexIndex = polygon.Indices[(i + 1) % polygon.Indices.Count];
                    Vector4 startVertex = projectedVertices[startVertexIndex];
                    Vector4 endVertex = projectedVertices[endVertexIndex];

                    // Check if the vertices are visible on the screen
                    if (startVertex.X < -1 || startVertex.X > 1 || startVertex.Y < -1 || startVertex.Y > 1 || startVertex.Z < -1 || startVertex.Z > 1) continue;
                    if (endVertex.X < -1 || endVertex.X > 1 || endVertex.Y < -1 || endVertex.Y > 1 || endVertex.Z < -1 || endVertex.Z > 1) continue;

                    // Defining screen coordinates of a vertex, if it has not been processed yet
                    if (viewPortVertices[startVertexIndex] == null)
                        viewPortVertices[startVertexIndex] = Vector4.Transform(startVertex, camera.ViewPort);
                    if (viewPortVertices[endVertexIndex] == null)
                        viewPortVertices[endVertexIndex] = Vector4.Transform(endVertex, camera.ViewPort);

                    // Line drawing
                    if (drawLines)
                    {
                        DrawLine(viewPortVertices[startVertexIndex].Value.X, viewPortVertices[startVertexIndex].Value.Y, viewPortVertices[endVertexIndex].Value.X, viewPortVertices[endVertexIndex].Value.Y, drawColor);
                    }

                    // Only points drawing
                    else
                    {
                        DrawLine(viewPortVertices[startVertexIndex].Value.X, viewPortVertices[startVertexIndex].Value.Y, viewPortVertices[startVertexIndex].Value.X, viewPortVertices[startVertexIndex].Value.Y, drawColor);
                        DrawLine(viewPortVertices[endVertexIndex].Value.X, viewPortVertices[endVertexIndex].Value.Y, viewPortVertices[endVertexIndex].Value.X, viewPortVertices[endVertexIndex].Value.Y, drawColor);
                    }
                }
            }
            int stop = Environment.TickCount;

            // Show render information
            renderInfo.RenderTime = stop - start;
            tbInfo.Text = renderInfo.GetInfomation(model, camera, rasterizationMethod);
            tbHelp.Text = renderInfo.GetHelp();
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

        private void DrawLine(float xStart, float yStart, float xEnd, float yEnd, Color color)
        {
            try
            {
                // Reserve the back buffer for updates
                renderBuffer.Lock();

                foreach (Pixel pixel in rasterizationMethod.Rasterize(xStart, yStart, xEnd, yEnd))
                {
                    DrawPixel(pixel.X, pixel.Y, drawColor);
                }
            }
            finally
            {
                // Release the back buffer and make it available for display
                renderBuffer.Unlock();
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
            if (model == null)
            {
                return;
            }
            if (e.Delta < 0)
            {
                if (Keyboard.IsKeyDown(X_CONTROL_KEY) && Keyboard.IsKeyDown(MOVE_KEY)) model.MoveByX(-model.MoveStep);
                else if (Keyboard.IsKeyDown(Y_CONTROL_KEY) && Keyboard.IsKeyDown(MOVE_KEY)) model.MoveByY(-model.MoveStep);
                else if (Keyboard.IsKeyDown(Z_CONTROL_KEY) && Keyboard.IsKeyDown(MOVE_KEY)) model.MoveByZ(-model.MoveStep);
                else if (Keyboard.IsKeyDown(X_CONTROL_KEY)) model.XAxisRotate -= rotationDelta;
                else if (Keyboard.IsKeyDown(Y_CONTROL_KEY)) model.YAxisRotate -= rotationDelta;
                else if (Keyboard.IsKeyDown(Z_CONTROL_KEY)) model.ZAxisRotate -= rotationDelta;
                else if (Keyboard.IsKeyDown(FOV_CHANGE_KEY)) camera.FOV -= camera.fovStep;
                else if (Keyboard.IsKeyDown(SCALE_KEY) && Keyboard.IsKeyDown(CONTROL_KEY)) model.DecreaseScaleStep();
                else if (Keyboard.IsKeyDown(SCALE_KEY)) model.Scale -= model.ScaleStep;
                else if (Keyboard.IsKeyDown(CONTROL_KEY)) camera.DecreaseZoomStep();
                else if (Keyboard.IsKeyDown(MOVE_STEP_KEY)) model.DecreaseMoveStep();
                else camera.ZoomIn();
            }
            else
            {
                if (Keyboard.IsKeyDown(X_CONTROL_KEY) && Keyboard.IsKeyDown(MOVE_KEY)) model.MoveByX(model.MoveStep);
                else if (Keyboard.IsKeyDown(Y_CONTROL_KEY) && Keyboard.IsKeyDown(MOVE_KEY)) model.MoveByY(model.MoveStep);
                else if (Keyboard.IsKeyDown(Z_CONTROL_KEY) && Keyboard.IsKeyDown(MOVE_KEY)) model.MoveByZ(model.MoveStep);
                else if (Keyboard.IsKeyDown(X_CONTROL_KEY)) model.XAxisRotate += rotationDelta;
                else if (Keyboard.IsKeyDown(Y_CONTROL_KEY)) model.YAxisRotate += rotationDelta;
                else if (Keyboard.IsKeyDown(Z_CONTROL_KEY)) model.ZAxisRotate += rotationDelta;
                else if (Keyboard.IsKeyDown(FOV_CHANGE_KEY)) camera.FOV += camera.fovStep;
                else if (Keyboard.IsKeyDown(SCALE_KEY) && Keyboard.IsKeyDown(CONTROL_KEY)) model.IncreaseScaleStep();
                else if (Keyboard.IsKeyDown(SCALE_KEY)) model.Scale += model.ScaleStep;
                else if (Keyboard.IsKeyDown(CONTROL_KEY)) camera.IncreaseZoomStep();
                else if (Keyboard.IsKeyDown(MOVE_STEP_KEY)) model.IncreaseMoveStep();
                else camera.ZoomOut();
            }
            DrawModel(model, camera);
        }

        private void InvertColors()
        {
            Color buffer = fillColor;
            fillColor = drawColor;
            drawColor = buffer;
            Brush textBrush = new SolidColorBrush(drawColor);
            tbInfo.Foreground = textBrush;
            tbHelp.Foreground = textBrush;
        }

        private void ChangeRasterization()
        {
            rasterizationMethod = rasterizationMethods[++rasterizationMethodIndex % rasterizationMethods.Length];
        }

        private void ToggleVisibility(TextBlock textBlock)
        {
            if (textBlock.Visibility == Visibility.Visible)
            {
                textBlock.Visibility = Visibility.Hidden;
            }
            else
            {
                textBlock.Visibility = Visibility.Visible;
            }
        }
    }
}
