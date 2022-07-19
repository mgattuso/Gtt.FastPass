using System.Collections.Generic;
using System.Net.Http;

namespace Gtt.FastPass
{
    public class ReqRes<TReq, TRes>
    {
        public TReq Request { get; set; }
        public TRes Response { get; set; }
        public HttpRequestMessage HttpRequest { get; set; }
        public HttpResponseMessage HttpResponse { get; set; }
    }
}