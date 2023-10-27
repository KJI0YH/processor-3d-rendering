using Rendering.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Media.Media3D;

namespace Rendering.Primitives;

public class Polygon
{
    public List<Vertex> Vertices { get; } = new();
    public Vector3 Normal => GetNormal();

    public Polygon(IReadOnlyCollection<Vertex> vertices)
    {
        if (vertices.Count < 3) throw new InvalidPolygonException("Vertices count less than 3");

        Vertices.AddRange(vertices);
    }

    public IEnumerable<Polygon> Triangulate()
    {
        if (Vertices.Count == 3) return new List<Polygon>() { this };

        List<Polygon> triangles = new();

        while (Vertices.Count >= 3)
        {
            List<double> relativeEarArea = new();
            for (var i = 0; i < Vertices.Count; i++)
            {
                var indA = i;
                var indB = (i + 1) % Vertices.Count;
                var indC = i == 0 ? Vertices.Count - 1 : i - 1;
                Point3D pA = new(Vertices[indA].Position.Original.X, Vertices[indA].Position.Original.Y,
                    Vertices[indA].Position.Original.Z);
                Point3D pB = new(Vertices[indB].Position.Original.X, Vertices[indB].Position.Original.Y,
                    Vertices[indB].Position.Original.Z);
                Point3D pC = new(Vertices[indC].Position.Original.X, Vertices[indC].Position.Original.Y,
                    Vertices[indC].Position.Original.Z);
                var ba = pB - pA;
                var cb = pC - pB;
                var ac = pA - pC;
                var triangleSquare = Vector3D.CrossProduct(ba, cb).Length / 2;
                var circleR = ba.Length * cb.Length * ac.Length / 4 / triangleSquare;
                var circleSquare = Math.PI * circleR * circleR;
                relativeEarArea.Add(triangleSquare / circleSquare);
            }

            // Find index of ear with max relative ear area
            var maxRelativeEarArea = relativeEarArea.Max();
            var indEar = relativeEarArea.FindIndex(r => r == maxRelativeEarArea);
            var indEarRight = (indEar + 1) % Vertices.Count;
            var indEarLeft = indEar == 0 ? Vertices.Count - 1 : indEar - 1;

            // Clip polygon
            Polygon polygon = new(new List<Vertex>
            {
                Vertices[indEar],
                Vertices[indEarRight],
                Vertices[indEarLeft]
            });
            triangles.Add(polygon);

            // Delete vertex from main polygon
            Vertices.RemoveAt(indEar);
        }

        return triangles;
    }

    private Vector3 GetNormal()
    {
        var ab = Vertices[1].Position.Transform - Vertices[0].Position.Transform;
        var ac = Vertices[2].Position.Transform - Vertices[0].Position.Transform;
        return Vector3.Normalize(Vector3.Cross(new Vector3(ab.X, ab.Y, ab.Z), new Vector3(ac.X, ac.Y, ac.Z)));
    }

    public bool CanTriangulate()
    {
        return Vertices.Count > 3;
    }
}