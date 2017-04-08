// Copyright 2017 door Erik de Roos
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
        public static readonly char[] BenamingScheidingstekens = { ':' };
        public static readonly char[] BenamingDeelScheidingstekens = { ' ' };
        private static readonly char[] VersScheidingstekens = { ',' };
        private static readonly char[] VersKoppeltekens = { '-' };
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
            if ((als ?? "").Trim().Equals(AlsBijbeltekst, StringComparison.CurrentCultureIgnoreCase))
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
                return InterpretatieNormaal.Empty;
            var voorBenamingStukken = voorOpties[0].Trim().Split(BenamingScheidingstekens, StringSplitOptions.RemoveEmptyEntries);
            if (voorBenamingStukken.Length == 0)
                return InterpretatieNormaal.Empty;
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
                return InterpretatieBijbeltekst.Empty;
            var benamingStukken = voorOpties[0].Trim().Split(BenamingScheidingstekens, StringSplitOptions.RemoveEmptyEntries);
            if (benamingStukken.Length == 0)
                return InterpretatieBijbeltekst.Empty;
            // Opknippen zodat hoofdstukken bij verzen blijven
            // Opgeknipt moet 'johannes 3: 5, 7, 9 - 8:1, 3, 9: 5 - 10' geven: 'johannes', '3: 5, 7, 9 -', '8:1, 3, ', '9: 5 - 10'
            // Opknippen ook zodat koppeltekens doorgegeven worden
            // Opgeknipt moet 'johannes 3: 5 - 8:10' geven: 'johannes', '3: 5 -', '8: - 10'
            var onthouden = string.Empty;
            var onthoudenVoorLaatsteElementIsKoppelteken = false;
            var voorBenaming = string.Empty;
            var deelVersen = new List<ILiturgieInterpretatieBijbeltekstDeel>();
            for (int teller = 0; teller < benamingStukken.Length; teller++)
            {
                var stuk = benamingStukken[teller].Trim();
                if (onthoudenVoorLaatsteElementIsKoppelteken)
                    stuk = $"{VersKoppeltekens[0]} {stuk}";
                var heeftVolgendStuk = teller + 1 < benamingStukken.Length;
                if (!heeftVolgendStuk && teller != 0)
                {
                    deelVersen.Add(new InterpretatieBijbeltekstDeel() {
                        Deel = onthouden,
                        VerzenZoalsIngevoerd = stuk,
                        Verzen = (stuk ?? "")
                          .Split(VersScheidingstekens, StringSplitOptions.RemoveEmptyEntries)
                          .Select(v => v.Trim())
                          .ToList()
                    });
                    break;
                }

                var elementen = teller == 0 ?
                    stuk.Split(BenamingDeelScheidingstekens, StringSplitOptions.RemoveEmptyEntries)
                    : stuk.Replace("-", " - ").Split(BenamingDeelScheidingstekens.Union(VersScheidingstekens).ToArray(), StringSplitOptions.RemoveEmptyEntries);
                var laatsteElement = string.Empty;
                if (elementen.Length >= 2)
                    laatsteElement = elementen[elementen.Length - 1];
                var voorLaatsteElementIsKoppelteken = elementen.Length >= 3 && VersKoppeltekens.Contains(elementen[elementen.Length - 2][0]);
                var stukZonderLaatsteElement = stuk.Substring(0, stuk.Length - laatsteElement.Length).Trim();
                if (teller == 0)
                    voorBenaming = stukZonderLaatsteElement;
                if (!heeftVolgendStuk && teller == 0)
                    deelVersen.Add(new InterpretatieBijbeltekstDeel() {
                        Deel = laatsteElement,
                        Verzen = new List<string>()
                    });
                else if (teller != 0)
                    deelVersen.Add(new InterpretatieBijbeltekstDeel() {
                        Deel = onthouden,
                        VerzenZoalsIngevoerd = stukZonderLaatsteElement,
                        Verzen = (stukZonderLaatsteElement ?? "")
                          .Split(VersScheidingstekens, StringSplitOptions.RemoveEmptyEntries)
                          .Select(v => v.Trim())
                          .ToList()
                    });
                onthouden = laatsteElement;
                onthoudenVoorLaatsteElementIsKoppelteken = voorLaatsteElementIsKoppelteken;
            }
            regel.PerDeelVersen = deelVersen;
            regel.Benaming = voorBenaming;
            var optieLijst = opties.ToList();

            // downward compatibility met ILiturgieInterpretatie
            regel.Deel = deelVersen.FirstOrDefault().Deel;
            regel.VerzenZoalsIngevoerd = deelVersen.FirstOrDefault().VerzenZoalsIngevoerd;
            regel.Verzen = deelVersen.FirstOrDefault().Verzen;

            // visualisatie handmatig regelen
            optieLijst.Add($"{LiturgieOplosser.LiturgieOplosserSettings.OptieAlternatieveNaamOverzicht} {voorOpties[0]}");
            optieLijst.Add($"{LiturgieOplosser.LiturgieOplosserSettings.OptieAlternatieveNaam} {voorOpties[0]}");

            // opties toekennen
            regel.Opties = optieLijst;
            return regel;
        }


        private class InterpretatieNormaal : ILiturgieInterpretatie
        {
            public static readonly InterpretatieNormaal Empty = new InterpretatieNormaal();

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
            new public static readonly InterpretatieBijbeltekst Empty = new InterpretatieBijbeltekst();

            public IEnumerable<ILiturgieInterpretatieBijbeltekstDeel> PerDeelVersen { get; set; }

            public override string ToString()
            {
                return $"{Benaming} {string.Join(", ", PerDeelVersen)}";
            }
        }

        private class InterpretatieBijbeltekstDeel : ILiturgieInterpretatieBijbeltekstDeel
        {
            public string Deel { get; set; }
            public IEnumerable<string> Verzen { get; set; }
            public string VerzenZoalsIngevoerd { get; set; }

            public override string ToString()
            {
                if (!string.IsNullOrWhiteSpace(VerzenZoalsIngevoerd))
                    return $"{Deel}: {VerzenZoalsIngevoerd}";
                return Deel;
            }
        }
    }
}
