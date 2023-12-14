using Microsoft.Win32;
using Rendering.Engine;
using Rendering.Exceptions;
using Rendering.Information;
using Rendering.Objects;
using Rendering.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Rendering.Actions;

namespace Rendering;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ActionList _actions = new();
    private readonly OpenFileDialog _openObjFileDialog;
    private readonly OpenFileDialog _openTextureFileDialog;
    private readonly ObjParser _modelParser = new();
    private readonly ImageParser _imageParser = new();
    private Model? _model;
    private readonly Camera _camera = new();
    private readonly RenderEngine _renderEngine = new();
    private readonly RenderInfo _renderInfo = new();
    private Point _mouseClickPosition;
    private readonly HashSet<Key> _pressedKeys = new();

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
        _pressedKeys.Add(e.Key);
        switch (e.Key)
        {
            case ActionList.OPEN_MODEL_KEY:
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
                    tbError.Text = RenderInfo.GetParseError(_modelParser.GetErrors());
                }

                _pressedKeys.Clear();

                break;
            case ActionList.OPEN_TEXTURE_KEY:
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
                        tbError.Text = RenderInfo.GetParseError(exception.Message);
                    }
                }

                _pressedKeys.Clear();

                break;
            case ActionList.VERTEX_ONLY_DRAW_MODE_KEY:
                _renderEngine.DrawMode = DrawMode.VertexOnly;
                break;
            case ActionList.WIRE_DRAW_MODE_KEY:
                _renderEngine.DrawMode = DrawMode.Wire;
                break;
            case ActionList.RASTERISATION_DRAW_MODE_KEY:
                _renderEngine.DrawMode = DrawMode.Lambert;
                break;
            case ActionList.PHONG_SHADING_DRAW_MODE_KEY:
                _renderEngine.DrawMode = DrawMode.PhongShading;
                break;
            case ActionList.PHONG_LIGHTING_DRAW_MODE_KEY:
                _renderEngine.DrawMode = DrawMode.PhongLighting;
                break;
            case ActionList.TEXTURE_DRAW_MODE_KEY:
                _renderEngine.DrawMode = DrawMode.Texture;
                break;
            case ActionList.CUSTOM_TEXTURE_DRAW_MODE_KEY:
                _renderEngine.DrawMode = DrawMode.Custom;
                break;
            case ActionList.PBR_DRAW_MODE_KEY:
                _renderEngine.DrawMode = DrawMode.PBR;
                break;
            case ActionList.INVERT_COLORS_KEY:
                InvertColors();
                break;
            case ActionList.RASTERISATION_CHANGE_KEY:
                ChangeRasterisation();
                break;
            case ActionList.CAMERA_RESET_KEY:
                if (_model != null)
                {
                    _model.SetInitialPosition();
                    _camera.SetInitialPosition(_model);
                }

                break;
            case ActionList.INFORMATION_TOGGLE_KEY:
                ToggleVisibility(tbInfo);
                break;
            case ActionList.HELP_TOGGLE_KEY:
                ToggleVisibility(tbHelp);
                break;
            case ActionList.ERRORS_TOGGLE_KEY:
                ToggleVisibility(tbError);
                break;
            case ActionList.MOVE_UP_KEY:
                _camera.ZenithAngle -= _camera.KeyRotation;
                break;
            case ActionList.MOVE_RIGHT_KEY:
                _camera.AzimuthAngle += _camera.KeyRotation;
                break;
            case ActionList.MOVE_DOWN_KEY:
                _camera.ZenithAngle += _camera.KeyRotation;
                break;
            case ActionList.MOVE_LEFT_KEY:
                _camera.AzimuthAngle -= _camera.KeyRotation;
                break;
            case ActionList.CLOSE_APP_KEY:
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

        var currentCombination = _actions.MouseWheelActions.Keys
            .First(combination => combination.All(_pressedKeys.Contains));

        if (_actions.MouseWheelActions.TryGetValue(currentCombination, out var action))
            action(delta, _renderEngine, _model, _camera);

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
}