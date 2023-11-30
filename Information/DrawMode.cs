using System.ComponentModel;

namespace Rendering.Information;

public enum DrawMode
{
    [Description("Vertex only")] VertexOnly,
    [Description("Wire")] Wire,
    [Description("Rasterisation")] Rasterisation,
    [Description("Phong shading")] PhongShading,
    [Description("Phong lighting")] PhongLighting,
    [Description("Texture")] Texture
}