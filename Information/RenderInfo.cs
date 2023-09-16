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
                .AppendLine($"Scale: {model.Scale}")
                .AppendLine($"Rotate X: {RadianToDegree(model.XAxisRotate):N0}°")
                .AppendLine($"Rotate Y: {RadianToDegree(model.YAxisRotate):N0}°")
                .AppendLine($"Rotate Z: {RadianToDegree(model.ZAxisRotate):N0}°")
                .AppendLine($"Move: ({model.Translation.X}, {model.Translation.Y}, {model.Translation.Z})")
                .AppendLine($"R: {camera.SphericalPosition.R}")
                .AppendLine($"Azimuth angle: {RadianToDegree(camera.SphericalPosition.AzimuthAngle):N0}°")
                .AppendLine($"Elevation angle: {RadianToDegree(camera.SphericalPosition.ElevationAngle):N0}°")
                .AppendLine($"Camera position: ({cameraPosition.X}, {cameraPosition.Y}, {cameraPosition.Z})")
                .AppendLine($"Camera target: ({camera.Target.X}, {camera.Target.Y}, {camera.Target.Z})")
                .AppendLine($"FOV: {RadianToDegree(camera.FOV):N0}°")
                .AppendLine($"Rasterization: {rasterization.GetType().Name}")
                .AppendLine($"Screen width: {camera.ScreenWidth}")
                .AppendLine($"Screen height: {camera.ScreenHeight}")
                .AppendLine($"Screen aspect: {camera.ScreenWidth / camera.ScreenHeight}");
            return builder.ToString();
        }

        private float RadianToDegree(float radian)
        {
            return radian * 180 / MathF.PI;
        }
    }
}
