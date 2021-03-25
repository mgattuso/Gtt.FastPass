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
        public void ShuffleDeck(FastPassEndpoint test)
        {
            test
                .Endpoint("deck/new/shuffle/?deck_count=1")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("Accepts", "application/json")
                .DependentOnPassingTest(GetNewDeck, x =>
                {
                    Console.WriteLine(x.Content);
                })
                .Get()
                .AssertStatusCode(200)
                .AssertHeader("Server")
                .AssertHeaderWithValue("CF-Cache-Status", "dynamic")
                .AssertBody("Contains deck_id", x => x.Contains("deck_id"))
                .WriteResults();
        }

        [ApiTest]
        public void GetNewDeck(FastPassEndpoint test)
        {
            test
                .Endpoint("deck/new/shuffle/?deck_count=1")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("Accepts", "application/json")
                .Get()
                .AssertStatusCode(201)
                .AssertHeader("Server")
                .AssertHeaderWithValue("CF-Cache-Status", "dynamic")
                .AssertBody("Contains deck_id", x => x.Contains("deck_id"))
                .WriteResults();
        }
    }
}
