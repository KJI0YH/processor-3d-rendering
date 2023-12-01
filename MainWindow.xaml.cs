using Microsoft.Win32;
using Rendering.Engine;
using Rendering.Exceptions;
using Rendering.Information;
using Rendering.Objects;
using Rendering.Parser;
using Rendering.Rasterisation;
using System;
using System.Collections.Generic;
using System.IO;
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
    public const Key CLOSE_APP_KEY = Key.Escape;
    public const Key OPEN_MODEL_KEY = Key.O;
    public const Key OPEN_TEXTURE_KEY = Key.T;
    public const Key INVERT_COLORS_KEY = Key.C;
    public const Key X_CONTROL_KEY = Key.X;
    public const Key Y_CONTROL_KEY = Key.Y;
    public const Key Z_CONTROL_KEY = Key.Z;
    public const Key FOV_CHANGE_KEY = Key.F;
    public const Key RASTERISATION_CHANGE_KEY = Key.Q;
    public const Key INFORMATION_TOGGLE_KEY = Key.I;
    public const Key HELP_TOGGLE_KEY = Key.F1;
    public const Key ERRORS_TOGGLE_KEY = Key.E;
    public const Key SCALE_KEY = Key.W;
    public const Key MOVE_KEY = Key.LeftShift;
    public const Key MOVE_STEP_KEY = Key.M;
    public const Key CONTROL_KEY = Key.LeftCtrl;
    public const Key NEAR_PLANE_DISTANCE_CHANGE_KEY = Key.N;
    public const Key FAR_PLANE_DISTANCE_CHANGE_KEY = Key.J;
    public const Key PLANE_DISTANCE_STEP_KEY = Key.P;
    public const Key VERTEX_ONLY_DRAW_MODE_KEY = Key.D0;
    public const Key WIRE_DRAW_MODE_KEY = Key.D1;
    public const Key RASTERISATION_DRAW_MODE_KEY = Key.D2;
    public const Key PHONG_SHADING_DRAW_MODE_KEY = Key.D3;
    public const Key PHONG_LIGHTING_DRAW_MODE_KEY = Key.D4;
    public const Key TEXTURE_DRAW_MODE_KEY = Key.D5;
    public const Key CUSTOM_TEXTURE_DRAW_MODE_KEY = Key.D6;
    public const Key CAMERA_RESET_KEY = Key.Home;
    public const Key MOVE_UP_KEY = Key.Up;
    public const Key MOVE_RIGHT_KEY = Key.Right;
    public const Key MOVE_DOWN_KEY = Key.Down;
    public const Key MOVE_LEFT_KEY = Key.Left;
    public const Key RED_CONTROL_KEY = Key.R;
    public const Key GREEN_CONTROL_KEY = Key.G;
    public const Key BLUE_CONTROL_KEY = Key.B;
    public const Key AMBIENT_CONTROL_KEY = Key.A;
    public const Key DIFFUSE_CONTROL_KEY = Key.D;
    public const Key SPECULAR_CONTROL_KEY = Key.S;
    public const Key SHININESS_CONTROL_KEY = Key.H;

    private readonly OpenFileDialog _openObjFileDialog;
    private readonly OpenFileDialog _openTextureFileDialog;
    private readonly ObjParser _modelParser = new();
    private readonly ImageParser _imageParser = new();
    private Model? _model;
    private readonly Camera _camera = new();
    private RenderEngine _renderEngine = new();
    private readonly RenderInfo _renderInfo = new();
    private Point _mouseClickPosition;
    private List<Key> _pressedKeys = new();

    public MainWindow()
    {
        InitializeComponent();
        _openObjFileDialog = new OpenFileDialog
        {
            Filter = "Wavefront files (.obj)|*.obj"
        };
        _openTextureFileDialog = new OpenFileDialog
        {
            Filter = "Texture files|*.jpg;*.png;*.bmp;*.jpeg;|All files|*.*"
        };
        imgScreen.Source = _renderEngine.RenderBuffer;
        tbInfo.Foreground = new SolidColorBrush(_renderEngine.Background.InvertColor);
        tbInfo.Text = _renderInfo.GetInformation(_renderEngine, _model, _camera);
        tbHelp.Foreground = new SolidColorBrush(_renderEngine.Background.InvertColor);
        tbHelp.Text = _renderInfo.HelpInfo;
    }

    private void Window_ContentRendered(object sender, EventArgs e)
    {
        ChangeSize();
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ChangeSize();
    }

    private void ChangeSize()
    {
        _camera.ScreenWidth = (float)ActualWidth;
        _camera.ScreenHeight = (float)ActualHeight;
        _renderEngine.ChangeSize((int)ActualWidth, (int)ActualHeight);
        imgScreen.Source = _renderEngine.RenderBuffer;
    }

    private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
    {
        _pressedKeys.Remove(e.Key);
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (!_pressedKeys.Contains(e.Key)) _pressedKeys.Add(e.Key);
        switch (e.Key)
        {
            case OPEN_MODEL_KEY:
                if (_openObjFileDialog.ShowDialog() == true)
                {
                    _model = null;
                    var filePath = _openObjFileDialog.FileName;
                    try
                    {
                        // Clear background
                        _renderEngine.Clear();
                        _model = _modelParser.Parse(filePath);
                        _model.MoveToWorldCenter();
                        _camera.SetInitialPosition(_model);
                    }
                    catch (ParserException exception)
                    {
                    }

                    // Show error message
                    tbError.Visibility = Visibility.Visible;
                    tbError.Text = _renderInfo.GetParseError(_modelParser.GetErrors());
                }

                _pressedKeys.Clear();

                break;
            case OPEN_TEXTURE_KEY:
                if (_openTextureFileDialog.ShowDialog() == true)
                {
                    var filePath = _openTextureFileDialog.FileName;
                    try
                    {
                        _renderEngine.CustomTexture = _imageParser.Parse(filePath);
                    }
                    catch (FileNotFoundException exception)
                    {
                        tbError.Visibility = Visibility.Visible;
                        tbError.Text = _renderInfo.GetParseError(exception.Message);
                    }
                }

                _pressedKeys.Clear();

                break;
            case VERTEX_ONLY_DRAW_MODE_KEY:
                _renderEngine.DrawMode = DrawMode.VertexOnly;
                break;
            case WIRE_DRAW_MODE_KEY:
                _renderEngine.DrawMode = DrawMode.Wire;
                break;
            case RASTERISATION_DRAW_MODE_KEY:
                _renderEngine.DrawMode = DrawMode.Rasterisation;
                break;
            case PHONG_SHADING_DRAW_MODE_KEY:
                _renderEngine.DrawMode = DrawMode.PhongShading;
                break;
            case PHONG_LIGHTING_DRAW_MODE_KEY:
                _renderEngine.DrawMode = DrawMode.PhongLighting;
                break;
            case TEXTURE_DRAW_MODE_KEY:
                _renderEngine.DrawMode = DrawMode.Texture;
                break;
            case CUSTOM_TEXTURE_DRAW_MODE_KEY:
                _renderEngine.DrawMode = DrawMode.Custom;
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
                break;
            case ERRORS_TOGGLE_KEY:
                ToggleVisibility(tbError);
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

    private void DrawModel(Model? model, Camera camera)
    {
        if (model == null) return;

        var start = Environment.TickCount;
        _renderEngine.DrawModel(model, camera);
        var stop = Environment.TickCount;

        // Show render information
        _renderInfo.RenderTime = stop - start;
        tbInfo.Text = _renderInfo.GetInformation(_renderEngine, model, camera);
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _mouseClickPosition = e.GetPosition(this);
    }

    private void Window_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;
        var clickPosition = e.GetPosition(this);
        var deltaX = (float)(_mouseClickPosition.X - clickPosition.X);
        var deltaY = (float)(_mouseClickPosition.Y - clickPosition.Y);
        _mouseClickPosition = clickPosition;
        _camera.AzimuthAngle += deltaX * _camera.AngleDelta;
        _camera.ZenithAngle += deltaY * _camera.AngleDelta;
        DrawModel(_model, _camera);
    }

    private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (_model == null) return;
        var delta = e.Delta > 0 ? 1 : -1;
        if (e.Delta < 0)
        {
            if (Keyboard.IsKeyDown(X_CONTROL_KEY) && Keyboard.IsKeyDown(MOVE_KEY)) _model.XPosition -= Model.MOVE_STEP;
            else if (Keyboard.IsKeyDown(Y_CONTROL_KEY) && Keyboard.IsKeyDown(MOVE_KEY))
                _model.YPosition -= Model.MOVE_STEP;
            else if (Keyboard.IsKeyDown(Z_CONTROL_KEY) && Keyboard.IsKeyDown(MOVE_KEY))
                _model.ZPosition -= Model.MOVE_STEP;
            else if (Keyboard.IsKeyDown(X_CONTROL_KEY)) _model.XAxisRotate -= Model.MOUSE_ROTATION_DELTA;
            else if (Keyboard.IsKeyDown(Y_CONTROL_KEY)) _model.YAxisRotate -= Model.MOUSE_ROTATION_DELTA;
            else if (Keyboard.IsKeyDown(Z_CONTROL_KEY)) _model.ZAxisRotate -= Model.MOUSE_ROTATION_DELTA;
            else if (Keyboard.IsKeyDown(FOV_CHANGE_KEY)) _camera.FOV -= _camera.FovStep;
            else if (Keyboard.IsKeyDown(SCALE_KEY) && Keyboard.IsKeyDown(CONTROL_KEY)) _model.DecreaseScaleStep();
            else if (Keyboard.IsKeyDown(SCALE_KEY)) _model.Scale -= Model.SCALE_STEP;
            else if (Keyboard.IsKeyDown(MOVE_STEP_KEY)) _model.DecreaseMoveStep();
            else if (Keyboard.IsKeyDown(NEAR_PLANE_DISTANCE_CHANGE_KEY)) _camera.ZNear -= _camera.PlaneDistanceStep;
            else if (Keyboard.IsKeyDown(FAR_PLANE_DISTANCE_CHANGE_KEY)) _camera.ZFar -= _camera.PlaneDistanceStep;
            else if (Keyboard.IsKeyDown(PLANE_DISTANCE_STEP_KEY)) _camera.DecreasePlaneDistanceStep();
            else if (Keyboard.IsKeyDown(AMBIENT_CONTROL_KEY) && Keyboard.IsKeyDown(CONTROL_KEY))
                _renderEngine.KAmbient -= RenderEngine.K_STEP;
            else if (Keyboard.IsKeyDown(DIFFUSE_CONTROL_KEY) && Keyboard.IsKeyDown(CONTROL_KEY))
                _renderEngine.KDiffuse -= RenderEngine.K_STEP;
            else if (Keyboard.IsKeyDown(SPECULAR_CONTROL_KEY) && Keyboard.IsKeyDown(CONTROL_KEY))
                _renderEngine.KSpecular -= RenderEngine.K_STEP;
            else if (Keyboard.IsKeyDown(SHININESS_CONTROL_KEY) && Keyboard.IsKeyDown(CONTROL_KEY))
                _renderEngine.KShininess -= RenderEngine.K_STEP;
            else if (Keyboard.IsKeyDown(AMBIENT_CONTROL_KEY) && Keyboard.IsKeyDown(RED_CONTROL_KEY))
                _renderEngine.Ambient.R -= RenderEngine.COLOR_STEP;
            else if (Keyboard.IsKeyDown(AMBIENT_CONTROL_KEY) && Keyboard.IsKeyDown(GREEN_CONTROL_KEY))
                _renderEngine.Ambient.G -= RenderEngine.COLOR_STEP;
            else if (Keyboard.IsKeyDown(AMBIENT_CONTROL_KEY) && Keyboard.IsKeyDown(BLUE_CONTROL_KEY))
                _renderEngine.Ambient.B -= RenderEngine.COLOR_STEP;
            else if (Keyboard.IsKeyDown(DIFFUSE_CONTROL_KEY) && Keyboard.IsKeyDown(RED_CONTROL_KEY))
                _renderEngine.Diffuse.R -= RenderEngine.COLOR_STEP;
            else if (Keyboard.IsKeyDown(DIFFUSE_CONTROL_KEY) && Keyboard.IsKeyDown(GREEN_CONTROL_KEY))
                _renderEngine.Diffuse.G -= RenderEngine.COLOR_STEP;
            else if (Keyboard.IsKeyDown(DIFFUSE_CONTROL_KEY) && Keyboard.IsKeyDown(BLUE_CONTROL_KEY))
                _renderEngine.Diffuse.B -= RenderEngine.COLOR_STEP;
            else if (Keyboard.IsKeyDown(SPECULAR_CONTROL_KEY) && Keyboard.IsKeyDown(RED_CONTROL_KEY))
                _renderEngine.Specular.R -= RenderEngine.COLOR_STEP;
            else if (Keyboard.IsKeyDown(SPECULAR_CONTROL_KEY) && Keyboard.IsKeyDown(GREEN_CONTROL_KEY))
                _renderEngine.Specular.G -= RenderEngine.COLOR_STEP;
            else if (Keyboard.IsKeyDown(SPECULAR_CONTROL_KEY) && Keyboard.IsKeyDown(BLUE_CONTROL_KEY))
                _renderEngine.Specular.B -= RenderEngine.COLOR_STEP;
            else if (Keyboard.IsKeyDown(CONTROL_KEY)) _camera.DecreaseZoomStep();
            else _camera.ZoomIn();
        }
        else
        {
            if (Keyboard.IsKeyDown(X_CONTROL_KEY) && Keyboard.IsKeyDown(MOVE_KEY)) _model.XPosition += Model.MOVE_STEP;
            else if (Keyboard.IsKeyDown(Y_CONTROL_KEY) && Keyboard.IsKeyDown(MOVE_KEY))
                _model.YPosition += Model.MOVE_STEP;
            else if (Keyboard.IsKeyDown(Z_CONTROL_KEY) && Keyboard.IsKeyDown(MOVE_KEY))
                _model.ZPosition += Model.MOVE_STEP;
            else if (Keyboard.IsKeyDown(X_CONTROL_KEY)) _model.XAxisRotate += Model.MOUSE_ROTATION_DELTA;
            else if (Keyboard.IsKeyDown(Y_CONTROL_KEY)) _model.YAxisRotate += Model.MOUSE_ROTATION_DELTA;
            else if (Keyboard.IsKeyDown(Z_CONTROL_KEY)) _model.ZAxisRotate += Model.MOUSE_ROTATION_DELTA;
            else if (Keyboard.IsKeyDown(FOV_CHANGE_KEY)) _camera.FOV += _camera.FovStep;
            else if (Keyboard.IsKeyDown(SCALE_KEY) && Keyboard.IsKeyDown(CONTROL_KEY)) _model.IncreaseScaleStep();
            else if (Keyboard.IsKeyDown(SCALE_KEY)) _model.Scale += Model.SCALE_STEP;
            else if (Keyboard.IsKeyDown(MOVE_STEP_KEY)) _model.IncreaseMoveStep();
            else if (Keyboard.IsKeyDown(NEAR_PLANE_DISTANCE_CHANGE_KEY)) _camera.ZNear += _camera.PlaneDistanceStep;
            else if (Keyboard.IsKeyDown(FAR_PLANE_DISTANCE_CHANGE_KEY)) _camera.ZFar += _camera.PlaneDistanceStep;
            else if (Keyboard.IsKeyDown(PLANE_DISTANCE_STEP_KEY)) _camera.IncreasePlaneDistanceStep();
            else if (Keyboard.IsKeyDown(AMBIENT_CONTROL_KEY) && Keyboard.IsKeyDown(CONTROL_KEY))
                _renderEngine.KAmbient += RenderEngine.K_STEP;
            else if (Keyboard.IsKeyDown(DIFFUSE_CONTROL_KEY) && Keyboard.IsKeyDown(CONTROL_KEY))
                _renderEngine.KDiffuse += RenderEngine.K_STEP;
            else if (Keyboard.IsKeyDown(SPECULAR_CONTROL_KEY) && Keyboard.IsKeyDown(CONTROL_KEY))
                _renderEngine.KSpecular += RenderEngine.K_STEP;
            else if (Keyboard.IsKeyDown(SHININESS_CONTROL_KEY) && Keyboard.IsKeyDown(CONTROL_KEY))
                _renderEngine.KShininess += RenderEngine.K_STEP;
            else if (Keyboard.IsKeyDown(AMBIENT_CONTROL_KEY) && Keyboard.IsKeyDown(RED_CONTROL_KEY))
                _renderEngine.Ambient.R += RenderEngine.COLOR_STEP;
            else if (Keyboard.IsKeyDown(AMBIENT_CONTROL_KEY) && Keyboard.IsKeyDown(GREEN_CONTROL_KEY))
                _renderEngine.Ambient.G += RenderEngine.COLOR_STEP;
            else if (Keyboard.IsKeyDown(AMBIENT_CONTROL_KEY) && Keyboard.IsKeyDown(BLUE_CONTROL_KEY))
                _renderEngine.Ambient.B += RenderEngine.COLOR_STEP;
            else if (Keyboard.IsKeyDown(DIFFUSE_CONTROL_KEY) && Keyboard.IsKeyDown(RED_CONTROL_KEY))
                _renderEngine.Diffuse.R += RenderEngine.COLOR_STEP;
            else if (Keyboard.IsKeyDown(DIFFUSE_CONTROL_KEY) && Keyboard.IsKeyDown(GREEN_CONTROL_KEY))
                _renderEngine.Diffuse.G += RenderEngine.COLOR_STEP;
            else if (Keyboard.IsKeyDown(DIFFUSE_CONTROL_KEY) && Keyboard.IsKeyDown(BLUE_CONTROL_KEY))
                _renderEngine.Diffuse.B += RenderEngine.COLOR_STEP;
            else if (Keyboard.IsKeyDown(SPECULAR_CONTROL_KEY) && Keyboard.IsKeyDown(RED_CONTROL_KEY))
                _renderEngine.Specular.R += RenderEngine.COLOR_STEP;
            else if (Keyboard.IsKeyDown(SPECULAR_CONTROL_KEY) && Keyboard.IsKeyDown(GREEN_CONTROL_KEY))
                _renderEngine.Specular.G += RenderEngine.COLOR_STEP;
            else if (Keyboard.IsKeyDown(SPECULAR_CONTROL_KEY) && Keyboard.IsKeyDown(BLUE_CONTROL_KEY))
                _renderEngine.Specular.B += RenderEngine.COLOR_STEP;
            else if (Keyboard.IsKeyDown(CONTROL_KEY)) _camera.IncreaseZoomStep();
            else _camera.ZoomOut();
        }

        DrawModel(_model, _camera);
    }

    private void InvertColors()
    {
        _renderEngine.Background.Invert();

        Brush textBrush = new SolidColorBrush(_renderEngine.Background.InvertColor);
        tbInfo.Foreground = textBrush;
        tbHelp.Foreground = textBrush;
    }

    private void ChangeRasterisation()
    {
        _renderEngine.NextRasterisation();
    }

    private void ToggleVisibility(TextBlock textBlock)
    {
        textBlock.Visibility = textBlock.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
    }

    private void MainWindow_OnKeyUp(object sender, KeyEventArgs e)
    {
        throw new NotImplementedException();
    }
}