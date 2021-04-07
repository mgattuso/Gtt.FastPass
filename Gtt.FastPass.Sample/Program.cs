using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gtt.FastPass.Sample.Flows;

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
            var tests = GlobalResults.Tests;
            var t1 = new FastPassTestRunner().RunAllTests(root);
            var t2 = new FastPassTestRunner().RunAllTests(root);
            return t1 + t2;
        }
    }

}
