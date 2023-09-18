using System.Collections.Generic;
using System.Numerics;

namespace Lab1.Primitives
{
    public class Polygon
    {
        public List<Vector4> Vertices { get; } = new List<Vector4>();
        public List<int> Indices { get; } = new List<int>();
        public List<Vector4> VerticesTextures { get; } = new List<Vector4>();
        public List<Vector4> VerticesNormals { get; } = new List<Vector4>();

        public Polygon()
        {

        }

        public void AddVertex(List<Vector4> readVertices, int vertexIndex)
        {
            Vertices.Add(readVertices[vertexIndex]);
            Indices.Add(vertexIndex);
        }

        public void AddVertexTexture(Vector4 vertexTexture)
        {
            VerticesTextures.Add(vertexTexture);
        }

        public void AddVertexNormal(Vector4 vertexNormal)
        {
            VerticesNormals.Add(vertexNormal);
        }
    }
}
