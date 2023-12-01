using System;

namespace Rendering.Exceptions;

public class MaterialNotFoundException : Exception
{
    public MaterialNotFoundException(string message) : base(message)
    {
    }
}