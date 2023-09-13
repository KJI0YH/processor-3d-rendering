﻿using Lab1.Exceptions;
using Lab1.Primitives;
using System;
using System.Collections.Generic;
using System.IO;

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
                string line;
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
                                model.AddPolygon(ParsePolygon(tokens, model.Vertices));
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (ParserException exception)
                {
                    //localize exception with lineCount and line
                    //vertices read
                }
            }
            return model;
        }

        private Vector3 ParseVertex(string[] tokens)
        {
            if (tokens.Length >= 4)
            {
                if (float.TryParse(tokens[1], out float x) && float.TryParse(tokens[2], out float y) && float.TryParse(tokens[3], out float z))
                {
                    if (tokens.Length == 5 && float.TryParse(tokens[4], out float w))
                    {
                        return new Vector3(x, y, z, w);
                    }
                    return new Vector3(x, y, z);
                }
            }
            throw new ParserException("Invalid vertex syntax");
        }

        private Vector3 ParseVertexTexture(string[] tokens)
        {
            // TODO parse vertex texture
            return new Vector3();
        }

        private Vector3 ParseVertexNormal(string[] tokens)
        {
            // TODO parse vertex normal
            return new Vector3();
        }

        private Polygon ParsePolygon(string[] tokens, List<Vector3> readVertices)
        {
            if (tokens.Length >= 4)
            {
                Polygon polygon = new Polygon();
                for (int i = 1; i < tokens.Length; i++)
                {
                    string[] vertices = tokens[i].Split('/');
                    if (int.TryParse(vertices[0], out int vertexIndex))
                    {
                        // Correct vertex index to access from early readed vertices
                        if (vertexIndex < 0) vertexIndex += readVertices.Count;
                        else vertexIndex--;

                        try
                        {
                            polygon.AddVertex(readVertices[vertexIndex]);
                        }
                        catch (IndexOutOfRangeException)
                        {
                            throw new ParserException("Invalid polygon vertex number index");
                        }
                    }
                }
                return polygon;
            }
            throw new ParserException("Invalid polygon syntax");
        }
    }
}