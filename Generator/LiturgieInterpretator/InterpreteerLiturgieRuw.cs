﻿using ILiturgieDatabase;
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

        private static readonly string AlsCommando = "als";
        private static readonly char[] AlsScheidingstekens = { ':' };
        private static readonly string AlsBijbeltekst = "bijbeltekst";

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

        private static ILiturgieInterpretatie SplitTekstregel(string invoer)
        {
            var invoerTrimmed = invoer.Trim();
            var voorOpties = invoerTrimmed.Split(OptieStart, StringSplitOptions.RemoveEmptyEntries);
            var optiesRuw = voorOpties.Length > 1 ? voorOpties[1].Split(OptieEinde, StringSplitOptions.RemoveEmptyEntries)[0].Trim() : Empty;
            var opties = (!IsNullOrEmpty(optiesRuw) ? optiesRuw : "")
              .Split(OptieScheidingstekens, StringSplitOptions.RemoveEmptyEntries)
              .Select(v => v.Trim())
              .ToList();
            var als = Als(opties);
            if ((als ?? "").Equals(AlsBijbeltekst, StringComparison.CurrentCultureIgnoreCase))
                return VerwerkAlsBijbeltekst(voorOpties, opties);
            return VerwerkNormaal(voorOpties, opties);
        }

        private static string Als(IEnumerable<string> voorOpties)
        {
            if (voorOpties == null || !voorOpties.Any())
                return null;
            return voorOpties.Select(o => o.Split(AlsScheidingstekens))
                .Where(o => o.Length == 2 && o[0].Trim().Equals(AlsCommando, StringComparison.CurrentCultureIgnoreCase))
                .Select(o => o[1])
                .FirstOrDefault();
        }

        private static ILiturgieInterpretatie VerwerkNormaal(string[] voorOpties, IEnumerable<string> opties)
        {
            var regel = new InterpretatieNormaal();
            if (voorOpties.Length == 0)
                return null;
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
            regel.Opties = opties.ToList();
            return regel;
        }

        private static ILiturgieInterpretatieBijbeltekst VerwerkAlsBijbeltekst(string[] voorOpties, IEnumerable<string> opties)
        {
            var regel = new InterpretatieBijbeltekst();
            if (voorOpties.Length == 0)
                return null;
            var voorBenamingStukken = voorOpties[0].Trim().Split(BenamingScheidingstekens, StringSplitOptions.RemoveEmptyEntries);
            if (voorBenamingStukken.Length == 0)
                return null;
            var preBenamingTrimmed = voorBenamingStukken[0].Trim();
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
            regel.Opties = opties.ToList();
            return regel;
        }


        private class InterpretatieNormaal : ILiturgieInterpretatie
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

        private class InterpretatieBijbeltekst : InterpretatieNormaal, ILiturgieInterpretatieBijbeltekst
        {
            public IEnumerable<ILiturgieInterpretatieBijbeltekstDeel> PerDeelVersen { get; set; }
        }
    }
}
