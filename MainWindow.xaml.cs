using Microsoft.Win32;
using Rendering.Engine;
using Rendering.Exceptions;
using Rendering.Information;
using Rendering.Objects;
using Rendering.Parser;
using Rendering.Rasterisation;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Rendering;

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
    private const Key RASTERISATION_CHANGE_KEY = Key.R;
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
    private const Key RASTERISATION_DRAW_MODE_KEY = Key.D2;
    private const Key PHONG_SHADING_DRAW_MODE_KEY = Key.D3;
    private const Key PHONG_LIGHTING_DRAW_MODE_KEY = Key.D4;
    private const Key CAMERA_RESET_KEY = Key.Home;
    private const Key MOVE_UP_KEY = Key.Up;
    private const Key MOVE_RIGHT_KEY = Key.Right;
    private const Key MOVE_DOWN_KEY = Key.Down;
    private const Key MOVE_LEFT_KEY = Key.Left;

    private readonly OpenFileDialog _openFileDialog;
    private readonly ObjParser _parser = new();
    private Model? _model;
    private readonly Camera _camera = new();
    private RenderEngine _renderEngine;
    private DrawMode _drawMode = DrawMode.Wire;
    private readonly RenderInfo _renderInfo = new();

    private Point _mouseClickPosition;
    private static Color _backgroundColor = Colors.Black;
    private static Color _backgroundColorInvert = Colors.White;
    private static Color _surfaceColor = Colors.White;
    private static Color _lightColor = Colors.White;
    private static Color _rasterizedEdgeColor = Colors.Black;
    private static Color _edgeColor = _backgroundColorInvert;

    private int _rasterisationMethodIndex = 0;

    private readonly IRasterisation[] _rasterisationMethods = new IRasterisation[]
    {
        new Bresenham(),
        new DDALine()
    };

    public MainWindow()
    {
        InitializeComponent();
        _openFileDialog = new OpenFileDialog
        {
            Filter = "Wavefront files (.obj)|*.obj"
        };
        tbInfo.Foreground = new SolidColorBrush(_surfaceColor);
        tbHelp.Foreground = new SolidColorBrush(_surfaceColor);
        tbHelp.Text = _renderInfo.GetHelp();
    }

    private void Window_ContentRendered(object sender, EventArgs e)
    {
        InitializeRenderBuffer();
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        _camera.ScreenWidth = (float)ActualWidth;
        _camera.ScreenHeight = (float)ActualHeight;
        InitializeRenderBuffer();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case OPEN_FILE_KEY:
                if (_openFileDialog.ShowDialog() == true)
                {
                    _model = null;
                    var filename = _openFileDialog.FileName;
                    try
                    {
                        // Clear background
                        _renderEngine.FillRenderBuffer(_backgroundColor);
                        _model = _parser.Parse(filename);
                        _model.MoveToWorldCenter();
                        _camera.SetInitialPosition(_model);
                    }
                    catch (ParserException exception)
                    {
                        // Show error message
                        tbInfo.Visibility = Visibility.Visible;
                        tbInfo.Text = _renderInfo.GetParseError(filename, exception.Message);
                    }
                }

                break;
            case VERTEX_ONLY_DRAW_MODE_KEY:
                _renderEngine.DrawMode = DrawMode.VertexOnly;
                break;
            case WIRE_DRAW_MODE_KEY:
                _renderEngine.DrawMode = DrawMode.Wire;
                _edgeColor = _backgroundColorInvert;
                break;
            case RASTERISATION_DRAW_MODE_KEY:
                _renderEngine.DrawMode = DrawMode.Rasterisation;
                break;
            case PHONG_SHADING_DRAW_MODE_KEY:
                _renderEngine.DrawMode = DrawMode.PhongShading;
                break;
            case INVERT_COLORS_KEY:
                InvertColors();
                break;
            case RASTERISATION_CHANGE_KEY:
                ChangeRasterisation();
                break;
            case CAMERA_RESET_KEY:
                if (_model != null)
                {
                    _model.SetInitialPosition();
                    _camera.SetInitialPosition(_model);
                }

                break;
            case INFORMATION_TOGGLE_KEY:
                ToggleVisibility(tbInfo);
                break;
            case HELP_TOGGLE_KEY:
                ToggleVisibility(tbHelp);
                tbHelp.Text = _renderInfo.GetHelp();
                break;
            case MOVE_UP_KEY:
                _camera.ZenithAngle -= _camera.KeyRotation;
                break;
            case MOVE_RIGHT_KEY:
                _camera.AzimuthAngle += _camera.KeyRotation;
                break;
            case MOVE_DOWN_KEY:
                _camera.ZenithAngle += _camera.KeyRotation;
                break;
            case MOVE_LEFT_KEY:
                _camera.AzimuthAngle -= _camera.KeyRotation;
                break;
            case CLOSE_APP_KEY:
                Application.Current.Shutdown();
                break;
        }

        DrawModel(_model, _camera);
    }

    private void InitializeRenderBuffer()
    {
        _renderEngine = new RenderEngine((int)_camera.ScreenWidth, (int)_camera.ScreenHeight);
        _renderEngine.FillRenderBuffer(_backgroundColor);
        imgScreen.Source = _renderEngine.RenderBuffer;
    }

    private void DrawModel(Model? model, Camera camera)
    {
        if (model == null) return;

        var start = Environment.TickCount;
        _renderEngine.DrawModel(model, camera, _backgroundColor, _surfaceColor, _edgeColor, _lightColor);
        var stop = Environment.TickCount;

        // Show render information
        _renderInfo.RenderTime = stop - start;
        tbInfo.Text = _renderInfo.GetInformation(_renderEngine, model, camera);
        tbHelp.Text = _renderInfo.GetHelp();
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _mouseClickPosition = e.GetPosition(this);
    }

    private void Window_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            var clickPosition = e.GetPosition(this);
            var deltaX = (float)(_mouseClickPosition.X - clickPosition.X);
            var deltaY = (float)(_mouseClickPosition.Y - clickPosition.Y);
            _mouseClickPosition = clickPosition;
            _camera.AzimuthAngle += deltaX * _camera.AngleDelta;
            _camera.ZenithAngle += deltaY * _camera.AngleDelta;
            DrawModel(_model, _camera);
        }
    }

    private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (_model == null) return;
        if (e.Delta < 0)
        {
            if (Keyboard.IsKeyDown(X_CONTROL_KEY) && Keyboard.IsKeyDown(MOVE_KEY)) _model.XPosition -= _model.MoveStep;
            else if (Keyboard.IsKeyDown(Y_CONTROL_KEY) && Keyboard.IsKeyDown(MOVE_KEY))
                _model.YPosition -= _model.MoveStep;
            else if (Keyboard.IsKeyDown(Z_CONTROL_KEY) && Keyboard.IsKeyDown(MOVE_KEY))
                _model.ZPosition -= _model.MoveStep;
            else if (Keyboard.IsKeyDown(X_CONTROL_KEY)) _model.XAxisRotate -= Model.MOUSE_ROTATION_DELTA;
            else if (Keyboard.IsKeyDown(Y_CONTROL_KEY)) _model.YAxisRotate -= Model.MOUSE_ROTATION_DELTA;
            else if (Keyboard.IsKeyDown(Z_CONTROL_KEY)) _model.ZAxisRotate -= Model.MOUSE_ROTATION_DELTA;
            else if (Keyboard.IsKeyDown(FOV_CHANGE_KEY)) _camera.FOV -= _camera.FovStep;
            else if (Keyboard.IsKeyDown(SCALE_KEY) && Keyboard.IsKeyDown(CONTROL_KEY)) _model.DecreaseScaleStep();
            else if (Keyboard.IsKeyDown(SCALE_KEY)) _model.Scale -= _model.ScaleStep;
            else if (Keyboard.IsKeyDown(CONTROL_KEY)) _camera.DecreaseZoomStep();
            else if (Keyboard.IsKeyDown(MOVE_STEP_KEY)) _model.DecreaseMoveStep();
            else if (Keyboard.IsKeyDown(NEAR_PLANE_DISTANCE_CHANGE_KEY)) _camera.ZNear -= _camera.PlaneDistanceStep;
            else if (Keyboard.IsKeyDown(FAR_PLANE_DISTANCE_CHANGE_KEY)) _camera.ZFar -= _camera.PlaneDistanceStep;
            else if (Keyboard.IsKeyDown(PLANE_DISTANCE_STEP_KEY)) _camera.DecreasePlaneDistanceStep();
            else _camera.ZoomIn();
        }
        else
        {
            if (Keyboard.IsKeyDown(X_CONTROL_KEY) && Keyboard.IsKeyDown(MOVE_KEY)) _model.XPosition += _model.MoveStep;
            else if (Keyboard.IsKeyDown(Y_CONTROL_KEY) && Keyboard.IsKeyDown(MOVE_KEY))
                _model.YPosition += _model.MoveStep;
            else if (Keyboard.IsKeyDown(Z_CONTROL_KEY) && Keyboard.IsKeyDown(MOVE_KEY))
                _model.ZPosition += _model.MoveStep;
            else if (Keyboard.IsKeyDown(X_CONTROL_KEY)) _model.XAxisRotate += Model.MOUSE_ROTATION_DELTA;
            else if (Keyboard.IsKeyDown(Y_CONTROL_KEY)) _model.YAxisRotate += Model.MOUSE_ROTATION_DELTA;
            else if (Keyboard.IsKeyDown(Z_CONTROL_KEY)) _model.ZAxisRotate += Model.MOUSE_ROTATION_DELTA;
            else if (Keyboard.IsKeyDown(FOV_CHANGE_KEY)) _camera.FOV += _camera.FovStep;
            else if (Keyboard.IsKeyDown(SCALE_KEY) && Keyboard.IsKeyDown(CONTROL_KEY)) _model.IncreaseScaleStep();
            else if (Keyboard.IsKeyDown(SCALE_KEY)) _model.Scale += _model.ScaleStep;
            else if (Keyboard.IsKeyDown(CONTROL_KEY)) _camera.IncreaseZoomStep();
            else if (Keyboard.IsKeyDown(MOVE_STEP_KEY)) _model.IncreaseMoveStep();
            else if (Keyboard.IsKeyDown(NEAR_PLANE_DISTANCE_CHANGE_KEY)) _camera.ZNear += _camera.PlaneDistanceStep;
            else if (Keyboard.IsKeyDown(FAR_PLANE_DISTANCE_CHANGE_KEY)) _camera.ZFar += _camera.PlaneDistanceStep;
            else if (Keyboard.IsKeyDown(PLANE_DISTANCE_STEP_KEY)) _camera.IncreasePlaneDistanceStep();
            else _camera.ZoomOut();
        }

        DrawModel(_model, _camera);
    }

    private void InvertColors()
    {
        (_backgroundColor, _backgroundColorInvert) = (_backgroundColorInvert, _backgroundColor);

        if (_drawMode != DrawMode.Rasterisation) _edgeColor = _backgroundColorInvert;

        Brush textBrush = new SolidColorBrush(_backgroundColorInvert);
        tbInfo.Foreground = textBrush;
        tbHelp.Foreground = textBrush;
    }

    private void ChangeRasterisation()
    {
        _renderEngine.Rasterisation = _rasterisationMethods[++_rasterisationMethodIndex % _rasterisationMethods.Length];
    }

    private void ToggleVisibility(TextBlock textBlock)
    {
        textBlock.Visibility = textBlock.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
    }
}