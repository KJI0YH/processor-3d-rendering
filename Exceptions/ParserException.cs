using System;

namespace Lab1.Exceptions
{
    public class ParserException : Exception
    {
        public ParserException() { }

        public ParserException(string message) : base(message) { }
    }
}
