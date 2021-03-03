using System;

namespace Infra.Net.Extensions.ExceptionHandlers
{
    public class ArgumentStringInvalidException : Exception
    {
        public string Name { get; }
        public string Value { get; }

        public ArgumentStringInvalidException(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
