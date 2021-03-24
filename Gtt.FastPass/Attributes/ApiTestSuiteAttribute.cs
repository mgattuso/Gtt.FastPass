using System;

namespace Gtt.FastPass.Attributes
{
    public class ApiTestSuiteAttribute : Attribute
    {
        public ApiTestSuiteAttribute()
        {
            Name = this.GetType().Name;
        }

        public ApiTestSuiteAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
