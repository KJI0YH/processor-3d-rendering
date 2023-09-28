using System;

namespace simple_3d_rendering.Exceptions
{
    public class InvalidPolygonException : Exception
    {
        public InvalidPolygonException(string message) : base(message) { }
    }
}
