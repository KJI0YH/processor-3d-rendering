using Lab1.Objects;
using Lab1.Parser;
using Lab1.Primitives;
using Lab1.Rasterization;
using Microsoft.Win32;
using System;
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
        private const Key OPEN_FILE_KEY = Key.O;
        private const Key CLOSE_APP_KEY = Key.Escape;

        private OpenFileDialog openFileDialog;
        private ObjParser parser;
        private WriteableBitmap renderBuffer;

        private Color fillColor = Colors.Black;
        private Color drawColor = Colors.White;

        private DDALine DDALineRasterization = new DDALine();
        private Bresenham BresenhamRasterizaton = new Bresenham();
        private IRasterization rasterization;

        public MainWindow()
        {
            InitializeComponent();
            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Wavefront files (.obj)|*.obj";
            parser = new ObjParser();
            rasterization = BresenhamRasterizaton;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            InitializeRenderBuffer();
            FillRenderBuffer(fillColor);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitializeRenderBuffer();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case OPEN_FILE_KEY:
                    if (openFileDialog.ShowDialog() == true)
                    {
                        Model model = parser.Parse(openFileDialog.FileName);
                        Camera camera = new Camera();
                        DrawModel(model, camera);
                    }
                    break;
                case CLOSE_APP_KEY:
                    Application.Current.Shutdown();
                    break;
                default:
                    break;
            }
        }

        private void InitializeRenderBuffer()
        {
            renderBuffer = new WriteableBitmap((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Bgr32, null);
            imgScreen.Source = renderBuffer;
        }

        private void DrawModel(Model model, Camera camera)
        {
            double width = ActualWidth;
            double height = ActualHeight;
            foreach (var vertex in model.Vertices)
            {
                // Local to world
                vertex.Update(Matrix4.Move(new Vector3(0, 0, 0)) * vertex);

                // World to view
                vertex.Update(camera.View() * vertex);

                // View to clip
                vertex.Update(Matrix4.Projection(MathF.PI / 2, (float)(width / height), 0, 100) * vertex);
                vertex.Update(vertex / vertex.W);

                // Clip to screen
                // TODO check that point in screen
                //vertex.Update(Matrix4.Viewport(width, heigth, 0, 0) * vertex);
            }

            FillRenderBuffer(fillColor);

            DrawLine(0, 0, (int)width - 1, (int)height - 1, drawColor);
            DrawLine(0, (int)height - 1, (int)width - 1, 0, drawColor);
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

        private void DrawLine(int xStart, int yStart, int xEnd, int yEnd, Color color)
        {
            foreach (Pixel pixel in rasterization.Rasterize(xStart, yStart, xEnd, yEnd))
            {
                DrawPixel(pixel.X, pixel.Y, color);
            }
        }
    }
}
