using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.FastPass.Sample.Models
{
    public class DeckResponse
    {
        public bool Success { get; set; }
        public string Deck_id { get; set; }
        public bool Shuffled { get; set; }
        public int Remaining { get; set; }
    }
}
