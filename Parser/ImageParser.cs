using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using Rendering.Exceptions;
using Rendering.Primitives;

namespace Rendering.Parser;

public class ImageParser
{
    public MaterialMap Parse(string filePath)
    {
        Bitmap bitmap;
        try
        {
            bitmap = new Bitmap(filePath);
        }
        catch (FileNotFoundException e)
        {
            throw new MaterialNotFoundException($"File {filePath} not found");
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

            for (var y = height - 1; y >= 0; y--)
            for (var x = 0; x < width; x++)
            {
                float r = 0, g = 0, b = 0;
                if (bytesPerPixel >= 1) b = rgbValues[index++] / maxColorValue;
                if (bytesPerPixel >= 2) g = rgbValues[index++] / maxColorValue;
                if (bytesPerPixel >= 3) r = rgbValues[index++] / maxColorValue;

                normalValues[x, y] = new Vector3(r, g, b);

                // Skip alpha component
                if (bytesPerPixel > 3) index += bytesPerPixel - 3;
            }
        }
        finally
        {
            bitmap.UnlockBits(bitmapData);
        }

        return new MaterialMap(normalValues);
    }
}