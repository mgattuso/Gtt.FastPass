using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Gtt.FastPass
{
    public static class GlobalResults
    {
        public static Dictionary<Guid, Dictionary<string, TestDefinition>> Tests = new Dictionary<Guid, Dictionary<string, TestDefinition>>();
    }

    public class TestDefinition
    {
        public string Key { get; set; }
        public Type TestClass { get; set; }
        public MethodInfo TestMethod { get; set; }
        public bool TestHasBeenRun => TestResult != null;
        public FastPassResponse TestResult => EndPoint?.Response;
        public FastPassEndpoint EndPoint { get; set; }
        public Exception Exception { get; set; }
        public string File { get; set; }

        public void Execute(Guid session)
        {
            var test = TestMethod;
            var suite = Activator.CreateInstance(TestClass);
            var testRoot = EndPoint;
            var p = test.GetParameters();

            try
            {
                if (!TestHasBeenRun)
                {
                    test.Invoke(suite, new object[] { testRoot });
                }
            }
            catch (Exception ex)
            {
                GlobalResults.Tests[session][Key].Exception = ex;
            }
        }
    }
}
