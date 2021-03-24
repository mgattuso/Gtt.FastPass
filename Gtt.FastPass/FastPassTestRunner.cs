using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Gtt.FastPass.Attributes;

namespace Gtt.FastPass
{
    public static class FastPassTestRunner
    {
        public static int RunAllTests(FastPassEndpoint root)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(GetTypesWithHelpAttribute);

            foreach (var t in types)
            {
                var attr = t.GetCustomAttribute<ApiTestSuiteAttribute>();
                string name = attr?.Name ?? t.Name;
                Console.WriteLine($"Test Suite: {name}");
                Console.WriteLine();
                var tests = t.GetMembers().Where(m => m.GetCustomAttribute<ApiTestAttribute>() != null).OfType<MethodInfo>();
                foreach (MethodInfo test in tests)
                {
                    string testReference = $"{t.Name}:{test.Name}";
                    GlobalResults.Tests[testReference] = new TestDefinition
                    {
                        TestClass = t,
                        TestMethod = test,
                        EndPoint = root.Clone(testReference)
                    };
                    var p = test.GetParameters();
                    if (p.Length != 0 && p[0].ParameterType != typeof(FastPassEndpoint))
                    {
                        Console.WriteLine(
                            $"Cannot call test {name} with the parameters defined. Should take a single argument of the type 'FastPassEndpoint'");
                    }
                }
            }

            foreach (var testDefinition in GlobalResults.Tests)
            {
                var test = testDefinition.Value.TestMethod;
                var suite = Activator.CreateInstance(testDefinition.Value.TestClass);
                var testRoot = testDefinition.Value.EndPoint;
                var p = test.GetParameters();

                try
                {
                    if (!testDefinition.Value.TestHasBeenRun)
                    {
                        test.Invoke(suite, new object[] {testRoot});
                    }
                }
                catch (Exception ex)
                {
                    GlobalResults.Tests[testDefinition.Key].Exception = ex;
                }

            }

            return GlobalResults.FailedTests > 0 ? -1 : 0;
        }

        private static IEnumerable<Type> GetTypesWithHelpAttribute(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(ApiTestSuiteAttribute), true).Length > 0)
                {
                    yield return type;
                }
            }
        }
    }

    public class TestOptions
    {
        public bool PrintHttpContext { get; set; }
    }
}
