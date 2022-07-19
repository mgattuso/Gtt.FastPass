using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using Gtt.FastPass.Serializers;

namespace Gtt.FastPass
{
    public class FastPassRequestBuilder
    {
        private static readonly HttpClient Client = new HttpClient();
        private static bool _configurationComplete;
        private static readonly object Lock = new object();
        public Dictionary<string, string[]> Headers { get; } = new Dictionary<string, string[]>();
        public string Content { get; private set; }
        public HttpMethod Method { get; set; }

        public FastPassRequestBuilder(FastPassEndpoint endpoint, TestOptions options)
        {
            Endpoint = endpoint;
            if (!_configurationComplete)
            {
                lock (Lock)
                {
                    _configurationComplete = true;
                    Client.Timeout = new TimeSpan(0, 0, 0, options.HttpConnectionTimeoutSeconds);
                }
            }
            
        }


        public FastPassRequestBuilder WithHeader(string key, string value)
        {
            Headers[key] = new[] { value };
            return this;
        }

        public FastPassRequestBuilder WithHeader(string key, params string[] values)
        {
            Headers[key] = values;
            return this;
        }

        public FastPassRequestBuilder WithBody(string content)
        {
            Content = content;
            return this;
        }
        public FastPassRequestBuilder WithBody<T>(T obj)
        {
            var serializer = new JsonObjectSerializer(true);
            Content = serializer.Serialize(obj).GetAwaiter().GetResult();
            return this;
        }

        public FastPassEndpoint Endpoint { get; }

        public FastPassResponse Get()
        {
            return Get("");
        }

        public FastPassResponse Get(string path, bool resetPath = false)
        {
            if (!string.IsNullOrWhiteSpace(path))
                Endpoint.Endpoint(path, resetPath);

            return Call(HttpMethod.Get);
        }

        public FastPassResponse Post(string path = null, bool resetPath = false)
        {
            if (!string.IsNullOrWhiteSpace(path))
                Endpoint.Endpoint(path, resetPath);

            return Call(HttpMethod.Post);
        }

        public FastPassResponse PostWithBody<T>(string path, T obj, bool resetPath = false)
        {
            WithBody(obj);
            if (!string.IsNullOrWhiteSpace(path))
                Endpoint.Endpoint(path, resetPath);

            return Call(HttpMethod.Post);
        }

        public FastPassResponse PostWithBody<T>(T obj)
        {
            WithBody(obj);
            return Call(HttpMethod.Post);
        }

        public FastPassResponse Put(string path = null, bool resetPath = false)
        {
            if (!string.IsNullOrWhiteSpace(path))
                Endpoint.Endpoint(path, resetPath);

            return Call(HttpMethod.Put);
        }

        public FastPassResponse PutWithBody<T>(string path, T obj, bool resetPath = false)
        {
            WithBody(obj);
            if (!string.IsNullOrWhiteSpace(path))
                Endpoint.Endpoint(path, resetPath);

            return Call(HttpMethod.Put);
        }

        /// <summary>
        /// PUT using the object specified as the payload
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public FastPassResponse PutWithBody<T>(T obj)
        {
            WithBody(obj);
            return Call(HttpMethod.Put);
        }

        public FastPassResponse Delete(string path = null, bool resetPath = false)
        {
            if (!string.IsNullOrWhiteSpace(path))
                Endpoint.Endpoint(path, resetPath);

            return Call(HttpMethod.Delete);
        }

        public FastPassResponse DeleteWithBody<T>(string path, T obj, bool resetPath)
        {
            WithBody(obj);
            if (!string.IsNullOrWhiteSpace(path))
                Endpoint.Endpoint(path, resetPath);

            return Call(HttpMethod.Delete);
        }

        public FastPassResponse DeleteWithBody<T>(T obj)
        {
            WithBody(obj);
            return Call(HttpMethod.Delete);
        }

