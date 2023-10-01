using Lab1.Exceptions;
using Lab1.Information;
using Lab1.Objects;
using Lab1.Parser;
using Lab1.Rasterization;
using Microsoft.Win32;
using simple_3d_rendering;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
        private const Key FOV_CHANGE_KEY = Key.F;
        private const Key RASTERIZATION_CHANGE_KEY = Key.R;
        private const Key INFORMATION_TOGGLE_KEY = Key.I;
        private const Key HELP_TOGGLE_KEY = Key.F1;
        private const Key SCALE_KEY = Key.S;
        private const Key MOVE_KEY = Key.LeftShift;
        private const Key MOVE_STEP_KEY = Key.M;
        private const Key CONTROL_KEY = Key.LeftCtrl;
        private const Key NEAR_PLANE_DISTANCE_CHANGE_KEY = Key.N;
        private const Key FAR_PLANE_DISTANCE_CHANGE_KEY = Key.B;
        private const Key PLANE_DISTANCE_STEP_KEY = Key.P;
        private const Key VERTEX_ONLY_DRAW_MODE_KEY = Key.D0;
        private const Key WIRE_DRAW_MODE_KEY = Key.D1;
        private const Key RASTERIZATION_DRAW_MODE_KEY = Key.D2;

        private const float rotationDelta = MathF.PI / 36;

        private readonly OpenFileDialog openFileDialog;
        private readonly ObjParser parser = new();
        private Model? model = null;
        private readonly Camera camera = new();
        private RenderEngine renderEngine;
        private DrawMode drawMode = DrawMode.Wire;
        private readonly RenderInfo renderInfo = new();

        private Point mouseClickPosition;
        private static Color backgroundColor = Colors.Black;
        private static Color backgroundColorInvert = Colors.White;
        private static Color surfaceColor = Colors.White;
        private static Color lightColor = Colors.Aqua;
        private static Color rasterizedEdgeColor = Colors.Black;
        private static Color edgeColor = backgroundColorInvert;

        private int rasterizationMethodIndex = 0;
        private readonly IRasterization[] rasterizationMethods = new IRasterization[]
        {
            new Bresenham(),
            new DDALine(),
        };

        public MainWindow()
        {
            InitializeComponent();
            openFileDialog = new()
            {
                Filter = "Wavefront files (.obj)|*.obj"
            };
            tbInfo.Foreground = new SolidColorBrush(surfaceColor);
            tbHelp.Foreground = new SolidColorBrush(surfaceColor);
            tbHelp.Text = renderInfo.GetHelp();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            InitializeRenderBuffer();
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
                            renderEngine.FillRenderBuffer(backgroundColor);
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
                case VERTEX_ONLY_DRAW_MODE_KEY:
                    drawMode = DrawMode.VertexOnly;
                    break;
                case WIRE_DRAW_MODE_KEY:
                    drawMode = DrawMode.Wire;
                    edgeColor = backgroundColorInvert;
                    break;
                case RASTERIZATION_DRAW_MODE_KEY:
                    drawMode = DrawMode.Rasterization;
                    edgeColor = rasterizedEdgeColor;
                    break;
                case INVERT_COLORS_KEY:
                    InvertColors();
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
            renderEngine = new RenderEngine((int)camera.ScreenWidth, (int)camera.ScreenHeight);
            renderEngine.FillRenderBuffer(backgroundColor);
            imgScreen.Source = renderEngine.RenderBuffer;
        }

        private void DrawModel(Model? model, Camera camera)
        {
            if (model == null) return;

            int start = Environment.TickCount;
            renderEngine.DrawModel(model, camera, backgroundColor, surfaceColor, edgeColor, lightColor, drawMode);
            int stop = Environment.TickCount;

            // Show render information
            renderInfo.RenderTime = stop - start;
            tbInfo.Text = renderInfo.GetInfomation(model, camera, renderEngine.Rasterization);
            tbHelp.Text = renderInfo.GetHelp();
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
                else if (Keyboard.IsKeyDown(FOV_CHANGE_KEY)) camera.FOV -= camera.FovStep;
                else if (Keyboard.IsKeyDown(SCALE_KEY) && Keyboard.IsKeyDown(CONTROL_KEY)) model.DecreaseScaleStep();
                else if (Keyboard.IsKeyDown(SCALE_KEY)) model.Scale -= model.ScaleStep;
                else if (Keyboard.IsKeyDown(CONTROL_KEY)) camera.DecreaseZoomStep();
                else if (Keyboard.IsKeyDown(MOVE_STEP_KEY)) model.DecreaseMoveStep();
                else if (Keyboard.IsKeyDown(NEAR_PLANE_DISTANCE_CHANGE_KEY)) camera.ZNear -= camera.PlaneDistanceStep;
                else if (Keyboard.IsKeyDown(FAR_PLANE_DISTANCE_CHANGE_KEY)) camera.ZFar -= camera.PlaneDistanceStep;
                else if (Keyboard.IsKeyDown(PLANE_DISTANCE_STEP_KEY)) camera.DecreasePlaneDistanceStep();
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
                else if (Keyboard.IsKeyDown(FOV_CHANGE_KEY)) camera.FOV += camera.FovStep;
                else if (Keyboard.IsKeyDown(SCALE_KEY) && Keyboard.IsKeyDown(CONTROL_KEY)) model.IncreaseScaleStep();
                else if (Keyboard.IsKeyDown(SCALE_KEY)) model.Scale += model.ScaleStep;
                else if (Keyboard.IsKeyDown(CONTROL_KEY)) camera.IncreaseZoomStep();
                else if (Keyboard.IsKeyDown(MOVE_STEP_KEY)) model.IncreaseMoveStep();
                else if (Keyboard.IsKeyDown(NEAR_PLANE_DISTANCE_CHANGE_KEY)) camera.ZNear += camera.PlaneDistanceStep;
                else if (Keyboard.IsKeyDown(FAR_PLANE_DISTANCE_CHANGE_KEY)) camera.ZFar += camera.PlaneDistanceStep;
                else if (Keyboard.IsKeyDown(PLANE_DISTANCE_STEP_KEY)) camera.IncreasePlaneDistanceStep();
                else camera.ZoomOut();
            }
            DrawModel(model, camera);
        }

        private void InvertColors()
        {
            (backgroundColor, backgroundColorInvert) = (backgroundColorInvert, backgroundColor);

            if (drawMode != DrawMode.Rasterization)
            {
                edgeColor = backgroundColorInvert;
            }

            Brush textBrush = new SolidColorBrush(backgroundColorInvert);
            tbInfo.Foreground = textBrush;
            tbHelp.Foreground = textBrush;
        }

        private void ChangeRasterization()
        {
            renderEngine.Rasterization = rasterizationMethods[++rasterizationMethodIndex % rasterizationMethods.Length];
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
