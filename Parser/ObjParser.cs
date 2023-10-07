using Rendering.Exceptions;
using Rendering.Objects;
using Rendering.Primitives;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace Rendering.Parser
{
    public class ObjParser : IModelParser
    {
        public Model Parse(string filename)
        {
            Model model = new();
            var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read); ;
            using (var streamReader = new StreamReader(fileStream))
            {
                string line = string.Empty;
                int lineCount = 0, vertexCount = 0;
                try
                {
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        lineCount++;
                        string[] tokens = line.Trim().Replace('.', ',').Split(' ');
                        switch (tokens[0])
                        {
                            case "v":
                                model.AddVertex(ParseVertex(tokens, ++vertexCount));
                                break;
                            case "vt":
                                break;
                            case "vn":
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

        private Vertex ParseVertex(string[] tokens, int vertexIndex)
        {
            if (tokens.Length >= 4)
            {
                if (float.TryParse(tokens[1], out float x) && float.TryParse(tokens[2], out float y) && float.TryParse(tokens[3], out float z))
                {
                    if (tokens.Length == 5 && float.TryParse(tokens[4], out float w))
                    {
                        return new Vertex(new Vector4(x, y, z, w), vertexIndex);
                    }
                    return new Vertex(new Vector4(x, y, z, 1), vertexIndex);
                }
            }
            throw new ParserException("Invalid vertex syntax");
        }

        private Polygon ParsePolygon(string[] tokens, List<Vertex> readVertices)
        {
            if (tokens.Length >= 4)
            {
                List<Vertex> vertices = new();
                for (int i = 1; i < tokens.Length; i++)
                {
                    string[] vertexToken = tokens[i].Split('/');
                    if (int.TryParse(vertexToken[0], out int vertexIndex))
                    {
                        // Correct vertex index to access from early readed vertices
                        if (vertexIndex < 0) vertexIndex += readVertices.Count;
                        else vertexIndex--;

                        vertices.Add(readVertices[vertexIndex]);
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
