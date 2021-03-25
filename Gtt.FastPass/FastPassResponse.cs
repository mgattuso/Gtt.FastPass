using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using Gtt.FastPass.Serializers;

namespace Gtt.FastPass
{
    public class FastPassResponse
    {
        public FastPassRequestBuilder Request { get; }
        public List<TestResult> Results { get; } = new List<TestResult>();
        public bool AllTestsPassed => Results.Count > 0 && Results.All(x => x.Passed);

        public Dictionary<string, string[]> Headers { get; }

        public int StatusCode { get; }
        public string Content { get; }

        public T ContentAs<T>()
        {
            return new JsonObjectSerializer(true).Deserialize<T>(Content).GetAwaiter().GetResult();
        }

        public Version HttpVersion { get; }

        public FastPassResponse(FastPassRequestBuilder requestBuilder, HttpResponseMessage response)
        {
            Request = requestBuilder;
            HttpVersion = response.Version;
            Headers = response.Headers.ToDictionary(x => x.Key, y => y.Value.ToArray());
            StatusCode = (int)response.StatusCode;
            Content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            Request.Endpoint.Response = this;
            WritePayload();
        }

        public FastPassResponse AssertStatusCode(HttpStatusCode code)
        {
            return AssertStatusCode((int)code);
        }

        public FastPassResponse AssertStatusCode(Func<FastPassResponse, int> f)
        {
            try
            {
                int code = f(this);
                bool passed = code == StatusCode;
                AddTestResult(new TestResult
                {
                    Name = "Status Code",
                    Passed = passed,
                    Actual = StatusCode.ToString(),
                    Expected = code.ToString()
                });
            }
            catch (Exception ex)
            {
                AddTestResult(new TestResult
                {
                    Name = "Status Code",
                    Passed = false,
                    Actual = StatusCode.ToString(),
                    Expected = ex.ToString()
                });
            }

            return this;
        }

        public FastPassResponse AssertStatusCode(int code)
        {
            try
            {
                bool passed = code == StatusCode;
                AddTestResult(new TestResult
                {
                    Name = "Status Code",
                    Passed = passed,
                    Actual = StatusCode.ToString(),
                    Expected = code.ToString()
                });
            }
            catch (Exception ex)
            {
                AddTestResult(new TestResult
                {
                    Name = "Status Code",
                    Passed = false,
                    Actual = ex.ToString(),
                    Expected = code.ToString()
                });
            }

            return this;
        }

        public FastPassResponse AssertHeader(string header, bool caseSensitive = false)
        {
            var passes = Headers.ContainsKey(header);
            if (caseSensitive == false && !passes)
            {
                passes = Headers.Select(x => x.Key).Contains(header, StringComparer.OrdinalIgnoreCase);
            }

            AddTestResult(new TestResult
            {
                Passed = passes,
                Name = "Has Header",
                Expected = header,
                Actual = ""
            });

            return this;
        }

        public FastPassResponse AssertHeaderWithValue(string header, string value, bool caseSensitive = false)
        {
            string actualValue = "";
            string actualHeader = "";
            var passes = false;

            if (caseSensitive)
            {
                passes = Headers.ContainsKey(header);
                if (passes)
                {
                    actualHeader = header;
                    var foundValue = Headers[header];
                    passes = foundValue.Contains(value);
                    actualValue = string.Join("; ", foundValue);
                }
            }
            else
            {
                var foundKey = Headers.FirstOrDefault(x => x.Key.Equals(header, StringComparison.InvariantCultureIgnoreCase));
                if (!string.IsNullOrWhiteSpace(foundKey.Key))
                {
                    actualHeader = foundKey.Key;
                    var foundValue = Headers[foundKey.Key];
                    passes = foundValue.Contains(value, StringComparer.InvariantCultureIgnoreCase);
                    actualValue = string.Join("; ", foundValue);
                }
            }

            AddTestResult(new TestResult
            {
                Passed = passes,
                Name = "Has Header with Value",
                Expected = $"{header}: {value}",
                Actual = $"{actualHeader}: {actualValue}"
            });

            return this;
        }

