using System.Collections.Generic;

namespace Lab1.Primitives
{
    public class Polygon
    {
        public List<Vector3> Vertices { get; } = new List<Vector3>();
        public List<int> Indices { get; } = new List<int>();
        public List<Vector3> VerticesTextures { get; } = new List<Vector3>();
        public List<Vector3> VerticesNormals { get; } = new List<Vector3>();

        public Polygon()
        {

        }

        public void AddVertex(List<Vector3> readVertices, int vertexIndex)
        {
            Vertices.Add(readVertices[vertexIndex]);
            Indices.Add(vertexIndex);
        }

        public void AddVertexTexture(Vector3 vertexTexture)
        {
            VerticesTextures.Add(vertexTexture);
        }

        public void AddVertexNormal(Vector3 vertexNormal)
        {
            VerticesNormals.Add(vertexNormal);
        }
    }
}
