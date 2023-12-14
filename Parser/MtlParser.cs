using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.RegularExpressions;
using Rendering.Exceptions;
using Rendering.Primitives;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Image = System.Drawing.Image;

namespace Rendering.Parser;

public class MtlParser
{
    private string? _currentDirectory;
    private Material? _currentMaterial;
    private ImageParser _imageParser = new();

    private void InitParser()
    {
        _currentMaterial = null;
    }

    public List<Material> Parse(string filePath)
    {
        _currentDirectory = Path.GetDirectoryName(filePath);
        InitParser();
        List<Material> materials = new();
        Stream fileStream;
        try
        {
            fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }
        catch (FileNotFoundException e)
        {
            throw new MaterialNotFoundException($"Mtl file {filePath} not found");
        }

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
                    case "newmtl":
                        _currentMaterial = ParseMaterial(tokens);
                        materials.Add(_currentMaterial);
                        break;
                    case "map_Kd":
                        _currentMaterial.Diffuse = ParseMaterialMap(tokens);
                        break;
                    case "map_Ks":
                        _currentMaterial.Mirror = ParseMaterialMap(tokens);
                        break;
                    case "norm":
                        _currentMaterial.Normal = ParseMaterialMap(tokens);
                        break;
                    case "map_MRAO":
                        _currentMaterial.MRAO = ParseMaterialMap(tokens);
                        break;
                }
            }
        }
        catch (ParserException exception)
        {
            throw new ParserException(
                $"Error in file: {filePath}\r\nin line: {lineCount}\r\nLine: {line}\r\nException: {exception.Message}");
        }

        return materials;
    }

    private Material ParseMaterial(string[] tokens)
    {
        if (tokens.Length < 2) throw new ParserException("Invalid newmtl syntax");
        return new Material(tokens[1]);
    }

    private MaterialMap ParseMaterialMap(string[] tokens)
    {
        if (tokens.Length < 2) throw new ParserException("Invalid diffuse syntax");
        if (_currentDirectory == null) throw new ParserException("Invalid directory path");
        var filePath = Path.Combine(_currentDirectory, tokens[1]);
        return _imageParser.Parse(filePath);
    }
}