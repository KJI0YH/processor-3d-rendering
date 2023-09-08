using Lab1.Primitives;
using System.Collections.Generic;

namespace Lab1
{
    public class Model
    {
        public List<Vector3> Vertices { get; } = new List<Vector3>();
        public List<Vector3> VerticesTextures { get; } = new List<Vector3>();
        public List<Vector3> VerticesNormals { get; } = new List<Vector3>();
        public List<Polygon> Polygons { get; } = new List<Polygon>();

        public Model() { }

        public void AddVertex(Vector3 vertex)
        {
            Vertices.Add(vertex);
        }

        public void AddVertexTexture(Vector3 vertexTexture)
        {
            VerticesTextures.Add(vertexTexture);
        }

        public void AddVertexNormal(Vector3 vectorNormal)
        {
            VerticesNormals.Add(vectorNormal);
        }

        public void AddPolygon(Polygon polygon)
        {
            Polygons.Add(polygon);
        }
    }
}
