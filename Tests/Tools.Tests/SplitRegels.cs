using NUnit.Framework;
using System.Linq;

namespace Tools.Tests
{
    public class SplitRegels
    {
        [TestCase("Dit wordt opgeknipt.", new[] { "Dit ", "wordt ", "opgeknipt." })]
        [TestCase("Greedy   en intact doorgegeven.", new[] { "Greedy   ", "en ", "intact ", "doorgegeven." })]
        [TestCase("Zin 1. Zin 2.", new[] { "Zin ", "1. ", "Zin ", "2." })]
        public void KnipInWoorden_Klopt(string regel, string[] verwachtResultaat)
        {
            var opgeknipt = Tools.SplitRegels.KnipInWoorden(regel).ToList();

            Assert.That(opgeknipt.Count, Is.EqualTo(verwachtResultaat.Length));
            verwachtResultaat.Select((r, i) => new { r, i }).ToList()
                .ForEach(r => Assert.That(r.r, Is.EqualTo(verwachtResultaat[r.i])));
        }
    }
}
