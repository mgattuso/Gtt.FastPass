using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Gtt.FastPass
{
    public static class GlobalResults
    {
        public static int FailedTests;
        public static int PassedTests;
        public static Dictionary<string, FastPassResponse> TestData = new Dictionary<string, FastPassResponse>();
        public static Dictionary<string, TestDefinition> Tests = new Dictionary<string, TestDefinition>();
    }

    public class TestDefinition
    {
        public Type TestClass { get; set; }
        public MethodInfo TestMethod { get; set; }
        public bool TestHasBeenRun => TestResult != null;
        public FastPassResponse TestResult => EndPoint?.Response;
        public FastPassEndpoint EndPoint { get; set; }
        public Exception Exception { get; set; }
    }
}
