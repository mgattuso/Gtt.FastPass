using System;

namespace Gtt.FastPass.Attributes
{
    public class ApiTestAttribute : Attribute
    {
        public string Name { get; }

        public ApiTestAttribute(string name = null)
        {
            Name = name ?? "";
        }
    }
}