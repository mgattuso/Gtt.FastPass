using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Gtt.FastPass.Attributes;

namespace Gtt.FastPass
{
    public class FastPassTestRunner<T>
    {
        private readonly FastPassEndpoint _root;
        private readonly TestRepository _repository = new TestRepository();
        private readonly Guid _session = Guid.NewGuid();

        public FastPassTestRunner(FastPassEndpoint root)
        {
            _root = root;
            _root.SessionId = _session;
            GlobalResults.Tests[_session] = new Dictionary<string, TestDefinition>();
            CollectTests();
        }

        private void CollectTests()
        {
            var assemblies = typeof(T).Assembly.GetReferencedAssemblies().ToList();
            assemblies.Add(typeof(T).Assembly.GetName());
            var asf = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var a in asf)
            {
                if (assemblies.All(x => x.FullName != a.FullName))
                {
                    assemblies.Add(a.GetName());
                }
            }

            var types = assemblies.Distinct().Select(Assembly.Load).SelectMany(GetTypesWithApiAttribute);

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
                    var endpoint = _root.Clone(testReference, _session);
                    endpoint.Name = testName;

                    var p = test.GetParameters();
                    if (p.Length != 0 && p[0].ParameterType != typeof(FastPassEndpoint))
                    {
                        Console.WriteLine(
                            $"Cannot call test {name} with the parameters defined. Should take a single argument of the type 'FastPassEndpoint'");
                    }

                    var w = Activator.CreateInstance(test.DeclaringType);
                    var x = new StackTrace(true);
                    var file = x.GetFrames();

                    _repository.Tests.Add(new TestDefinition
                    {
                        Key = testReference,
                        TestClass = t,
                        TestMethod = test,
                        EndPoint = endpoint,
                        File = file.ToString()
                    });
                }

                var warmUps = t.GetMembers().Where(m => m.GetCustomAttribute<WarmUpAttribute>() != null).OfType<MethodInfo>();

                foreach (MethodInfo warmUp in warmUps)
                {
                    _repository.WarmUpMethods.Add(warmUp);
                }
            }

            GlobalResults.Tests[_session] = _repository.Tests.ToDictionary(x => x.Key);
        }

        public void RunWarmUps()
        {
            foreach (var warmUp in _repository.WarmUpMethods)
            {
                var w = Activator.CreateInstance(warmUp.DeclaringType);
                Console.WriteLine($"Running warm up: {warmUp.DeclaringType.Name}:{warmUp.Name}");
                warmUp.Invoke(w, new object[] { _root.Clone($"WARMUP {warmUp.DeclaringType.Name}-{warmUp.Name}", _session) });
            }
        }

        public List<TestDefinition> GetTests()
        {
            return _repository.Tests;
        }

        public int RunAllTests()
        {
            if (!_root.Options.SkipWarmupTests)
            {
                RunWarmUps();
            }

            foreach (var testDefinition in GlobalResults.Tests[_session])
            {
                testDefinition.Value.Execute();
            }

            var failedTests = GlobalResults.Tests[_session].Sum(x => x.Value.TestResult.Results.Count(c => !c.Passed));

            return failedTests > 0 ? -1 : 0;
        }

        public void RunAsGui()
        {
            new GuiRunner<T>(this).Run();
        }

        private static IEnumerable<Type> GetTypesWithApiAttribute(Assembly assembly)
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
        public int HttpConnectionTimeoutSeconds { get; set; } = 100;
        public bool SkipWarmupTests { get; set; }
    }
}
