using Gtt.FastPass.Sample.Models;

namespace Gtt.FastPass.Sample.Tests
{
    [ApiTestSuite]
    public class DeckOfCardsTests
    {
        [ApiTest]
        public void ShuffleDeck(FastPassEndpoint test)
        {
            test
                .Endpoint("api")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("Accepts", "application/json")
                .Get("deck/new/shuffle/?deck_count=1")
                .AssertStatusCode(200)
                .AssertMaxResponseTimeMs(1000)
                .AssertBody<DeckResponse>("Contains deck_id", x => !string.IsNullOrWhiteSpace(x.Deck_id))
                .WriteResults();
        }

        [ApiTest]
        public void DrawACard(FastPassEndpoint test)
        {
            DeckResponse response = null;
            test
                .Endpoint("api")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("Accepts", "application/json")
                .DependentOn(ShuffleDeck, x =>
                {
                    response = x.ResAs<DeckResponse>();
                })
                .Get($"deck/{response.Deck_id}/draw/?count=2")
                .AssertStatusCode(200)
                .AssertMaxResponseTimeMs(2)
                .WriteResults();
        }

        [WarmUp]
        public void WarmUp(FastPassEndpoint test)
        {
            test.Endpoint("api")
                .WithHeader("Content-Type", "application/json")
                .WithHeader("Accepts", "application/json")
                .Get()
                .AssertStatusCode(200)
                .WriteResults();
        }
    }
}
