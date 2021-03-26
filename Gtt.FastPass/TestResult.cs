using System.Linq;

namespace Gtt.FastPass
{
    public class TestResult
    {
        public string Name { get; set; }
        public string Expected { get; set; }
        public string Actual { get; set; }
        public bool Passed => new[] { ResultLabel.Pass, ResultLabel.Skip, ResultLabel.Warn }.Contains(Label);
        public ResultLabel Label { get; set; }
    }
}