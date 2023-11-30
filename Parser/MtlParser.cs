using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Rendering.Exceptions;
using Rendering.Primitives;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Media.TextFormatting;
using Image = System.Drawing.Image;

namespace Rendering.Parser;

public class MtlParser
{
    private string? _currentDirectory;
    private Material _currentMaterial;

    public List<Material> Parse(string filePath)
    {
        List<Material> materials = new();
        _currentDirectory = Path.GetDirectoryName(filePath);
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
        Bitmap bitmap;
        try
        {
            bitmap = new Bitmap(filePath);
        }
        catch (FileNotFoundException e)
        {
            throw new ParserException($"File {filePath} not found");
        }

        var width = bitmap.Width;
        var height = bitmap.Height;
        const float maxColorValue = 255.0f;
        var bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly,
            bitmap.PixelFormat);
        var bytesPerPixel = (Image.GetPixelFormatSize(bitmap.PixelFormat) + 7) / 8;
        var normalValues = new Vector3[width, height];
        var index = 0;

        try
        {
            var pointer = bitmapData.Scan0;
            var bytes = Math.Abs(bitmapData.Stride) * height;
            var rgbValues = new byte[bytes];
            Marshal.Copy(pointer, rgbValues, 0, bytes);

            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                normalValues[x, y] = new Vector3(
                    rgbValues[index++] / maxColorValue,
                    rgbValues[index++] / maxColorValue,
                    rgbValues[index++] / maxColorValue);

                // Skip alpha component
                index += bytesPerPixel - 3;
            }
        }
        finally
        {
            bitmap.UnlockBits(bitmapData);
        }

        return new MaterialMap(normalValues);
    }
}