        public FastPassResponse Call(HttpMethod method)
        {
            Method = method;
            string[] contentHeaders = {
                "Content-Type"
            };

            Console.WriteLine();
            using (var cw = new ConsoleWithColor(ConsoleColor.DarkYellow))
            {
                var title = "TEST: " + Endpoint.Name;
                cw.WriteLine(title);
                Console.WriteLine(new string('=', title.Length));
            }

            var msg = new HttpRequestMessage(method, Endpoint.BuildUrl());
            foreach (var header in Headers)
            {
                if (!contentHeaders.Contains(header.Key, StringComparer.InvariantCultureIgnoreCase))
                {
                    msg.Headers.Add(header.Key, header.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(Content))
            {
                msg.Content = new StringContent(Content, Encoding.UTF8, "application/json");
            }

            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            Stopwatch sw = new Stopwatch();
            sw.Start();
            var response = Client.SendAsync(msg, HttpCompletionOption.ResponseContentRead, cts.Token).GetAwaiter().GetResult();
            sw.Stop();
            return new FastPassResponse(this, msg, response, sw.ElapsedMilliseconds);
        }

        public FastPassRequestBuilder DependentOn<TRes>(Func<FastPassEndpoint, TRes> otherCall, Action<TRes> response)
        {
            var otherMethod = otherCall.GetMethodInfo();
            var otherClass = otherMethod.DeclaringType;
            TestDefinition currentDefinition = GlobalResults.Tests[Endpoint.SessionId][Endpoint.TestId];
            TestDefinition dependencyDefinition = GlobalResults.Tests[Endpoint.SessionId][$"{otherClass.Name}:{otherMethod.Name}"];
            string dependencyKey = currentDefinition.Key + "-" + dependencyDefinition.Key;
            string currentKey = dependencyDefinition.Key + "-" + currentDefinition.Key;
            if (_dependencies.Contains(currentKey))
            {
                throw new Exception($"Circular dependency exception. The dependency on ${dependencyDefinition.Key} is circular");
            }
            _dependencies.Add(dependencyKey);
            if (!dependencyDefinition.TestHasBeenRun)
            {
                dependencyDefinition.Execute();
            }

            if (dependencyDefinition.TestResult != null && dependencyDefinition.TestResult.AllTestsPassed)
            {
                response(dependencyDefinition.TestResult.ResAs<TRes>());
            }
            else
            {
                using (var cw = new ConsoleWithColor(ConsoleColor.Black, ConsoleColor.Red))
                {
                    cw.Write($"Dependency did not pass {dependencyDefinition.EndPoint.Name}");
                }

                Console.WriteLine();

                throw new Exception("Dependency failed");
            }

            return this;
        }

        public FastPassRequestBuilder DependentOn<TReq, TRes>(Func<FastPassEndpoint, ReqRes<TReq, TRes>> otherCall, Action<ReqRes<TReq, TRes>> response)
        {
            var otherMethod = otherCall.GetMethodInfo();
            var otherClass = otherMethod.DeclaringType;
            TestDefinition currentDefinition = GlobalResults.Tests[Endpoint.SessionId][Endpoint.TestId];
            TestDefinition dependencyDefinition = GlobalResults.Tests[Endpoint.SessionId][$"{otherClass.Name}:{otherMethod.Name}"];
            string dependencyKey = currentDefinition.Key + "-" + dependencyDefinition.Key;
            string currentKey = dependencyDefinition.Key + "-" + currentDefinition.Key;
            if (_dependencies.Contains(currentKey))
            {
                throw new Exception($"Circular dependency exception. The dependency on ${dependencyDefinition.Key} is circular");
            }
            _dependencies.Add(dependencyKey);
            if (!dependencyDefinition.TestHasBeenRun)
            {
                dependencyDefinition.Execute();
            }

            if (dependencyDefinition.TestResult != null && dependencyDefinition.TestResult.AllTestsPassed)
            {
                response(new ReqRes<TReq, TRes>
                {
                    HttpRequest = dependencyDefinition.TestResult.HttpRequest,
                    HttpResponse = dependencyDefinition.TestResult.HttpResponse,
                    Request = dependencyDefinition.TestResult.ReqAs<TReq>(),
                    Response = dependencyDefinition.TestResult.ResAs<TRes>()
                });
            }
            else
            {
                using (var cw = new ConsoleWithColor(ConsoleColor.Black, ConsoleColor.Red))
                {
                    cw.Write($"Dependency did not pass {dependencyDefinition.EndPoint.Name}");
                }

                Console.WriteLine();

                throw new Exception("Dependency failed");
            }

            return this;
        }



        public FastPassRequestBuilder DependentOn(Action<FastPassEndpoint> otherCall, Action<FastPassResponse> response)
        {
            var otherMethod = otherCall.GetMethodInfo();
            var otherClass = otherMethod.DeclaringType;
            TestDefinition currentDefinition = GlobalResults.Tests[Endpoint.SessionId][Endpoint.TestId];
            TestDefinition dependencyDefinition = GlobalResults.Tests[Endpoint.SessionId][$"{otherClass.Name}:{otherMethod.Name}"];
            string dependencyKey = currentDefinition.Key + "-" + dependencyDefinition.Key;
            string currentKey = dependencyDefinition.Key + "-" + currentDefinition.Key;
            if (_dependencies.Contains(currentKey))
            {
                throw new Exception($"Circular dependency exception. The dependency on ${dependencyDefinition.Key} is circular");
            }
            _dependencies.Add(dependencyKey);
            if (!dependencyDefinition.TestHasBeenRun)
            {
                dependencyDefinition.Execute();
            }

            if (dependencyDefinition.TestResult != null && dependencyDefinition.TestResult.AllTestsPassed)
            {
                response(dependencyDefinition.TestResult);
            }
            else
            {
                using (var cw = new ConsoleWithColor(ConsoleColor.Black, ConsoleColor.Red))
                {
                    cw.Write($"Dependency did not pass {dependencyDefinition.EndPoint.Name}");
                }

                Console.WriteLine();

                throw new Exception("Dependency failed");
            }

            return this;
        }

        private static readonly List<string> _dependencies = new List<string>();
    }
}