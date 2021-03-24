using System;
using Gtt.FastPass.Sample.Flows;

namespace Gtt.FastPass.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var n = new DeckOfCardsTests().ShuffleDeck(new FastPassEndpoint("http://deckofcardsapi.com/api"));
        }
    }
}
