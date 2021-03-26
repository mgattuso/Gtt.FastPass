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
            });
            return FastPassTestRunner.RunAllTests(root);
        }
    }

}
