// Copyright 2018 door Erik de Roos
using Generator.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Generator.Tests.Tools
{
    public class SplitRegelsTests
    {
        [TestInitialize]
        public void Initialise()
        {
        }

        [TestClass]
        public class KnipInWoordenMethod : SplitRegelsTests
        {
            [DataTestMethod]
            [DataRow("Dit wordt opgeknipt.", "", new[] { "Dit ", "wordt ", "opgeknipt." })]
            [DataRow("Greedy   en intact doorgegeven.", "", new[] { "Greedy   ", "en ", "intact ", "doorgegeven." })]
            [DataRow("Zin 1. Zin 2.", "", new[] { "Zin ", "1. ", "Zin ", "2." })]
            public void Klopt(string regel, string empty, string[] verwachtResultaat)
            {
                var opgeknipt = SplitRegels.KnipInWoorden(regel).ToList();

                Assert.AreEqual(opgeknipt.Count, verwachtResultaat.Length);
                verwachtResultaat.Select((r, i) => new { r, i }).ToList()
                    .ForEach(r => Assert.AreEqual(r.r, verwachtResultaat[r.i]));
            }
        }
    }
}
