using System;
using System.Collections.Generic;
using System.Linq;

namespace Gtt.FastPass
{
    public class FastPassEndpoint
    {
        public string TestId { get; private set; }
        private string _protocol;
        private string _host;
        private int _port;
        private string _query;
        private readonly List<string> _paths = new List<string>();
        public TestOptions Options { get; internal set; } = new TestOptions();
        public string Name { get; internal set; }
        public bool SkipTest { get; internal set; }

        public FastPassEndpoint(string root, Action<TestOptions> opts = null)
        {
            opts?.Invoke(Options);

            if (!string.IsNullOrWhiteSpace(root))
            {
                Endpoint(root);
            }
        }

        public FastPassEndpoint WithTestIdentifier(string testId)
        {
            TestId = testId;
            return this;
        }


        public FastPassRequestBuilder Endpoint(string url = null, bool resetPath = false)
        {
            if (resetPath)
            {
                _paths.Clear();
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                return new FastPassRequestBuilder(this, Options);
            }

            try
            {
                var uri = new Uri(url);
                if (uri.IsAbsoluteUri)
                {
                    _protocol = uri.Scheme;
                    _host = uri.Host;
                    _port = uri.Port;
                    _query = uri.Query;
                    _paths.Add(TrimSlashes(uri.LocalPath));
                }
            }
            catch (UriFormatException)
            {
                string[] segments = url.Split('?');
                if (segments.Length > 1)
                {
                    _query = "?" + segments[1];
                }

                _paths.Add(TrimSlashes(segments[0]));
            }

            return new FastPassRequestBuilder(this, Options);
        }

        public FastPassEndpoint Clone(string testId = null)
        {
            var ep = new FastPassEndpoint(BuildUrl()) { Options = Options };
            ep.WithTestIdentifier(testId ?? TestId);
            ep.Name = !string.IsNullOrWhiteSpace(testId) ? testId : ep.Name;
            return ep;
        }

        public string BuildUrl()
        {
            var pathsWithContent = _paths.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            string path = pathsWithContent.Count == 0 ? "" : "/" + string.Join("/", pathsWithContent);
            var url = $"{_protocol}://{_host}:{_port}{path}{_query}";
            return url;
        }

        private string TrimSlashes(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "";

            while (path.EndsWith("/"))
                path = path.Substring(0, path.Length - 1);

            while (path.StartsWith("/"))
                path = path.Substring(1, path.Length - 1);

            return path;
        }
        internal FastPassResponse Response { get; set; }
    }
}