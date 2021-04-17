using System;

namespace Gtt.FastPass
{
    public class ApiTestSuiteAttribute : Attribute
    {
        public ApiTestSuiteAttribute()
        {
        }

        public ApiTestSuiteAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
