using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Gtt.FastPass.Sample.Flows;
using Gtt.FastPass.Sample.Models;

namespace Gtt.FastPass.Sample
{
    class Program
    {
        static int Main(string[] args)
        {
            var root = new FastPassEndpoint("http://deckofcardsapi.com:80", opts =>
            {
                opts.PrintHttpContext = true;
                opts.WarnOnResponseTimeFailures = true;
                opts.HttpConnectionTimeoutSeconds = 60 * 20; // 20 mins for local development
            });

            int counter = 0;

            Parallel.For(0, 1, new ParallelOptions
            {
                MaxDegreeOfParallelism = 8
            }, idx =>
            {
                int errors = new FastPassTestRunner<TestModel>(root).RunAllTests();
                Interlocked.Add(ref counter, errors);
            });

            return counter;
        }
    }

}
