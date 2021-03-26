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
        public long ResponseTime { get; }
        public List<TestResult> Results { get; } = new List<TestResult>();
        public bool AllTestsPassed => Results.Count > 0 && Results.All(x => x.Passed);

        public Dictionary<string, string[]> Headers { get; }

        public int StatusCode { get; }
        public string Content { get; }

        public T ResAs<T>()
        {
            if (string.IsNullOrWhiteSpace(Content))
            {
                return default(T);
            }
            return new JsonObjectSerializer(true).Deserialize<T>(Content).GetAwaiter().GetResult();
        }

        public T ReqAs<T>()
        {
            if (string.IsNullOrWhiteSpace(Request.Content))
            {
                return default(T);
            }
            return new JsonObjectSerializer(true).Deserialize<T>(Request.Content).GetAwaiter().GetResult();
        }

        public Version HttpVersion { get; }

        public FastPassResponse(FastPassRequestBuilder requestBuilder, HttpResponseMessage response, long responseTime)
        {
            Request = requestBuilder;
            ResponseTime = responseTime;
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
            if (Results.Any(x => !x.Passed)) return this;

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
            if (Results.Any(x => !x.Passed)) return this;

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
            if (Results.Any(x => !x.Passed)) return this;

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
            if (Results.Any(x => !x.Passed)) return this;

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
            if (Results.Any(x => !x.Passed)) return this;

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
            if (Results.Any(x => !x.Passed)) return this;

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
            if (Results.Any(x => !x.Passed)) return this;

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

            using (var cw = new ConsoleWithColor(ConsoleColor.Magenta))
            {
                cw.WriteLine($"{Request.Method} {Request.Endpoint.BuildUrl()}");
            }

            using (var cw = new ConsoleWithColor(ConsoleColor.DarkGray))
            {
                foreach (var header in Request.Headers)
                {
                    cw.Write(header.Key + ": ");
                    cw.Write(string.Join("; ", header.Value), ConsoleColor.White);
                    cw.WriteLine();
                }
            }

            var prettyContent = new JsonObjectSerializer(true).Pretty(Request.Content);
            Console.WriteLine(prettyContent);

            using (var cw = new ConsoleWithColor(ConsoleColor.DarkCyan))
            {
                cw.WriteLine($"HTTP/{HttpVersion} {StatusCode} {(HttpStatusCode)StatusCode}");
            }

            using (var cw = new ConsoleWithColor(ConsoleColor.DarkGray))
            {
                foreach (var header in Headers)
                {
                    cw.Write(header.Key + ": ");
                    cw.Write(string.Join("; ", header.Value), ConsoleColor.White);
                    cw.WriteLine();
                }
            }

            var prettyResponse = new JsonObjectSerializer(true).Pretty(Content);
            Console.WriteLine(prettyResponse);
            Console.WriteLine();

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
                    using (var cw = new ConsoleWithColor(ConsoleColor.Green))
                    {
                        cw.Write("PASS:");
                    }
                }
                else
                {
                    using (var cw = new ConsoleWithColor(ConsoleColor.Red))
                    {
                        cw.Write("FAIL:");
                    }
                }

                Console.WriteLine($"  {result.Name} {expected} {actual}");
            }

            Console.WriteLine();

            return this;
        }

        public T ReturnResponse<T>()
        {
            return ResAs<T>();
        }

        public ReqRes<TRequest, TResponse> ReturnContext<TRequest, TResponse>()
        {
            return new ReqRes<TRequest, TResponse>
            {
                Request = ReqAs<TRequest>(),
                Response = ResAs<TResponse>()
            };
        }

        public FastPassResponse AssertMaxResponseTimeMs(long responseTime)
        {
            AddTestResult(new TestResult
            {
                Name = "Max Response time",
                Passed = ResponseTime <= responseTime,
                Actual = ResponseTime + "ms",
                Expected = responseTime + "ms"
            });

            return this;
        }
    }
}