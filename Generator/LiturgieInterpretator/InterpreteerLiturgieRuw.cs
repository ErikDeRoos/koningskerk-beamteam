// Copyright 2016 door Erik de Roos
using ILiturgieDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.String;

namespace Generator.LiturgieInterpretator
{
    /// <summary>
    /// Maak een ruwe lijst van een tekstuele liturgie
    /// </summary>
    public class InterpreteerLiturgieRuw : ILiturgieInterpreteer
    {
        private static readonly char[] BenamingScheidingstekens = { ':' };
        private static readonly char[] BenamingDeelScheidingstekens = { ' ' };
        private static readonly char[] VersScheidingstekens = { ',' };
        private static readonly char[] OptieStart = { '(' };
        private static readonly char[] OptieEinde = { ')' };
        private static readonly char[] OptieScheidingstekens = { ',' };

        private static Interpretatie SplitTekstregel(string invoer)
        {
            var regel = new Interpretatie();
            var invoerTrimmed = invoer.Trim();
            var voorOpties = invoerTrimmed.Split(OptieStart, StringSplitOptions.RemoveEmptyEntries);
            if (voorOpties.Length == 0)
                return null;
            var opties = voorOpties.Length > 1 ? voorOpties[1].Split(OptieEinde, StringSplitOptions.RemoveEmptyEntries)[0].Trim() : Empty;
            var voorBenamingStukken = voorOpties[0].Trim().Split(BenamingScheidingstekens, StringSplitOptions.RemoveEmptyEntries);
            if (voorBenamingStukken.Length == 0)
                return null;
            var preBenamingTrimmed = voorBenamingStukken[0].Trim();
            // Een benaming kan uit delen bestaan, bijvoorbeeld 'psalm 110' in 'psalm 110:1,2' of 'opwekking 598' in 'opwekking 598'
            var voorPreBenamingStukken = preBenamingTrimmed.Split(BenamingDeelScheidingstekens, StringSplitOptions.RemoveEmptyEntries);
            if (voorPreBenamingStukken.Length > 1)
                regel.Deel = voorPreBenamingStukken[voorPreBenamingStukken.Length - 1];  // Is altijd laatste deel
            regel.Benaming = preBenamingTrimmed.Substring(0, preBenamingTrimmed.Length - (regel.Deel ?? "").Length).Trim();
            // Verzen als '1,2' in 'psalm 110:1,2'
            regel.VerzenZoalsIngevoerd = voorBenamingStukken.Length > 1 ? voorBenamingStukken[1].Trim() : null;
            regel.Verzen = (regel.VerzenZoalsIngevoerd ?? "")
              .Split(VersScheidingstekens, StringSplitOptions.RemoveEmptyEntries)
              .Select(v => v.Trim())
              .ToList();
            regel.Opties = (!IsNullOrEmpty(opties) ? opties : "")
              .Split(OptieScheidingstekens, StringSplitOptions.RemoveEmptyEntries)
              .Select(v => v.Trim())
              .ToList();
            return regel;
        }

        public ILiturgieInterpretatie VanTekstregel(string regels)
        {
            return SplitTekstregel(regels);
        }

        /// <summary>
        /// Leest de tekstuele invoer in en maakt er een ruwe liturgie lijst van
        /// </summary>
        public IEnumerable<ILiturgieInterpretatie> VanTekstregels(string[] regels)
        {
            return regels
              .Where(r => !IsNullOrWhiteSpace(r))
              .Select(VanTekstregel)
              .Where(r => r != null)
              .ToList();
        }

        private class Interpretatie : ILiturgieInterpretatie
        {
            public string Benaming { get; set; }
            public string Deel { get; set; }

            public IEnumerable<string> Opties { get; set; }
            public IEnumerable<string> Verzen { get; set; }
            public string VerzenZoalsIngevoerd { get; set; }

            public override string ToString()
            {
                return $"{Benaming} {Deel} {VerzenZoalsIngevoerd}";
            }
        }
    }
}
