using Lab1.Objects;
using Lab1.Primitives;
using Lab1.Rasterization;
using System;
using System.Text;

namespace Lab1.Information
{
    public class RenderInfo
    {
        public int RenderTime = 0;

        public RenderInfo() { }

        public string GetInfomation(Model model, Camera camera, IRasterization rasterization)
        {
            Vector3 cameraPosition = camera.SphericalPosition.ToCartesian();
            StringBuilder builder = new StringBuilder();
            builder
                .AppendLine($"Render time: {RenderTime} ms")
                .AppendLine($"Vertex count: {model.Vertices.Count}")
                .AppendLine($"Polygons count: {model.Polygons.Count}")
                .AppendLine($"Scale: {model.Scale:F5}")
                .AppendLine($"Scale step: {model.ScaleStep:F5}")
                .AppendLine($"Rotate X: {RadianToDegree(model.XAxisRotate):N0}°")
                .AppendLine($"Rotate Y: {RadianToDegree(model.YAxisRotate):N0}°")
                .AppendLine($"Rotate Z: {RadianToDegree(model.ZAxisRotate):N0}°")
                .AppendLine($"Move: ({model.Translation.X:F2}, {model.Translation.Y:F2}, {model.Translation.Z:F2})")
                .AppendLine($"Move step: {model.MoveStep:F2}")
                .AppendLine($"R: {camera.SphericalPosition.R:F2}")
                .AppendLine($"Zoom step: {camera.ZoomStep:F2}")
                .AppendLine($"Azimuth angle: {RadianToDegree(camera.SphericalPosition.AzimuthAngle):N0}°")
                .AppendLine($"Elevation angle: {RadianToDegree(camera.SphericalPosition.ElevationAngle):N0}°")
                .AppendLine($"Camera position: ({cameraPosition.X:F2}, {cameraPosition.Y:F2}, {cameraPosition.Z:F2})")
                .AppendLine($"Camera target: ({camera.Target.X}, {camera.Target.Y}, {camera.Target.Z})")
                .AppendLine($"FOV: {RadianToDegree(camera.FOV):N0}°")
                .AppendLine($"Rasterization: {rasterization.GetType().Name}")
                .AppendLine($"Screen width: {camera.ScreenWidth}")
                .AppendLine($"Screen height: {camera.ScreenHeight}")
                .AppendLine($"Screen aspect: {camera.ScreenWidth / camera.ScreenHeight:F5}");
            return builder.ToString();
        }

        public string GetHelp()
        {
            StringBuilder builder = new StringBuilder();
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
                .AppendLine("To invert colors: C")
                .AppendLine("To toggle line drawing: L")
                .AppendLine("To change the rasterization algorithm: R")
                .AppendLine("To toggle the render information: I")
                .AppendLine("To toggle the help : F1");
            return builder.ToString();
        }

        public string GetParseError(string filename, string message)
        {
            StringBuilder builder = new StringBuilder();
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
