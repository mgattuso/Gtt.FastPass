using System;
using Gtt.FastPass.Sample.Flows;

namespace Gtt.FastPass.Sample
{
    class Program
    {
        static int Main(string[] args)
        {
            var n = new DeckOfCardsTests().ShuffleDeck(new FastPassEndpoint("http://deckofcardsapi.com/api"));


            return GlobalResults.FailedTests > 0 ? -1 : 0;
        }
    }
}