        private void AddTestResult(TestResult result)
        {
            if (result.Passed)
                Interlocked.Increment(ref GlobalResults.PassedTests);
            else
                Interlocked.Increment(ref GlobalResults.FailedTests);

            Results.Add(result);
        }

        public FastPassResponse AssertBody(string name, Func<string, bool> f)
        {
            try
            {
                bool passes = f(Content);
                AddTestResult(new TestResult()
                {
                    Name = "Body " + name,
                    Passed = passes
                });
            }
            catch (Exception ex)
            {
                AddTestResult(new TestResult
                {
                    Name = "Body " + name,
                    Passed = false,
                    Actual = ex.ToString()
                });
            }

            return this;
        }

        public FastPassResponse AssertBody<T>(string name, Func<T, bool> f)
        {
            try
            {
                T obj = new JsonObjectSerializer(true).Deserialize<T>(Content).GetAwaiter().GetResult();
                bool passes = f(obj);
                AddTestResult(new TestResult()
                {
                    Name = "Body " + name,
                    Passed = passes
                });
            }
            catch (Exception ex)
            {
                AddTestResult(new TestResult
                {
                    Name = "Body " + name,
                    Passed = false,
                    Actual = ex.ToString()
                });
            }

            return this;
        }

        public FastPassResponse AssertBody(string name, Func<FastPassResponse, string, bool> f)
        {
            try
            {
                bool passes = f(this, Content);
                AddTestResult(new TestResult
                {
                    Name = name,
                    Passed = passes
                });
            }
            catch (Exception ex)
            {
                AddTestResult(new TestResult()
                {
                    Name = name,
                    Passed = false,
                    Actual = ex.ToString()
                });
            }

            return this;
        }

        public FastPassResponse AssertBody<T>(string name, Func<FastPassResponse, T, bool> f)
        {
            try
            {
                T obj = new JsonObjectSerializer(true).Deserialize<T>(Content).GetAwaiter().GetResult();
                bool passes = f(this, obj);
                AddTestResult(new TestResult
                {
                    Name = name,
                    Passed = passes
                });
            }
            catch (Exception ex)
            {
                AddTestResult(new TestResult()
                {
                    Name = name,
                    Passed = false,
                    Actual = ex.ToString()
                });
            }

            return this;
        }

        private FastPassResponse WritePayload()
        {
            if (!Request.Endpoint.Options.PrintHttpContext)
            {
                return this;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{Request.Method} {Request.Endpoint.BuildUrl()}");
            foreach (var header in Request.Headers)
            {
                sb.AppendLine($"{header.Key}: {string.Join("; ", header.Value)}");
            }

            var prettyContent = new JsonObjectSerializer(true).Pretty(Request.Content);
            sb.AppendLine(prettyContent);

            sb.AppendLine();

            sb.AppendLine($"HTTP/{HttpVersion} {StatusCode} {(HttpStatusCode) StatusCode}");
            foreach (var header in Headers)
            {
                sb.AppendLine($"{header.Key}: {string.Join("; ", header.Value)}");
            }
            var prettyResponse = new JsonObjectSerializer(true).Pretty(Content);
            sb.AppendLine(prettyResponse);
            sb.AppendLine();

            Console.WriteLine(sb.ToString());

            return this;
        }

        public FastPassResponse WriteResults()
        {
            foreach (var result in Results)
            {
                string expected = "";
                string actual = "";
                if (!string.IsNullOrWhiteSpace(result.Expected))
                    expected = $"Expected: {result.Expected}";
                
                if (!string.IsNullOrWhiteSpace(result.Actual))
                    actual = $"Actual: {result.Actual}";

                var current = Console.ForegroundColor;
                if (result.Passed)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("PASS:");
                    Console.ForegroundColor = current;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("FAIL:");
                    Console.ForegroundColor = current;
                }

                Console.WriteLine($"  {result.Name} {expected} {actual}");
            }

            Console.WriteLine();

            return this;
        }
    }
}