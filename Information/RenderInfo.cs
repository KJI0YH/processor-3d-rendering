using Rendering.Objects;
using Rendering.Rasterisation;
using System;
using System.Numerics;
using System.Text;

namespace Rendering.Information
{
    public class RenderInfo
    {
        public int RenderTime = 0;

        public RenderInfo() { }

        public string GetInfomation(Model model, Camera camera, IRasterisation rasterisation)
        {
            Vector3 cameraPosition = camera.SphericalPosition.ToCartesian();
            StringBuilder builder = new();
            builder
                .AppendLine($"Render time: {RenderTime} ms")
                .AppendLine($"Vertex count: {model.Vertices.Count}")
                .AppendLine($"Polygons count: {model.Polygons.Count}")
                .AppendLine($"Scale: {model.Scale:F5}")
                .AppendLine($"Scale step: {model.ScaleStep:F5}")
                .AppendLine($"Rotate X: {RadianToDegree(model.XAxisRotate):N0}°")
                .AppendLine($"Rotate Y: {RadianToDegree(model.YAxisRotate):N0}°")
                .AppendLine($"Rotate Z: {RadianToDegree(model.ZAxisRotate):N0}°")
                .AppendLine($"Move: ({model.XPosition:F2}, {model.YPosition:F2}, {model.ZPosition:F2})")
                .AppendLine($"Move step: {model.MoveStep:F2}")
                .AppendLine($"R: {camera.SphericalPosition.R:F2}")
                .AppendLine($"Zoom step: {camera.ZoomStep:F2}")
                .AppendLine($"Azimuth angle: {RadianToDegree(camera.SphericalPosition.AzimuthAngle):N0}°")
                .AppendLine($"Elevation angle: {RadianToDegree(camera.SphericalPosition.ElevationAngle):N0}°")
                .AppendLine($"Camera position: ({cameraPosition.X:F2}, {cameraPosition.Y:F2}, {cameraPosition.Z:F2})")
                .AppendLine($"Camera target: ({camera.Target.X}, {camera.Target.Y}, {camera.Target.Z})")
                .AppendLine($"FOV: {RadianToDegree(camera.FOV):N0}°")
                .AppendLine($"Near plane distance: {camera.ZNear}")
                .AppendLine($"Far plane distance: {camera.ZFar}")
                .AppendLine($"Plane distance step: {camera.PlaneDistanceStep}")
                .AppendLine($"Rasterisation: {rasterisation.GetType().Name}")
                .AppendLine($"Screen width: {camera.ScreenWidth}")
                .AppendLine($"Screen height: {camera.ScreenHeight}")
                .AppendLine($"Screen aspect: {camera.ScreenWidth / camera.ScreenHeight:F5}");
            return builder.ToString();
        }

        public string GetHelp()
        {
            StringBuilder builder = new();
            builder
                .AppendLine("To close the application: Escape")
                .AppendLine("To open a file: O")
                .AppendLine("For model rotation: Mouse Drag")
                .AppendLine("To zoom in|out of the camera: Mouse Wheel")
                .AppendLine("To change the zoom step of the camera: Left Ctrl + MouseWheel")
                .AppendLine("For rotation around the X axis: X + Mouse Wheel")
                .AppendLine("For rotation around the Y axis: Y + Mouse Wheel")
                .AppendLine("For rotation around the Z axis: Z + Mouse Wheel")
                .AppendLine("For X axis movement: X + Left Shift + Mouse Wheel")
                .AppendLine("For Y axis movement: Y + Left Shift + Mouse Wheel")
                .AppendLine("For Z axis movement: Z + Left Shift + Mouse Wheel")
                .AppendLine("For change move step of the model: M + Mouse Wheel")
                .AppendLine("For scaling model: S + Mouse Wheel")
                .AppendLine("For change scaling step of the model: S + Left Ctrl + Mouse Wheel")
                .AppendLine("To change the FOV: F + Mouse Wheel")
                .AppendLine("To change near plane distance: N + Mouse Wheel")
                .AppendLine("To change far plane distance: B + Mouse Wheel")
                .AppendLine("To change plane distance step: P + Mouse Wheel")
                .AppendLine("To invert colors: C")
                .AppendLine("To set the camera to the initial position: Home")
                .AppendLine("To change the rasterisation algorithm: R")
                .AppendLine("Vertex only drawing mode: 0")
                .AppendLine("Wire drawing mode: 1")
                .AppendLine("Rasterisation drawing mode: 2")
                .AppendLine("To toggle the render information: I")
                .AppendLine("To toggle the help : F1");
            return builder.ToString();
        }

        public string GetParseError(string filename, string message)
        {
            StringBuilder builder = new();
            builder
                .AppendLine($"File {filename} cannot be parsed")
                .AppendLine($"{message}");
            return builder.ToString();
        }

        private float RadianToDegree(float radian)
        {
            float degree = (radian * 180 / MathF.PI) % 360;
            if (degree < 0) degree += 360;
            return degree;
        }
    }
}
