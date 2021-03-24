using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
    }
}