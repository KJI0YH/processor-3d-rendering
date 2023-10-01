using simple_3d_rendering.Exceptions;
using simple_3d_rendering.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Media.Media3D;

namespace Lab1.Primitives
{
    public class Polygon
    {
        public List<Vertex> Vertices { get; } = new();
        public Vector3 Normal
        {
            get
            {
                return GetNormal();
            }
        }

        public Polygon(List<Vertex> vertices)
        {
            if (vertices.Count < 3) throw new InvalidPolygonException("Vertices count less than 3");

            foreach (var vertex in vertices)
            {
                Vertices.Add(vertex);
            }
        }

        public List<Polygon> Triangulate()
        {
            if (Vertices.Count == 3)
            {
                return new List<Polygon>() { this };
            }

            List<Polygon> triangles = new();

            while (Vertices.Count >= 3)
            {
                List<double> relativeEarArea = new();
                for (int i = 0; i < Vertices.Count; i++)
                {
                    int indA = i;
                    int indB = (i + 1) % Vertices.Count;
                    int indC = (i == 0) ? (Vertices.Count - 1) : (i - 1);
                    Point3D pA = new(Vertices[indA].Original.X, Vertices[indA].Original.Y, Vertices[indA].Original.Z); ;
                    Point3D pB = new(Vertices[indB].Original.X, Vertices[indB].Original.Y, Vertices[indB].Original.Z);
                    Point3D pC = new(Vertices[indC].Original.X, Vertices[indC].Original.Y, Vertices[indC].Original.Z);
                    Vector3D BA = pB - pA;
                    Vector3D CB = pC - pB;
                    Vector3D AC = pA - pC;
                    double triangleSquare = (Vector3D.CrossProduct(BA, CB).Length) / 2;
                    double circleR = BA.Length * CB.Length * AC.Length / 4 / triangleSquare;
                    double circleSquare = Math.PI * circleR * circleR;
                    relativeEarArea.Add(triangleSquare / circleSquare);
                }

                // Find index of ear with max relative ear area
                double maxRelativeEarArea = relativeEarArea.Max();
                int indEar = relativeEarArea.FindIndex(r => r == maxRelativeEarArea);
                int indEarRight = (indEar + 1) % Vertices.Count;
                int indEarLeft = indEar == 0 ? (Vertices.Count - 1) : (indEar - 1);

                // Clip polygon
                Polygon polygon = new(new() {
                    Vertices[indEar],
                    Vertices[indEarRight],
                    Vertices[indEarLeft],
                });
                triangles.Add(polygon);

                // Delete vertex from main polygon
                Vertices.RemoveAt(indEar);
            }

            return triangles;
        }

        private Vector3 GetNormal()
        {
            Vector4 ab = Vertices[0].Transform - Vertices[1].Transform;
            Vector4 ac = Vertices[2].Transform - Vertices[0].Transform;
            return Vector3.Normalize(Vector3.Cross(new Vector3(ab.X, ab.Y, ab.Z), new Vector3(ac.X, ac.Y, ac.Z)));

        }
    }
}
