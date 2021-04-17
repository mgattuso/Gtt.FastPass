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
            bool consoleMode = false;
            if (args != null && args.Length > 0)
            {
                consoleMode = args[0].Equals("console", StringComparison.InvariantCultureIgnoreCase);
            }

            var root = new FastPassEndpoint("http://deckofcardsapi.com:80", opts =>
            {
                opts.PrintHttpContext = true;
                opts.WarnOnResponseTimeFailures = true;
                opts.HttpConnectionTimeoutSeconds = 60 * 20; // 20 mins for local development
            });

            if (consoleMode)
            {
                return new FastPassTestRunner<TestModel>(root).RunAllTests();

            }

            new GuiRunner<TestModel>(root).Run();
            return 0;
        }
    }

}
