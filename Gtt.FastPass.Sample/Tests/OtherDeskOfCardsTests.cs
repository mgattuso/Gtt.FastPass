using System.Linq;
using System.Net.Http;
using Gtt.FastPass.Sample.Models;

namespace Gtt.FastPass.Sample.Tests
{
    [ApiTestSuite("Other Deck of Cards Tests")]
    public class OtherDeckOfCardsTests
    {
        [WarmUp]
        public void WarmUp(FastPassEndpoint test)
        {
            test.BaseCall()
                .Get("", resetPath: true)
                .AssertStatusCode(200)
                .WriteResults();
        }

        [ApiTest]
        public void DrawACard(FastPassEndpoint test)
        {
            DeckResponse response = null;
            HttpResponseMessage httpResponse = null;
            test
                .BaseCall()
                .DependentOn(ShuffleDeck, x =>
                {
                    httpResponse = x.HttpResponse;
                    response = x.Response;
                })
                .Get($"deck/{response.Deck_id}/draw/?count=2")
                .AssertStatusCode(200)
                .AssertMaxResponseTimeMs(2)
                .WriteResults();
        }

        [ApiTest]
        public ReqRes<string, DeckResponse> ShuffleDeck(FastPassEndpoint test)
        {
            return test
                .BaseCall()
                .Get("deck/new/shuffle/?deck_count=1")
                .AssertStatusCode(200)
                .AssertMaxResponseTimeMs(1000)
                .AssertBody<DeckResponse>("Contains deck_id", x => !string.IsNullOrWhiteSpace(x.Deck_id))
                .WriteResults()
                .ReturnContext<string, DeckResponse>();
        }
    }

    public static class Ext
    {
        public static FastPassRequestBuilder BaseCall(this FastPassEndpoint endpoint)
        {
            return endpoint.Endpoint("api")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("Accepts", "application/json");
        }

    }
}
