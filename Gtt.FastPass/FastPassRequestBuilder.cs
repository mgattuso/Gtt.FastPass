﻿using System;
using System.Collections.Generic;
using System.Linq;
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
        public Dictionary<string, string[]> Headers { get; private set; } = new Dictionary<string, string[]>();
        public string Content { get; private set; }
        public HttpMethod Method { get; set; }

        public FastPassRequestBuilder(FastPassEndpoint endpoint)
        {
            Endpoint = endpoint;
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

        public FastPassResponse Get(string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
                Endpoint.Endpoint(path);

            return Call(HttpMethod.Get);
        }

        public FastPassResponse Post(string path = null)
        {
            if (!string.IsNullOrWhiteSpace(path))
                Endpoint.Endpoint(path);

            return Call(HttpMethod.Post);
        }

        public FastPassResponse Post<T>(string path, T obj)
        {
            WithBody(obj);
            if (!string.IsNullOrWhiteSpace(path))
                Endpoint.Endpoint(path);

            return Call(HttpMethod.Post);
        }

        public FastPassResponse Post<T>(T obj)
        {
            WithBody(obj);
            return Call(HttpMethod.Post);
        }

        public FastPassResponse Put(string path = null)
        {
            if (!string.IsNullOrWhiteSpace(path))
                Endpoint.Endpoint(path);

            return Call(HttpMethod.Put);
        }

        public FastPassResponse Put<T>(string path, T obj)
        {
            WithBody(obj);
            if (!string.IsNullOrWhiteSpace(path))
                Endpoint.Endpoint(path);

            return Call(HttpMethod.Put);
        }

        public FastPassResponse Put<T>(T obj)
        {
            WithBody(obj);
            return Call(HttpMethod.Put);
        }

        public FastPassResponse Delete(string path = null)
        {
            if (!string.IsNullOrWhiteSpace(path))
                Endpoint.Endpoint(path);

            return Call(HttpMethod.Delete);
        }

        public FastPassResponse Delete<T>(string path, T obj)
        {
            WithBody(obj);
            if (!string.IsNullOrWhiteSpace(path))
                Endpoint.Endpoint(path);

            return Call(HttpMethod.Delete);
        }

        public FastPassResponse Delete<T>(T obj)
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

            Console.WriteLine("TEST: " + Endpoint.Name);

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
            var response = Client.SendAsync(msg, HttpCompletionOption.ResponseContentRead, cts.Token).GetAwaiter().GetResult();
            return new FastPassResponse(this, response);
        }

        public FastPassRequestBuilder DependentOnPassingTest(Action<FastPassEndpoint> otherCall, Action<FastPassResponse> response)
        {
            var otherMethod = otherCall.GetMethodInfo();
            var otherClass = otherMethod.DeclaringType;
            TestDefinition currentDefinition = GlobalResults.Tests[Endpoint.TestId];
            TestDefinition dependencyDefinition = GlobalResults.Tests[$"{otherClass.Name}:{otherMethod.Name}"];
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
                var currentBg = Console.BackgroundColor;
                var currentCol = Console.ForegroundColor;
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write($"Dependency did not pass {dependencyDefinition.EndPoint.Name}");
                Console.BackgroundColor = currentBg;
                Console.ForegroundColor = currentCol;
                Console.WriteLine();
                throw new Exception("Dependency failed");
            }

            return this;
        }

        private static readonly List<string> _dependencies = new List<string>();
    }
}