using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Gtt.FastPass.Attributes;

namespace Gtt.FastPass
{
    public class TestRepository
    {
        public List<TestDefinition> Tests { get; set; } = new List<TestDefinition>();
        public List<MethodInfo> WarmUpMethods { get; set; } = new List<MethodInfo>();
    }

    public static class FastPassTestRunner
    {
        private static readonly TestRepository Repository = new TestRepository();

        public static void CollectTests(FastPassEndpoint root)
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
                    var testAttr = test.GetCustomAttribute<ApiTestAttribute>();
                    string testName = !string.IsNullOrWhiteSpace(testAttr.Name)
                        ? $"{testAttr.Name} ({test.Name})"
                        : test.Name;
                    string testReference = $"{t.Name}:{test.Name}";
                    var endpoint = root.Clone(testReference);
                    endpoint.Name = testName;

                    var p = test.GetParameters();
                    if (p.Length != 0 && p[0].ParameterType != typeof(FastPassEndpoint))
                    {
                        Console.WriteLine(
                            $"Cannot call test {name} with the parameters defined. Should take a single argument of the type 'FastPassEndpoint'");
                    }

                    Repository.Tests.Add(new TestDefinition
                    {
                        Key = testReference,
                        TestClass = t,
                        TestMethod = test,
                        EndPoint = endpoint
                    });
                }

                var warmUps = t.GetMembers().Where(m => m.GetCustomAttribute<WarmUpAttribute>() != null).OfType<MethodInfo>();

                foreach (MethodInfo warmUp in warmUps)
                {
                    Repository.WarmUpMethods.Add(warmUp);
                }

                GlobalResults.FailedTests = 0;
                GlobalResults.PassedTests = 0;
            }
        }

        public static void RunWarmUps(FastPassEndpoint root)
        {
            foreach (var warmUp in Repository.WarmUpMethods)
            {
                var w = Activator.CreateInstance(warmUp.DeclaringType);
                Console.WriteLine($"Running warm up: {warmUp.DeclaringType.Name}:{warmUp.Name}");
                warmUp.Invoke(w, new object[] { root.Clone($"WARMUP {warmUp.DeclaringType.Name}-{warmUp.Name}") });
            }

            GlobalResults.FailedTests = 0;
            GlobalResults.PassedTests = 0;
        }


        public static int RunAllTests(FastPassEndpoint root)
        {
            CollectTests(root);
            RunWarmUps(root);

            GlobalResults.Tests = Repository.Tests.ToDictionary(x => x.Key);

            foreach (var testDefinition in GlobalResults.Tests)
            {
                testDefinition.Value.Execute();
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
        public bool WarnOnResponseTimeFailures { get; set; }
    }
}
