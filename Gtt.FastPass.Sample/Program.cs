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
            var root = new FastPassEndpoint("http://deckofcardsapi.com/api", opts =>
            {
                opts.PrintHttpContext = true;
            });
            return FastPassTestRunner.RunAllTests(root);
        }
    }

}
