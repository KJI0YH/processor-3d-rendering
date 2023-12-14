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
    public readonly string HelpInfo = GetHelp();

    public string GetInformation(RenderEngine renderEngine, Model? model, Camera? camera)
    {
        var cameraPosition = camera?.SphericalPosition.ToCartesian() ?? Vector3.Zero;
        StringBuilder builder = new();
        builder
            .AppendLine($"Render time: {RenderTime} ms")
            .AppendLine($"Vertex count: {model?.Positions.Count ?? 0}")
            .AppendLine($"Polygons count: {model?.Polygons.Count ?? 0}")
            .AppendLine($"Camera radius: {camera?.SphericalPosition.R ?? 0:F2}")
            .AppendLine($"Azimuth angle: {RadianToDegree(camera?.SphericalPosition.AzimuthAngle ?? 0):N0}°")
            .AppendLine($"Elevation angle: {RadianToDegree(camera?.SphericalPosition.ZenithAngle ?? 0):N0}°")
            .AppendLine($"Camera position: ({cameraPosition.X:F2}, {cameraPosition.Y:F2}, {cameraPosition.Z:F2})")
            .AppendLine($"Camera target: ({camera?.Target.X ?? 0}, {camera?.Target.Y ?? 0}, {camera?.Target.Z ?? 0})")
            .AppendLine($"Scale: {model?.Scale ?? 0:F5}")
            .AppendLine($"Scale step: {Model.ScaleStep:F5}")
            .AppendLine($"Rotate X: {RadianToDegree(model?.XAxisRotate ?? 0):N0}°")
            .AppendLine($"Rotate Y: {RadianToDegree(model?.YAxisRotate ?? 0):N0}°")
            .AppendLine($"Rotate Z: {RadianToDegree(model?.ZAxisRotate ?? 0):N0}°")
            .AppendLine($"Move: ({model?.XPosition ?? 0:F2}, {model?.YPosition ?? 0:F2}, {model?.ZPosition ?? 0:F2})")
            .AppendLine($"Move step: {Model.MoveStep:F2}")
            .AppendLine($"Zoom step: {camera?.ZoomStep ?? 0:F2}")
            .AppendLine($"FOV: {RadianToDegree(camera?.FOV ?? 0):N0}°")
            .AppendLine($"Near plane distance: {camera?.ZNear ?? 0}")
            .AppendLine($"Far plane distance: {camera?.ZFar ?? 0}")
            .AppendLine($"Plane distance step: {Camera.PlaneDistanceStep}")
            .AppendLine($"Rasterisation: {renderEngine.Rasterisation}")
            .AppendLine($"Drawing mode: {GetDescription(renderEngine.DrawMode)}")
            .AppendLine($"Ambient:\t{renderEngine.KAmbient:N1} | {GetColorValue(renderEngine.Ambient)}")
            .AppendLine($"Diffuse:\t{renderEngine.KDiffuse:N1} | {GetColorValue(renderEngine.Diffuse)}")
            .AppendLine($"Specular:\t{renderEngine.KSpecular:N1} | {GetColorValue(renderEngine.Specular)}")
            .AppendLine($"Shininess:\t{renderEngine.KShininess:N1}")
            .AppendLine($"Screen width: {camera?.ScreenWidth ?? 0}")
            .AppendLine($"Screen height: {camera?.ScreenHeight ?? 0}")
            .AppendLine($"Screen aspect: {camera?.ScreenWidth / camera?.ScreenHeight ?? 0:F5}");
        return builder.ToString();
    }

    private static string GetHelp()
    {
        StringBuilder builder = new();
        builder
            .AppendLine($"To close the application: {ActionList.CLOSE_APP_KEY}")
            .AppendLine($"To open a model file: {ActionList.OPEN_MODEL_KEY}")
            .AppendLine($"To open a texture file: {ActionList.OPEN_TEXTURE_KEY}")
            .AppendLine($"For model rotation: Mouse Drag or Key Arrows")
            .AppendLine($"To zoom (in|out) of the camera: Mouse Wheel")
            .AppendLine($"To change the zoom step of the camera: {ActionList.CONTROL_KEY} + MouseWheel")
            .AppendLine($"For rotation around the X axis: {ActionList.X_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"For rotation around the Y axis: {ActionList.Y_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"For rotation around the Z axis: {ActionList.Z_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"For X axis movement: {ActionList.MOVE_KEY} + {ActionList.X_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"For Y axis movement: {ActionList.MOVE_KEY} + {ActionList.Y_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"For Z axis movement: {ActionList.MOVE_KEY} + {ActionList.Z_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"For change move step of the model: {ActionList.MOVE_STEP_KEY} + Mouse Wheel")
            .AppendLine($"For scaling model: {ActionList.SCALE_KEY} + Mouse Wheel")
            .AppendLine($"For change scaling step of the model: {ActionList.CONTROL_KEY} + " +
                        $"{ActionList.SCALE_KEY} + Mouse Wheel")
            .AppendLine($"To change the FOV: {ActionList.FOV_CHANGE_KEY} + Mouse Wheel")
            .AppendLine($"To change near plane distance: {ActionList.NEAR_PLANE_DISTANCE_CHANGE_KEY} + Mouse Wheel")
            .AppendLine($"To change far plane distance: {ActionList.FAR_PLANE_DISTANCE_CHANGE_KEY} + Mouse Wheel")
            .AppendLine($"To change plane distance step: {ActionList.PLANE_DISTANCE_STEP_KEY} + Mouse Wheel")
            .AppendLine($"To change Ambient coefficient: {ActionList.CONTROL_KEY} + " +
                        $"{ActionList.AMBIENT_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"To change Diffuse coefficient: {ActionList.CONTROL_KEY} + " +
                        $"{ActionList.DIFFUSE_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"To change Specular coefficient: {ActionList.CONTROL_KEY} + " +
                        $"{ActionList.SPECULAR_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"To change Shininess coefficient: {ActionList.CONTROL_KEY} + " +
                        $"{ActionList.SHININESS_CONTROL_KEY} + Mouse Wheel")
            .AppendLine($"To change Ambient color components: {ActionList.AMBIENT_CONTROL_KEY} + " +
                        $"({ActionList.RED_CONTROL_KEY} | {ActionList.GREEN_CONTROL_KEY} | {ActionList.BLUE_CONTROL_KEY})" +
                        $" + Mouse Wheel")
            .AppendLine($"To change Diffuse color components: {ActionList.DIFFUSE_CONTROL_KEY} + " +
                        $"({ActionList.RED_CONTROL_KEY} | {ActionList.GREEN_CONTROL_KEY} | {ActionList.BLUE_CONTROL_KEY})" +
                        $" + Mouse Wheel")
            .AppendLine($"To change Specular color components: {ActionList.SPECULAR_CONTROL_KEY} + " +
                        $"({ActionList.RED_CONTROL_KEY} | {ActionList.GREEN_CONTROL_KEY} | {ActionList.BLUE_CONTROL_KEY})" +
                        $" + Mouse Wheel")
            .AppendLine($"To invert colors: {ActionList.INVERT_COLORS_KEY}")
            .AppendLine($"To set the camera to the initial position: {ActionList.CAMERA_RESET_KEY}")
            .AppendLine($"To change the rasterisation algorithm: {ActionList.RASTERISATION_CHANGE_KEY}")
            .AppendLine($"Vertex only drawing mode: {ActionList.VERTEX_ONLY_DRAW_MODE_KEY}")
            .AppendLine($"Wire drawing mode: {ActionList.WIRE_DRAW_MODE_KEY}")
            .AppendLine($"Rasterisation drawing mode: {ActionList.RASTERISATION_DRAW_MODE_KEY}")
            .AppendLine($"Phong shading drawing mode: {ActionList.PHONG_SHADING_DRAW_MODE_KEY}")
            .AppendLine($"Phong lighting drawing mode: {ActionList.PHONG_LIGHTING_DRAW_MODE_KEY}")
            .AppendLine($"Texture drawing mode: {ActionList.TEXTURE_DRAW_MODE_KEY}")
            .AppendLine($"Custom texture drawing mode: {ActionList.CUSTOM_TEXTURE_DRAW_MODE_KEY}")
            .AppendLine($"PBR drawing mode: {ActionList.PBR_DRAW_MODE_KEY}")
            .AppendLine($"To toggle the errors information: {ActionList.ERRORS_TOGGLE_KEY}")
            .AppendLine($"To toggle the render information: {ActionList.INFORMATION_TOGGLE_KEY}")
            .AppendLine($"To toggle the help : {ActionList.HELP_TOGGLE_KEY}");
        return builder.ToString();
    }

    public static string GetParseError(string message)
    {
        StringBuilder builder = new();
        builder
            .AppendLine($"{message}");
        return builder.ToString();
    }

    private static float RadianToDegree(float radian)
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