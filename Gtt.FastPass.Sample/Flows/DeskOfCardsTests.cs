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
                .DependentOn<DeckResponse>(ShuffleDeck, x => response = x)
                .Get($"deck/{response.Deck_id}/draw/?count=2")
                .AssertStatusCode(200)
                .WriteResults();
        }

        [ApiTest]
        public void ShuffleDeck(FastPassEndpoint test)
        {
            test
                .BaseCall()
                .Get("deck/new/shuffle/?deck_count=1")
                .AssertStatusCode(200)
                .AssertHeader("Server")
                .AssertHeaderWithValue("CF-Cache-Status", "dynamic")
                .AssertBody("Contains deck_id", x => x.Contains("deck_id"))
                .WriteResults();
        }


    }

    public static class Ext
    {
        public static FastPassRequestBuilder BaseCall(this FastPassEndpoint endpoint)
        {
            return endpoint.Endpoint("api")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("Accepts", "application/json").Clone();
        }

    }
}
