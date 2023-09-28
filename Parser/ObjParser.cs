using Lab1.Exceptions;
using Lab1.Primitives;
using simple_3d_rendering.Exceptions;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace Lab1.Parser
{
    public class ObjParser : IModelParser
    {
        public Model Parse(string filename)
        {
            Model model = new Model();
            var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read); ;
            using (var streamReader = new StreamReader(fileStream))
            {
                string line = string.Empty;
                int lineCount = 0;
                try
                {
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        lineCount++;
                        string[] tokens = line.Trim().Replace('.', ',').Split(' ');
                        switch (tokens[0])
                        {
                            case "v":
                                model.AddVertex(ParseVertex(tokens));
                                break;
                            case "vt":
                                model.AddVertexTexture(ParseVertexTexture(tokens));
                                break;
                            case "vn":
                                model.AddVertexNormal(ParseVertexNormal(tokens));
                                break;
                            case "f":
                                Polygon polygon = ParsePolygon(tokens, model.Vertices);
                                if (polygon.Vertices.Count > 3)
                                {
                                    model.AddPolygon(polygon.Triangulate());
                                }
                                else
                                {
                                    model.AddPolygon(polygon);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (ParserException exception)
                {
                    throw new ParserException($"Error in line: {lineCount}\r\nLine: {line}\r\nException: {exception.Message}");
                }
                if (model.IsEmpty())
                {
                    throw new ParserException($"File does not contain a model in obj format");
                }
            }
            return model;
        }

        private Vector4 ParseVertex(string[] tokens)
        {
            if (tokens.Length >= 4)
            {
                if (float.TryParse(tokens[1], out float x) && float.TryParse(tokens[2], out float y) && float.TryParse(tokens[3], out float z))
                {
                    if (tokens.Length == 5 && float.TryParse(tokens[4], out float w))
                    {
                        return new Vector4(x, y, z, w);
                    }
                    return new Vector4(x, y, z, 1);
                }
            }
            throw new ParserException("Invalid vertex syntax");
        }

        private Vector4 ParseVertexTexture(string[] tokens)
        {
            // TODO parse vertex texture
            return new Vector4();
        }

        private Vector4 ParseVertexNormal(string[] tokens)
        {
            // TODO parse vertex normal
            return new Vector4();
        }

        private Polygon ParsePolygon(string[] tokens, List<Vector4> readVertices)
        {
            if (tokens.Length >= 4)
            {
                List<(Vector4, int)> vertices = new();
                for (int i = 1; i < tokens.Length; i++)
                {
                    string[] vertexToken = tokens[i].Split('/');
                    if (int.TryParse(vertexToken[0], out int vertexIndex))
                    {
                        // Correct vertex index to access from early readed vertices
                        if (vertexIndex < 0) vertexIndex += readVertices.Count;
                        else vertexIndex--;

                        vertices.Add((readVertices[vertexIndex], vertexIndex));
                    }
                }

                try
                {
                    return new Polygon(vertices);
                }
                catch (InvalidPolygonException exception)
                {
                    throw new ParserException($"Invalid polygon: {exception.Message}");
                }
            }
            throw new ParserException("Invalid polygon syntax");
        }
    }
}
