using Rendering.Objects;
using System;
using System.ComponentModel;
using System.Numerics;
using System.Text;
using System.Windows.Media;
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
            .AppendLine($"Scale step: {Model.SCALE_STEP:F5}")
            .AppendLine($"Rotate X: {RadianToDegree(model?.XAxisRotate ?? 0):N0}°")
            .AppendLine($"Rotate Y: {RadianToDegree(model?.YAxisRotate ?? 0):N0}°")
            .AppendLine($"Rotate Z: {RadianToDegree(model?.ZAxisRotate ?? 0):N0}°")
            .AppendLine($"Move: ({model?.XPosition ?? 0:F2}, {model?.YPosition ?? 0:F2}, {model?.ZPosition ?? 0:F2})")
            .AppendLine($"Move step: {Model.MOVE_STEP:F2}")
            .AppendLine($"Zoom step: {camera.ZoomStep:F2}")
            .AppendLine($"FOV: {RadianToDegree(camera.FOV):N0}°")
            .AppendLine($"Near plane distance: {camera.ZNear}")
            .AppendLine($"Far plane distance: {camera.ZFar}")
            .AppendLine($"Plane distance step: {camera.PlaneDistanceStep}")
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
            .AppendLine($"To close the application: {MainWindow.CLOSE_APP_KEY}")
            .AppendLine($"To open a file: {MainWindow.OPEN_MODEL_KEY}")
            .AppendLine($"For model rotation: Mouse Drag or Key Arrows")
            .AppendLine($"To zoom (in|out) of the camera: Mouse Wheel")
            .AppendLine($"To change the zoom step of the camera: {MainWindow.CONTROL_KEY} + MouseWheel")
            .AppendLine($"For rotation around the X axis: {MainWindow.X_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"For rotation around the Y axis: {MainWindow.Y_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"For rotation around the Z axis: {MainWindow.Z_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"For X axis movement: {MainWindow.MOVE_KEY} + {MainWindow.X_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"For Y axis movement: {MainWindow.MOVE_KEY} + {MainWindow.Y_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"For Z axis movement: {MainWindow.MOVE_KEY} + {MainWindow.Z_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"For change move step of the model: {MainWindow.MOVE_STEP_KEY} + Mouse Wheel")
            .AppendLine($"For scaling model: {MainWindow.SCALE_KEY} + Mouse Wheel")
            .AppendLine($"For change scaling step of the model: {MainWindow.CONTROL_KEY} + " +
                        $"{MainWindow.SCALE_KEY} + Mouse Wheel")
            .AppendLine($"To change the FOV: {MainWindow.FOV_CHANGE_KEY} + Mouse Wheel")
            .AppendLine($"To change near plane distance: {MainWindow.NEAR_PLANE_DISTANCE_CHANGE_KEY} + Mouse Wheel")
            .AppendLine($"To change far plane distance: {MainWindow.FAR_PLANE_DISTANCE_CHANGE_KEY} + Mouse Wheel")
            .AppendLine($"To change plane distance step: {MainWindow.PLANE_DISTANCE_STEP_KEY} + Mouse Wheel")
            .AppendLine($"To change Ambient coefficient: {MainWindow.CONTROL_KEY} + " +
                        $"{MainWindow.AMBIENT_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"To change Diffuse coefficient: {MainWindow.CONTROL_KEY} + " +
                        $"{MainWindow.DIFFUSE_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"To change Specular coefficient: {MainWindow.CONTROL_KEY} + " +
                        $"{MainWindow.SPECULAR_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"To change Shininess coefficient: {MainWindow.CONTROL_KEY} + " +
                        $"{MainWindow.SHININESS_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"To change Ambient color components: {MainWindow.AMBIENT_CONTROL_KEY} + " +
                        $"({MainWindow.RED_CONTROL_KEY} | {MainWindow.GREEN_CONTROL_KEY} | {MainWindow.BLUE_CONTROL_KEY})" +
                        $" + Mouse Wheel")
            .AppendLine($"To change Diffuse color components: {MainWindow.DIFFUSE_CONTROL_KEY} + " +
                        $"({MainWindow.RED_CONTROL_KEY} | {MainWindow.GREEN_CONTROL_KEY} | {MainWindow.BLUE_CONTROL_KEY})" +
                        $" + Mouse Wheel")
            .AppendLine($"To change Specular color components: {MainWindow.SPECULAR_CONTROL_KEY} + " +
                        $"({MainWindow.RED_CONTROL_KEY} | {MainWindow.GREEN_CONTROL_KEY} | {MainWindow.BLUE_CONTROL_KEY})" +
                        $" + Mouse Wheel")
            .AppendLine($"To invert colors: {MainWindow.INVERT_COLORS_KEY}")
            .AppendLine($"To set the camera to the initial position: {MainWindow.CAMERA_RESET_KEY}")
            .AppendLine($"To change the rasterisation algorithm: {MainWindow.RASTERISATION_CHANGE_KEY}")
            .AppendLine($"Vertex only drawing mode: {MainWindow.VERTEX_ONLY_DRAW_MODE_KEY}")
            .AppendLine($"Wire drawing mode: {MainWindow.WIRE_DRAW_MODE_KEY}")
            .AppendLine($"Rasterisation drawing mode: {MainWindow.RASTERISATION_DRAW_MODE_KEY}")
            .AppendLine($"Phong shading drawing mode: {MainWindow.PHONG_SHADING_DRAW_MODE_KEY}")
            .AppendLine($"Phong lighting drawing mode: {MainWindow.PHONG_LIGHTING_DRAW_MODE_KEY}")
            .AppendLine($"To toggle the render information: {MainWindow.INFORMATION_TOGGLE_KEY}")
            .AppendLine($"To toggle the help : {MainWindow.HELP_TOGGLE_KEY}");
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