using System;
using System.Collections.Generic;
using System.Windows.Input;
using Rendering.Engine;
using Rendering.Objects;

namespace Rendering.Actions;

public class ActionList
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

    public Dictionary<HashSet<Key>, Action<int, RenderEngine, Model, Camera>> MouseWheelActions { get; } = new()
    {
        { new HashSet<Key> { X_CONTROL_KEY, MOVE_KEY }, MoveByX },
        { new HashSet<Key> { Y_CONTROL_KEY, MOVE_KEY }, MoveByY },
        { new HashSet<Key> { Z_CONTROL_KEY, MOVE_KEY }, MoveByZ },
        { new HashSet<Key> { X_CONTROL_KEY }, RotateByX },
        { new HashSet<Key> { Y_CONTROL_KEY }, RotateByY },
        { new HashSet<Key> { Z_CONTROL_KEY }, RotateByZ },
        { new HashSet<Key> { FOV_CHANGE_KEY }, ChangeFov },
        { new HashSet<Key> { SCALE_KEY, CONTROL_KEY }, ChangeScaleStep },
        { new HashSet<Key> { SCALE_KEY }, ChangeScale },
        { new HashSet<Key> { MOVE_STEP_KEY }, ChangeMoveStep },
        { new HashSet<Key> { NEAR_PLANE_DISTANCE_CHANGE_KEY }, ChangeNearPlaneDistance },
        { new HashSet<Key> { FAR_PLANE_DISTANCE_CHANGE_KEY }, ChangeFarPlaneDistance },
        { new HashSet<Key> { PLANE_DISTANCE_STEP_KEY }, ChangePlaneDistanceStep },
        { new HashSet<Key> { AMBIENT_CONTROL_KEY, CONTROL_KEY }, ChangeAmbientCoefficient },
        { new HashSet<Key> { DIFFUSE_CONTROL_KEY, CONTROL_KEY }, ChangeDiffuseCoefficient },
        { new HashSet<Key> { SPECULAR_CONTROL_KEY, CONTROL_KEY }, ChangeSpecularCoefficient },
        { new HashSet<Key> { SHININESS_CONTROL_KEY, CONTROL_KEY }, ChangeShininessCoefficient },
        { new HashSet<Key> { AMBIENT_CONTROL_KEY, RED_CONTROL_KEY }, ChangeAmbientRed },
        { new HashSet<Key> { AMBIENT_CONTROL_KEY, GREEN_CONTROL_KEY }, ChangeAmbientGreen },
        { new HashSet<Key> { AMBIENT_CONTROL_KEY, BLUE_CONTROL_KEY }, ChangeAmbientBlue },
        { new HashSet<Key> { DIFFUSE_CONTROL_KEY, RED_CONTROL_KEY }, ChangeDiffuseRed },
        { new HashSet<Key> { DIFFUSE_CONTROL_KEY, GREEN_CONTROL_KEY }, ChangeDiffuseGreen },
        { new HashSet<Key> { DIFFUSE_CONTROL_KEY, BLUE_CONTROL_KEY }, ChangeDiffuseBlue },
        { new HashSet<Key> { SPECULAR_CONTROL_KEY, RED_CONTROL_KEY }, ChangeSpecularRed },
        { new HashSet<Key> { SPECULAR_CONTROL_KEY, GREEN_CONTROL_KEY }, ChangeSpecularGreen },
        { new HashSet<Key> { SPECULAR_CONTROL_KEY, BLUE_CONTROL_KEY }, ChangeSpecularBlue },
        { new HashSet<Key> { CONTROL_KEY }, ChangeZoomStep },
        { new HashSet<Key>(), Zoom }
    };

    private static void MoveByX(int delta, RenderEngine engine, Model model, Camera camera)
    {
        model.XPosition += delta * Model.MoveStep;
    }

    private static void MoveByY(int delta, RenderEngine engine, Model model, Camera camera)
    {
        model.YPosition += delta * Model.MoveStep;
    }

    private static void MoveByZ(int delta, RenderEngine engine, Model model, Camera camera)
    {
        model.ZPosition += delta * Model.MoveStep;
    }

    private static void RotateByX(int delta, RenderEngine engine, Model model, Camera camera)
    {
        model.XAxisRotate += delta * Model.MOUSE_ROTATION_DELTA;
    }

    private static void RotateByY(int delta, RenderEngine engine, Model model, Camera camera)
    {
        model.YAxisRotate += delta * Model.MOUSE_ROTATION_DELTA;
    }

    private static void RotateByZ(int delta, RenderEngine engine, Model model, Camera camera)
    {
        model.ZAxisRotate += delta * Model.MOUSE_ROTATION_DELTA;
    }

    private static void ChangeFov(int delta, RenderEngine engine, Model model, Camera camera)
    {
        camera.FOV += delta * Camera.FOV_STEP;
    }

    private static void ChangeScaleStep(int delta, RenderEngine engine, Model model, Camera camera)
    {
        if (delta < 0) model.DecreaseScaleStep();
        else model.IncreaseScaleStep();
    }

    private static void ChangeScale(int delta, RenderEngine engine, Model model, Camera camera)
    {
        model.Scale += delta * Model.ScaleStep;
    }

    private static void ChangeMoveStep(int delta, RenderEngine engine, Model model, Camera camera)
    {
        if (delta < 0) model.DecreaseMoveStep();
        else model.IncreaseMoveStep();
    }

    private static void ChangeNearPlaneDistance(int delta, RenderEngine engine, Model model, Camera camera)
    {
        camera.ZNear += delta * Camera.PlaneDistanceStep;
    }

    private static void ChangeFarPlaneDistance(int delta, RenderEngine engine, Model model, Camera camera)
    {
        camera.ZFar += delta * Camera.PlaneDistanceStep;
    }

    private static void ChangePlaneDistanceStep(int delta, RenderEngine engine, Model model, Camera camera)
    {
        if (delta < 0) camera.DecreasePlaneDistanceStep();
        else camera.IncreasePlaneDistanceStep();
    }

    private static void ChangeAmbientCoefficient(int delta, RenderEngine engine, Model model, Camera camera)
    {
        engine.KAmbient += delta * RenderEngine.K_STEP;
    }

    private static void ChangeDiffuseCoefficient(int delta, RenderEngine engine, Model model, Camera camera)
    {
        engine.KDiffuse += delta * RenderEngine.K_STEP;
    }

    private static void ChangeSpecularCoefficient(int delta, RenderEngine engine, Model model, Camera camera)
    {
        engine.KSpecular += delta * RenderEngine.K_STEP;
    }

    private static void ChangeShininessCoefficient(int delta, RenderEngine engine, Model model, Camera camera)
    {
        engine.KShininess += delta * RenderEngine.K_STEP;
    }

    private static void ChangeAmbientRed(int delta, RenderEngine engine, Model model, Camera camera)
    {
        engine.Ambient.R += delta * RenderEngine.COLOR_STEP;
    }

    private static void ChangeAmbientGreen(int delta, RenderEngine engine, Model model, Camera camera)
    {
        engine.Ambient.G += delta * RenderEngine.COLOR_STEP;
    }

    private static void ChangeAmbientBlue(int delta, RenderEngine engine, Model model, Camera camera)
    {
        engine.Ambient.B += delta * RenderEngine.COLOR_STEP;
    }

    private static void ChangeDiffuseRed(int delta, RenderEngine engine, Model model, Camera camera)
    {
        engine.Diffuse.R += delta * RenderEngine.COLOR_STEP;
    }

    private static void ChangeDiffuseGreen(int delta, RenderEngine engine, Model model, Camera camera)
    {
        engine.Diffuse.G += delta * RenderEngine.COLOR_STEP;
    }

    private static void ChangeDiffuseBlue(int delta, RenderEngine engine, Model model, Camera camera)
    {
        engine.Diffuse.B += delta * RenderEngine.COLOR_STEP;
    }

    private static void ChangeSpecularRed(int delta, RenderEngine engine, Model model, Camera camera)
    {
        engine.Specular.R += delta * RenderEngine.COLOR_STEP;
    }

    private static void ChangeSpecularGreen(int delta, RenderEngine engine, Model model, Camera camera)
    {
        engine.Specular.G += delta * RenderEngine.COLOR_STEP;
    }

    private static void ChangeSpecularBlue(int delta, RenderEngine engine, Model model, Camera camera)
    {
        engine.Specular.B += delta * RenderEngine.COLOR_STEP;
    }

    private static void ChangeZoomStep(int delta, RenderEngine engine, Model model, Camera camera)
    {
        if (delta < 0) camera.DecreaseZoomStep();
        else camera.IncreaseZoomStep();
    }

    private static void Zoom(int delta, RenderEngine engine, Model model, Camera camera)
    {
        if (delta < 0) camera.ZoomIn();
        else camera.ZoomOut();
    }
}