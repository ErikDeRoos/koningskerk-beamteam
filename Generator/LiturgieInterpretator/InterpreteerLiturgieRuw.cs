// Copyright 2019 door Erik de Roos
using ILiturgieDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.String;

namespace Generator.LiturgieInterpretator
{
    public static class LiturgieOptieSettings
    {
        public const string OptieNietVerwerken = "geendb";
        public const string OptieNietTonenInVolgende = "geenvolg";
        public const string OptieNietTonenInOverzicht = "geenlt";
        public const string OptieAlternatieveNaamOverzicht = "altlt";
        public const string OptieAlternatieveNaam = "altnm";
        public const string AlsCommando = "als";
        public static readonly char[] OptieParamScheidingstekens = { ':' };
        public const string AlsBijbeltekst = "bijbeltekst";
    }

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

        public ILiturgieOptiesGebruiker BepaalBasisOptiesTekstinvoer(string invoerTekst, string uitDatabase)
        {
            var returnValue = new LiturgieOpties();
            returnValue.NietVerwerkenViaDatabase = String.IsNullOrWhiteSpace(invoerTekst);
            if (returnValue.NietVerwerkenViaDatabase)
                return returnValue;
            returnValue.AlsBijbeltekst = !string.IsNullOrWhiteSpace(uitDatabase) && uitDatabase == Database.LiturgieDatabaseSettings.DatabaseNameBijbeltekst;
            returnValue.ToonInOverzicht = true;
            returnValue.ToonInVolgende = true;
            return returnValue;
        }

        public string MaakTekstVanOpties(ILiturgieOptiesGebruiker opties)
        {
            if (opties == null)
                return string.Empty;

            var optiesReeks = new StringBuilder();
            if (opties.AlsBijbeltekst)
                optiesReeks
                    .Append(LiturgieOptieSettings.AlsCommando)
                    .Append(LiturgieOptieSettings.OptieParamScheidingstekens.First())
                    .Append(LiturgieOptieSettings.AlsBijbeltekst)
                    .Append(OptieScheidingstekens.First());
            if (opties.NietVerwerkenViaDatabase)
                optiesReeks
                    .Append(LiturgieOptieSettings.OptieNietVerwerken)
                    .Append(OptieScheidingstekens.First());
            if (opties.ToonInOverzicht != null && opties.ToonInOverzicht == false)
                optiesReeks
                    .Append(LiturgieOptieSettings.OptieNietTonenInOverzicht)
                    .Append(OptieScheidingstekens.First());
            if (!opties.ToonInVolgende != null && opties.ToonInVolgende == false)
                optiesReeks
                    .Append(LiturgieOptieSettings.OptieNietTonenInVolgende)
                    .Append(OptieScheidingstekens.First());
            if (!string.IsNullOrWhiteSpace(opties.AlternatieveNaam))
                optiesReeks
                    .Append(LiturgieOptieSettings.OptieAlternatieveNaam)
                    .Append(LiturgieOptieSettings.OptieParamScheidingstekens.First())
                    .Append(opties.AlternatieveNaam)
                    .Append(OptieScheidingstekens.First());
            if (!string.IsNullOrWhiteSpace(opties.AlternatieveNaamOverzicht))
                optiesReeks
                    .Append(LiturgieOptieSettings.OptieAlternatieveNaamOverzicht)
                    .Append(LiturgieOptieSettings.OptieParamScheidingstekens.First())
                    .Append(opties.AlternatieveNaamOverzicht)
                    .Append(OptieScheidingstekens.First());

            if (optiesReeks.Length == 0)
                return string.Empty;
            optiesReeks.Remove(optiesReeks.Length - 1, 1); // laatste ',' verwijderen
            return $"{OptieStart.First()}{optiesReeks}{OptieEinde.First()}";
        }

        public string[] SplitsVoorOpties(string liturgieRegel)
        {
            if (String.IsNullOrWhiteSpace(liturgieRegel))
                return new string[0];
            var startOp = liturgieRegel.IndexOfAny(OptieStart);
            if (startOp < 0)
                return new string[0];
            return new string[]
            {
                liturgieRegel.Substring(0, startOp),
                liturgieRegel.Substring(startOp),
            };
        }

        public ILiturgieOptiesGebruiker BepaalOptiesTekstinvoer(string optiesTekst)
        {
            var heeftOpties = optiesTekst.IndexOfAny(OptieStart) >= 0 && optiesTekst.IndexOfAny(OptieEinde) >= 0;
            var voorOpties = optiesTekst.Split(OptieStart, StringSplitOptions.RemoveEmptyEntries);
            var optiesRuw = heeftOpties && voorOpties.Length >= 1 ? voorOpties.Last().Split(OptieEinde, StringSplitOptions.RemoveEmptyEntries)[0].Trim() : Empty;
            return InterpreteerOpties(optiesRuw);
        }


