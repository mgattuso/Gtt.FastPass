using System;
using System.Collections.Generic;

namespace Gtt.FastPass
{
    public class FastPassEndpoint
    {
        public string TestId { get; private set; }
        private string protocol;
        private string host;
        private int port;
        private string query;
        private readonly List<string> paths = new List<string>();

        public FastPassEndpoint(string root)
        {
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


        public FastPassRequestBuilder Endpoint(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return new FastPassRequestBuilder(this);
            }

            try
            {
                var uri = new Uri(url);
                if (uri.IsAbsoluteUri)
                {
                    protocol = uri.Scheme;
                    host = uri.Host;
                    port = uri.Port;
                    query = uri.Query;
                    paths.Add(TrimSlashes(uri.LocalPath));
                }
            }
            catch (UriFormatException)
            {
                string[] segments = url.Split('?');
                if (segments.Length > 1)
                {
                    query = "?" + segments[1];
                }

                paths.Add(TrimSlashes(segments[0]));
            }

            return new FastPassRequestBuilder(this);
        }

        public FastPassEndpoint Clone(string testId = null)
        {
            var ep = new FastPassEndpoint(BuildUrl());

            if (!string.IsNullOrWhiteSpace(testId))
                ep.WithTestIdentifier(testId);

            return ep;
        }

        public string BuildUrl()
        {
            string path = paths.Count == 0 ? "" : "/" + string.Join("/", paths);
            return $"{protocol}://{host}:{port}{path}{query}";
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