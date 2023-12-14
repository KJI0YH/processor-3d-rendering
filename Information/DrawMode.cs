using System.ComponentModel;

namespace Rendering.Information;

public enum DrawMode
{
    [Description("Vertex only")] VertexOnly,
    [Description("Wire")] Wire,
    [Description("Lambert")] Lambert,
    [Description("Phong shading")] PhongShading,
    [Description("Phong lighting")] PhongLighting,
    [Description("Texture")] Texture,
    [Description("Custom texture")] Custom,

    [Description("Physically based rendering")]
    PBR
}