        private static ILiturgieInterpretatie SplitTekstregel(string invoer)
        {
            var invoerTrimmed = invoer.Trim();
            var voorOpties = invoerTrimmed.Split(OptieStart, StringSplitOptions.RemoveEmptyEntries);
            var optiesRuw = voorOpties.Length > 1 ? voorOpties[1].Split(OptieEinde, StringSplitOptions.RemoveEmptyEntries)[0].Trim() : Empty;
            var opties = InterpreteerOpties(optiesRuw);
            if (opties.AlsBijbeltekst)
                return VerwerkAlsBijbeltekst(voorOpties, opties);
            return VerwerkNormaal(voorOpties, opties);
        }

        private static ILiturgieOptiesGebruiker InterpreteerOpties(string optiesRuw)
        {
            var returnValue = new LiturgieOpties();
            var opties = (!IsNullOrEmpty(optiesRuw) ? optiesRuw : "")
              .Split(OptieScheidingstekens, StringSplitOptions.RemoveEmptyEntries)
              .Select(v => v.Trim())
              .ToList();
            returnValue.AlsBijbeltekst = (GetOptieParam(opties, LiturgieOptieSettings.AlsCommando) ?? "").Trim().Equals(LiturgieOptieSettings.AlsBijbeltekst, StringComparison.CurrentCultureIgnoreCase);
            returnValue.NietVerwerkenViaDatabase = opties.Any(o => o.StartsWith(LiturgieOptieSettings.OptieNietVerwerken, StringComparison.CurrentCultureIgnoreCase));
            returnValue.ToonInVolgende = opties.Any(o => o.StartsWith(LiturgieOptieSettings.OptieNietTonenInVolgende, StringComparison.CurrentCultureIgnoreCase)) ? (bool?)false : null;
            returnValue.ToonInOverzicht = opties.Any(o => o.StartsWith(LiturgieOptieSettings.OptieNietTonenInOverzicht, StringComparison.CurrentCultureIgnoreCase)) ? (bool?)false : null;
            returnValue.AlternatieveNaamOverzicht = GetOptieParam(opties, LiturgieOptieSettings.OptieAlternatieveNaamOverzicht);
            returnValue.AlternatieveNaam = GetOptieParam(opties, LiturgieOptieSettings.OptieAlternatieveNaam);
            return returnValue;
        }

        private static string GetOptieParam(IEnumerable<string> opties, string optie)
        {
            if (opties == null || !opties.Any())
                return null;
            return opties.Select(o => o.Split(LiturgieOptieSettings.OptieParamScheidingstekens))
                .Where(o => o.Length == 2 && o[0].Trim().Equals(optie, StringComparison.CurrentCultureIgnoreCase))
                .Select(o => o[1])
                .FirstOrDefault();
        }

        private static ILiturgieInterpretatie VerwerkNormaal(string[] voorOpties, ILiturgieOptiesGebruiker opties)
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
            regel.OptiesGebruiker = opties;
            return regel;
        }

        private static ILiturgieInterpretatieBijbeltekst VerwerkAlsBijbeltekst(string[] voorOpties, ILiturgieOptiesGebruiker opties)
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

            // downward compatibility met ILiturgieInterpretatie
            regel.Deel = deelVersen.FirstOrDefault().Deel;
            regel.VerzenZoalsIngevoerd = deelVersen.FirstOrDefault().VerzenZoalsIngevoerd;
            regel.Verzen = deelVersen.FirstOrDefault().Verzen;
                
            // bijbeltekst visualisatie handmatig regelen
            regel.TeTonenNaamOpOverzicht = voorOpties[0];
            regel.TeTonenNaam = voorOpties[0];

            // opties toekennen
            regel.OptiesGebruiker = opties;
            return regel;
        }


        private class InterpretatieNormaal : ILiturgieInterpretatie
        {
            public static readonly InterpretatieNormaal Empty = new InterpretatieNormaal();

            public string Benaming { get; set; }
            public string Deel { get; set; }

            public string TeTonenNaam { get; set; }
            public string TeTonenNaamOpOverzicht { get; set; }

            public ILiturgieOptiesGebruiker OptiesGebruiker { get; set; }
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

        private class LiturgieOpties : ILiturgieOptiesGebruiker
        {
            public bool NietVerwerkenViaDatabase { get; set; }
            public bool? ToonInOverzicht { get; set; }
            public bool? ToonInVolgende { get; set; }
            public bool AlsBijbeltekst { get; set; }
            public string AlternatieveNaamOverzicht { get; set; }
            public string AlternatieveNaam { get; set; }
        }
    }
}
