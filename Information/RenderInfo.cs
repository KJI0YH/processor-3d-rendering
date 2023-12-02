using Rendering.Objects;
using System;
using System.ComponentModel;
using System.Numerics;
using System.Text;
using System.Windows.Media;
using Rendering.Actions;
using Rendering.Engine;
using Rendering.Primitives;

namespace Rendering.Information;

public class RenderInfo
{
    public int RenderTime = 0;
    public readonly string HelpInfo;

    public RenderInfo()
    {
        HelpInfo = GetHelp();
    }

    public string GetInformation(RenderEngine renderEngine, Model? model, Camera? camera)
    {
        var cameraPosition = camera.SphericalPosition.ToCartesian();
        StringBuilder builder = new();
        builder
            .AppendLine($"Render time: {RenderTime} ms")
            .AppendLine($"Vertex count: {model?.Positions.Count ?? 0}")
            .AppendLine($"Polygons count: {model?.Polygons.Count ?? 0}")
            .AppendLine($"Camera radius: {camera.SphericalPosition.R:F2}")
            .AppendLine($"Azimuth angle: {RadianToDegree(camera.SphericalPosition.AzimuthAngle):N0}°")
            .AppendLine($"Elevation angle: {RadianToDegree(camera.SphericalPosition.ZenithAngle):N0}°")
            .AppendLine($"Camera position: ({cameraPosition.X:F2}, {cameraPosition.Y:F2}, {cameraPosition.Z:F2})")
            .AppendLine($"Camera target: ({camera.Target.X}, {camera.Target.Y}, {camera.Target.Z})")
            .AppendLine($"Scale: {model?.Scale ?? 0:F5}")
            .AppendLine($"Scale step: {Model.ScaleStep:F5}")
            .AppendLine($"Rotate X: {RadianToDegree(model?.XAxisRotate ?? 0):N0}°")
            .AppendLine($"Rotate Y: {RadianToDegree(model?.YAxisRotate ?? 0):N0}°")
            .AppendLine($"Rotate Z: {RadianToDegree(model?.ZAxisRotate ?? 0):N0}°")
            .AppendLine($"Move: ({model?.XPosition ?? 0:F2}, {model?.YPosition ?? 0:F2}, {model?.ZPosition ?? 0:F2})")
            .AppendLine($"Move step: {Model.MoveStep:F2}")
            .AppendLine($"Zoom step: {camera.ZoomStep:F2}")
            .AppendLine($"FOV: {RadianToDegree(camera.FOV):N0}°")
            .AppendLine($"Near plane distance: {camera.ZNear}")
            .AppendLine($"Far plane distance: {camera.ZFar}")
            .AppendLine($"Plane distance step: {Camera.PlaneDistanceStep}")
            .AppendLine($"Rasterisation: {renderEngine.Rasterisation}")
            .AppendLine($"Drawing mode: {GetDescription(renderEngine.DrawMode)}")
            .AppendLine($"Ambient:\t{renderEngine.KAmbient:N1} | {GetColorValue(renderEngine.Ambient)}")
            .AppendLine($"Diffuse:\t{renderEngine.KDiffuse:N1} | {GetColorValue(renderEngine.Diffuse)}")
            .AppendLine($"Specular:\t{renderEngine.KSpecular:N1} | {GetColorValue(renderEngine.Specular)}")
            .AppendLine($"Shininess:\t{renderEngine.KShininess:N1}")
            .AppendLine($"Screen width: {camera.ScreenWidth}")
            .AppendLine($"Screen height: {camera.ScreenHeight}")
            .AppendLine($"Screen aspect: {camera.ScreenWidth / camera.ScreenHeight:F5}");
        return builder.ToString();
    }

