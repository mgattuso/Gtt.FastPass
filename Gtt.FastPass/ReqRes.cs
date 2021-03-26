namespace Gtt.FastPass
{
    public class ReqRes<TReq, TRes>
    {
        public TReq Request { get; set; }
        public TRes Response { get; set; }
    }
}