using System;
using Rendering.Exceptions;
using Rendering.Objects;
using Rendering.Primitives;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace Rendering.Parser;

public class ObjParser : IModelParser
{
    private readonly List<Position> _readPositions = new();
    private readonly List<Vector3> _readNormals = new();
    private readonly List<Vector3> _readTextures = new();
    private readonly List<Polygon> _readPolygons = new();

    private void InitParser()
    {
        _readPositions.Clear();
        _readNormals.Clear();
        _readTextures.Clear();
        _readPolygons.Clear();
    }

    public Model Parse(string filename)
    {
        InitParser();
        var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
        using var streamReader = new StreamReader(fileStream);
        var line = string.Empty;
        var lineCount = 0;
        try
        {
            while ((line = streamReader?.ReadLine()) != null)
            {
                lineCount++;
                var tokens = line.Trim().Replace('.', ',').Split(' ');
                switch (tokens[0])
                {
                    case "v":
                        _readPositions.Add(ParsePosition(tokens));
                        break;
                    case "vt":
                        _readTextures.Add(ParseTexture(tokens));
                        break;
                    case "vn":
                        _readNormals.Add(ParseNormal(tokens));
                        break;
                    case "f":
                        _readPolygons.AddRange(ParsePolygon(tokens));
                        break;
                }
            }
        }
        catch (ParserException exception)
        {
            throw new ParserException(
                $"Error in line: {lineCount}\r\nLine: {line}\r\nException: {exception.Message}");
        }

        Model model = new(_readPositions, _readPolygons);
        if (model.IsEmpty()) throw new ParserException($"File does not contain a model in obj format");
        return model;
    }


    private static Position ParsePosition(string[] tokens)
    {
        if (tokens.Length >= 4)
            if (float.TryParse(tokens[1], out var x) &&
                float.TryParse(tokens[2], out var y) &&
                float.TryParse(tokens[3], out var z))
            {
                if (tokens.Length == 5 && float.TryParse(tokens[4], out var w))
                    return new Position(new Vector4(x, y, z, w));
                return new Position(new Vector4(x, y, z, 1));
            }

        throw new ParserException("Invalid vertex syntax");
    }

    private static Vector3 ParseTexture(string[] tokens)
    {
        var values = new float[3];
        var length = Math.Min(3, tokens.Length - 1);
        if (length == 0) throw new ParserException("Invalid vertex texture syntax");
        for (var index = 0; index < length; index++)
            if (!float.TryParse(tokens[index + 1], out values[index]))
                throw new ParserException("Invalid vertex texture syntax");
        return new Vector3(values[0], values[1], values[2]);
    }

    private static Vector3 ParseNormal(string[] tokens)
    {
        if (tokens.Length != 4) throw new ParserException("Invalid vertex normal syntax");

        if (float.TryParse(tokens[1], out var i) &&
            float.TryParse(tokens[2], out var j) &&
            float.TryParse(tokens[3], out var k))
            return new Vector3(i, j, k);

        throw new ParserException("Invalid vertex normal syntax");
    }

    private IEnumerable<Polygon> ParsePolygon(IReadOnlyList<string> tokens)
    {
        if (tokens.Count < 4) throw new ParserException("Invalid polygon syntax");

        List<Vertex> vertices = new();
        for (var i = 1; i < tokens.Count; i++)
        {
            Vertex vertex = new();
            var token = tokens[i].Split('/');

            // Parse vertex index
            if (int.TryParse(token[0], out var vIndex))
                vertex.Position = _readPositions[CorrectIndex(vIndex, _readPositions.Count)];

            // Parse vertex texture index
            if (int.TryParse(token[1], out var vtIndex))
                vertex.Texture = _readTextures[CorrectIndex(vtIndex, _readTextures.Count)];

            // Parse vertex normal index
            if (int.TryParse(token[2], out var vnIndex))
                vertex.Normal = _readNormals[CorrectIndex(vnIndex, _readNormals.Count)];

            vertices.Add(vertex);
        }

        try
        {
            var polygon = new Polygon(vertices);
            if (polygon.CanTriangulate())
                return polygon.Triangulate();
            return new List<Polygon>()
            {
                polygon
            };
        }
        catch (InvalidPolygonException exception)
        {
            throw new ParserException($"Invalid polygon: {exception.Message}");
        }
    }

    private static int CorrectIndex(int index, int maxLength)
    {
        if (index < 0)
            return index + maxLength;
        return --index;
    }
}