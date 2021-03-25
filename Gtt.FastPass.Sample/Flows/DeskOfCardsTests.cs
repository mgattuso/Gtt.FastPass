using System;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Gtt.FastPass.Attributes;
using Gtt.FastPass.Sample.Models;

namespace Gtt.FastPass.Sample.Flows
{
    [ApiTestSuite("Deck of Cards Tests")]
    public class DeckOfCardsTests
    {
        [ApiTest]
        public void DrawACard(FastPassEndpoint test)
        {
            DeckResponse response = null;
            test
                .BaseCall()
                .DependentOn(ShuffleDeck, x => response = x)
                .Get($"deck/{response.Deck_id}/draw/?count=2")
                .AssertStatusCode(200)
                .AssertMaxResponseTimeMs(1000)
                .WriteResults();
        }

        [ApiTest]
        public DeckResponse ShuffleDeck(FastPassEndpoint test)
        {
            return test
                .BaseCall()
                .Get("deck/new/shuffle/?deck_count=1")
                .AssertStatusCode(200)
                .AssertMaxResponseTimeMs(1000)
                .AssertBody<DeckResponse>("Contains deck_id", x => !string.IsNullOrWhiteSpace(x.Deck_id))
                .WriteResults()
                .ReturnBody<DeckResponse>();
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
