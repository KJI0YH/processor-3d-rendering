using System;
using Rendering.Exceptions;
using Rendering.Objects;
using Rendering.Primitives;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Rendering.Parser;

public class ObjParser : IModelParser
{
    private readonly List<Position> _readPositions = new();
    private readonly List<Normal> _readNormals = new();
    private readonly List<Vector3> _readTextures = new();
    private readonly List<Polygon> _readPolygons = new();
    private readonly List<Material> _readMaterials = new();
    private readonly MtlParser _mtlParser = new();
    private string? _currentDirectory;
    private Material? _currentMaterial;

    private void InitParser()
    {
        _readPositions.Clear();
        _readNormals.Clear();
        _readTextures.Clear();
        _readPolygons.Clear();
    }

    public Model Parse(string filePath)
    {
        _currentDirectory = Path.GetDirectoryName(filePath);
        InitParser();
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var streamReader = new StreamReader(fileStream);
        var line = string.Empty;
        var lineCount = 0;
        try
        {
            while ((line = streamReader?.ReadLine()) != null)
            {
                lineCount++;
                line = Regex.Replace(line, @"\s{2,}", " ");
                var tokens = line.Trim().Split(' ');
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
                    case "mtllib":
                        _readMaterials.AddRange(ParseMaterials(tokens));
                        break;
                    case "usemtl":
                        _currentMaterial = ParseMaterial(tokens);
                        break;
                }
            }
        }
        catch (ParserException exception)
        {
            throw new ParserException(
                $"Error in file: {filePath}\r\nin line: {lineCount}\r\nLine: {line}\r\nException: {exception.Message}");
        }

        SetVertexNormals();
        Model model = new(_readPositions, _readNormals, _readPolygons);
        if (model.IsEmpty()) throw new ParserException($"File does not contain a model in obj format");
        return model;
    }

    private Position ParsePosition(string[] tokens)
    {
        if (tokens.Length >= 4)
            if (ToFloat(tokens[1], out var x) &&
                ToFloat(tokens[2], out var y) &&
                ToFloat(tokens[3], out var z))
            {
                if (tokens.Length == 5 && ToFloat(tokens[4], out var w))
                    return new Position(new Vector4(x, y, z, w));
                return new Position(new Vector4(x, y, z, 1));
            }

        throw new ParserException("Invalid vertex syntax");
    }

    private Vector3 ParseTexture(string[] tokens)
    {
        var values = new float[3];
        var length = Math.Min(3, tokens.Length - 1);
        if (length == 0) throw new ParserException("Invalid vertex texture syntax");
        for (var index = 0; index < length; index++)
            if (!ToFloat(tokens[index + 1], out values[index]))
                throw new ParserException("Invalid vertex texture syntax");
        return new Vector3(values[0], values[1], values[2]);
    }

    private Normal ParseNormal(string[] tokens)
    {
        if (tokens.Length != 4) throw new ParserException("Invalid vertex normal syntax");

        if (ToFloat(tokens[1], out var i) &&
            ToFloat(tokens[2], out var j) &&
            ToFloat(tokens[3], out var k))
            return new Normal(Vector4.Normalize(new Vector4(i, j, k, 0)));

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
            var tokenCount = token.Length;

            // Parse vertex index
            if (tokenCount >= 1 && int.TryParse(token[0], out var vIndex))
                vertex.Position = _readPositions[CorrectIndex(vIndex, _readPositions.Count)];

            // Parse vertex texture index
            if (tokenCount >= 2 && int.TryParse(token[1], out var vtIndex))
                vertex.Texture = _readTextures[CorrectIndex(vtIndex, _readTextures.Count)];

            // Parse vertex normal index
            if (tokenCount >= 3 && int.TryParse(token[2], out var vnIndex))
                vertex.Normal = _readNormals[CorrectIndex(vnIndex, _readNormals.Count)];

            vertices.Add(vertex);
        }

        try
        {
            var polygon = new Polygon(vertices, _currentMaterial);
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

    private int CorrectIndex(int index, int maxLength)
    {
        if (index < 0)
            return index + maxLength;
        return --index;
    }

    private void SetVertexNormals()
    {
        Vector4 empty = new(0, 0, 0, 0);
        var verticesWithoutNormal = _readPolygons
            .SelectMany(polygon => polygon.Vertices)
            .Where(vertex => vertex.Normal.Original.Equals(empty))
            .Distinct()
            .ToList();

        foreach (var vertex in verticesWithoutNormal)
        {
            var polygons = _readPolygons
                .Where(polygon => polygon.Vertices.Contains(vertex))
                .ToList();

            Vector3 approximatedNormal = new(0, 0, 0);
            approximatedNormal = polygons.Aggregate(approximatedNormal, (current, polygon) => current + polygon.Normal);
            approximatedNormal /= polygons.Count;
            var normal = new Normal(new Vector4(approximatedNormal, 0));
            _readNormals.Add(normal);

            foreach (var polygon in polygons)
            {
                var index = polygon.Vertices.IndexOf(vertex);
                if (index != -1)
                {
                    var newVertex = vertex;
                    newVertex.Normal = normal;
                    polygon.Vertices[index] = newVertex;
                }
            }
        }
    }

    private List<Material> ParseMaterials(string[] tokens)
    {
        if (tokens.Length < 2) throw new ParserException("Invalid mtllib syntax");
        if (_currentDirectory == null) throw new ParserException("Invalid directory path");
        return _mtlParser.Parse(Path.Combine(_currentDirectory, tokens[1]));
    }

    private Material ParseMaterial(string[] tokens)
    {
        if (tokens.Length < 2) throw new ParserException("Invalid usemtl syntax");
        var materialName = tokens[1];
        var material = _readMaterials.Find(m => m.Name == materialName);
        if (material == null) throw new ParserException($"Material with name {materialName} does not exists");
        return material;
    }

    private bool ToFloat(string value, out float result)
    {
        return float.TryParse(value, CultureInfo.InvariantCulture, out result);
    }
}