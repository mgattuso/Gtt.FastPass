using System.Collections.Generic;
using System.Reflection;

namespace Gtt.FastPass
{
    public class TestRepository
    {
        public List<TestDefinition> Tests { get; set; } = new List<TestDefinition>();
        public List<MethodInfo> WarmUpMethods { get; set; } = new List<MethodInfo>();
    }
}