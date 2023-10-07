using System;

namespace Rendering.Exceptions
{
    public class InvalidPolygonException : Exception
    {
        public InvalidPolygonException(string message) : base(message) { }
    }
}