    public string GetHelp()
    {
        StringBuilder builder = new();
        builder
            .AppendLine($"To close the application: {Actions.ActionList.CLOSE_APP_KEY}")
            .AppendLine($"To open a file: {Actions.ActionList.OPEN_MODEL_KEY}")
            .AppendLine($"For model rotation: Mouse Drag or Key Arrows")
            .AppendLine($"To zoom (in|out) of the camera: Mouse Wheel")
            .AppendLine($"To change the zoom step of the camera: {Actions.ActionList.CONTROL_KEY} + MouseWheel")
            .AppendLine($"For rotation around the X axis: {Actions.ActionList.X_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"For rotation around the Y axis: {Actions.ActionList.Y_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"For rotation around the Z axis: {Actions.ActionList.Z_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"For X axis movement: {Actions.ActionList.MOVE_KEY} + {Actions.ActionList.X_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"For Y axis movement: {Actions.ActionList.MOVE_KEY} + {Actions.ActionList.Y_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"For Z axis movement: {Actions.ActionList.MOVE_KEY} + {Actions.ActionList.Z_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"For change move step of the model: {Actions.ActionList.MOVE_STEP_KEY} + Mouse Wheel")
            .AppendLine($"For scaling model: {Actions.ActionList.SCALE_KEY} + Mouse Wheel")
            .AppendLine($"For change scaling step of the model: {Actions.ActionList.CONTROL_KEY} + " +
                        $"{Actions.ActionList.SCALE_KEY} + Mouse Wheel")
            .AppendLine($"To change the FOV: {Actions.ActionList.FOV_CHANGE_KEY} + Mouse Wheel")
            .AppendLine($"To change near plane distance: {Actions.ActionList.NEAR_PLANE_DISTANCE_CHANGE_KEY} + Mouse Wheel")
            .AppendLine($"To change far plane distance: {Actions.ActionList.FAR_PLANE_DISTANCE_CHANGE_KEY} + Mouse Wheel")
            .AppendLine($"To change plane distance step: {Actions.ActionList.PLANE_DISTANCE_STEP_KEY} + Mouse Wheel")
            .AppendLine($"To change Ambient coefficient: {Actions.ActionList.CONTROL_KEY} + " +
                        $"{Actions.ActionList.AMBIENT_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"To change Diffuse coefficient: {Actions.ActionList.CONTROL_KEY} + " +
                        $"{Actions.ActionList.DIFFUSE_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"To change Specular coefficient: {Actions.ActionList.CONTROL_KEY} + " +
                        $"{Actions.ActionList.SPECULAR_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"To change Shininess coefficient: {Actions.ActionList.CONTROL_KEY} + " +
                        $"{Actions.ActionList.SHININESS_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"To change Ambient color components: {Actions.ActionList.AMBIENT_CONTROL_KEY} + " +
                        $"({Actions.ActionList.RED_CONTROL_KEY} | {Actions.ActionList.GREEN_CONTROL_KEY} | {Actions.ActionList.BLUE_CONTROL_KEY})" +
                        $" + Mouse Wheel")
            .AppendLine($"To change Diffuse color components: {Actions.ActionList.DIFFUSE_CONTROL_KEY} + " +
                        $"({Actions.ActionList.RED_CONTROL_KEY} | {Actions.ActionList.GREEN_CONTROL_KEY} | {Actions.ActionList.BLUE_CONTROL_KEY})" +
                        $" + Mouse Wheel")
            .AppendLine($"To change Specular color components: {Actions.ActionList.SPECULAR_CONTROL_KEY} + " +
                        $"({Actions.ActionList.RED_CONTROL_KEY} | {Actions.ActionList.GREEN_CONTROL_KEY} | {Actions.ActionList.BLUE_CONTROL_KEY})" +
                        $" + Mouse Wheel")
            .AppendLine($"To invert colors: {Actions.ActionList.INVERT_COLORS_KEY}")
            .AppendLine($"To set the camera to the initial position: {Actions.ActionList.CAMERA_RESET_KEY}")
            .AppendLine($"To change the rasterisation algorithm: {Actions.ActionList.RASTERISATION_CHANGE_KEY}")
            .AppendLine($"Vertex only drawing mode: {Actions.ActionList.VERTEX_ONLY_DRAW_MODE_KEY}")
            .AppendLine($"Wire drawing mode: {Actions.ActionList.WIRE_DRAW_MODE_KEY}")
            .AppendLine($"Rasterisation drawing mode: {Actions.ActionList.RASTERISATION_DRAW_MODE_KEY}")
            .AppendLine($"Phong shading drawing mode: {Actions.ActionList.PHONG_SHADING_DRAW_MODE_KEY}")
            .AppendLine($"Phong lighting drawing mode: {Actions.ActionList.PHONG_LIGHTING_DRAW_MODE_KEY}")
            .AppendLine($"To toggle the render information: {Actions.ActionList.INFORMATION_TOGGLE_KEY}")
            .AppendLine($"To toggle the help : {Actions.ActionList.HELP_TOGGLE_KEY}");
        return builder.ToString();
    }

    public string GetParseError(string message)
    {
        StringBuilder builder = new();
        builder
            .AppendLine($"{message}");
        return builder.ToString();
    }

    private float RadianToDegree(float radian)
    {
        var degree = radian * 180 / MathF.PI % 360;
        if (degree < 0) degree += 360;
        return degree;
    }

    private static string GetDescription(Enum value)
    {
        var fi = value.GetType().GetField(value.ToString());
        var attributes = (DescriptionAttribute[])fi?.GetCustomAttributes(typeof(DescriptionAttribute), false)!;
        return attributes.Length > 0 ? attributes[0].Description : value.ToString();
    }

    private static string GetColorValue(ColorComponent component)
    {
        var color = component.Color;
        return $"({component.R:F2}, {component.G:F2}, {component.B:F2}) | " +
               $"{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}