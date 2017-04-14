// Copyright 2017 door Erik de Roos
using ILiturgieDatabase;
using ISlideBuilder;
using mppt.Connect;
using mppt.LiedPresentator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace mppt.RegelVerwerking
{
    abstract class VerwerkBase
    {
        protected IMppApplication _applicatie { get; }
        protected IMppPresentatie _presentatie { get; }
        protected IMppFactory _mppFactory { get; }
        protected IBuilderBuildSettings _buildSettings { get; }
        protected IBuilderBuildDefaults _buildDefaults { get; }
        protected IBuilderDependendFiles _dependentFileList { get; }
        protected IEnumerable<ILiturgieRegel> _liturgie { get; }
        protected ILiedFormatter _liedFormatter { get; }

        protected Regex _tagSearch = new Regex("<[^<>]*>", RegexOptions.Compiled);

        public VerwerkBase(IMppApplication metApplicatie, IMppPresentatie toevoegenAanPresentatie, IMppFactory metFactory, ILiedFormatter gebruikLiedFormatter, IBuilderBuildSettings buildSettings,
                IBuilderBuildDefaults buildDefaults, IBuilderDependendFiles dependentFileList, IEnumerable<ILiturgieRegel> volledigeLiturgieOpVolgorde)
        {
            _applicatie = metApplicatie;
            _presentatie = toevoegenAanPresentatie;
            _mppFactory = metFactory;
            _buildSettings = buildSettings;
            _buildDefaults = buildDefaults;
            _dependentFileList = dependentFileList;
            _liturgie = volledigeLiturgieOpVolgorde;
            _liedFormatter = gebruikLiedFormatter;
        }

        /// <summary>
        /// Open een presentatie op het meegegeven pad
        /// </summary>
        /// <param name="path">het pad waar de powerpointpresentatie kan worden gevonden</param>
        /// <returns>de powerpoint presentatie</returns>
        protected IMppPresentatie OpenPps(string path)
        {
            //controleer voor het openen van de presentatie op het meegegeven path of de presentatie bestaat
            return File.Exists(path) ? _applicatie.Open(path, metWindow: false) : null;
        }

        /// <summary>
        /// Kijk of er in de tekst tags staan en vervang deze voor inhoud
        /// </summary>
        protected TagReplacementResult ProcessForTagReplacement(string text, ILiturgieRegel regel, Func<string, SearchForTagReplacementResult> preflightSearchForTagReplacement = null, Func<string, SearchForTagReplacementResult> additionalSearchForTagReplacement = null)
        {
            if (string.IsNullOrWhiteSpace(text))
                return TagReplacementResult.NoReplacement;
            var replacements = new List<RegexReplacement>();
            foreach (Match match in _tagSearch.Matches(text))
            {
                var tagSearch = SearchForTagReplacement(match.Value, regel, preflightSearchForTagReplacement, additionalSearchForTagReplacement);
                if (tagSearch.Resolved)
                    replacements.Add(new RegexReplacement() { Index = match.Index, Length = match.Length, NewValue = tagSearch.Value, Number = replacements.Count });
            }
            if (!replacements.Any())
                return TagReplacementResult.NoReplacement;
            var returnValue = new StringBuilder(text);
            foreach(var replacement in replacements.OrderByDescending(r => r.Number))
            {
                returnValue.Remove(replacement.Index, replacement.Length);
                returnValue.Insert(replacement.Index, replacement.NewValue);
            }
            return new TagReplacementResult(returnValue.ToString());
        }

        /// <summary>
        /// 'Standaard' tags waarvoor we hier de in te vullen tekst hebben
        /// </summary>
        public SearchForTagReplacementResult SearchForTagReplacement(string tag, ILiturgieRegel regel, Func<string, SearchForTagReplacementResult> preflightSearchForTagReplacement, Func<string, SearchForTagReplacementResult> additionalSearchForTagReplacement)
        {
            var searchTag = tag.Substring(1, tag.Length - 2).Trim().ToLower();

            if (preflightSearchForTagReplacement != null)
            {
                var preFlight = preflightSearchForTagReplacement(searchTag);
                if (preFlight != null && preFlight.Resolved)
                    return preFlight;
            }

            var returnValue = DefaultSearchForTagReplacement(searchTag, regel);
            if (returnValue != null && returnValue.Resolved)
                return returnValue;

            if (additionalSearchForTagReplacement != null)
            {
                var additionalFlight = additionalSearchForTagReplacement(searchTag);
                if (additionalFlight != null && additionalFlight.Resolved)
                    return additionalFlight;
            }
            return SearchForTagReplacementResult.Unresolved;
        }

        private SearchForTagReplacementResult DefaultSearchForTagReplacement(string tag, ILiturgieRegel regel)
        {
            switch (tag)
            {
                case "voorganger:":
                    //als de template de tekst bevat "Voorganger: " moet daar de Voorgangersnaam achter komen
                    return new SearchForTagReplacementResult(_buildDefaults.LabelVoorganger + _buildSettings.Voorganger);
                case "collecte:":
                    //als de template de tekst bevat "Collecte: " moet daar de collectedoel achter komen
                    return new SearchForTagReplacementResult(_buildDefaults.LabelCollecte + _buildSettings.Collecte1);
                case "1e collecte:":
                    //als de template de tekst bevat "1e Collecte: " moet daar de 1e collecte achter komen
                    return new SearchForTagReplacementResult(_buildDefaults.LabelCollecte1 + _buildSettings.Collecte1);
                case "2e collecte:":
                    //als de template de tekst bevat "2e Collecte: " moet daar de 2e collecte achter komen
                    return new SearchForTagReplacementResult(_buildDefaults.LabelCollecte2 + _buildSettings.Collecte2);
                case "lezen":
                    return new SearchForTagReplacementResult(_buildDefaults.LabelLezen + _buildSettings.Lezen);
                case "tekst":
                    return new SearchForTagReplacementResult(_buildDefaults.LabelTekst + _buildSettings.Tekst);
                case "tekst_onder":
                    return new SearchForTagReplacementResult(_buildSettings.Tekst);
            }

            return SearchForTagReplacementResult.Unresolved;
        }

        public class SearchForTagReplacementResult
        {
            public static readonly SearchForTagReplacementResult Unresolved = new SearchForTagReplacementResult();

            public bool Resolved { get; }
            public string Value { get; }

            private SearchForTagReplacementResult()
            {
                Resolved = false;
            }
            public SearchForTagReplacementResult(string value)
            {
                Resolved = true;
                Value = value;
            }
        }

        public class TagReplacementResult
        {
            public static readonly TagReplacementResult NoReplacement = new TagReplacementResult();

            public bool TagsReplaced { get; set; }
            public string NewValue { get; set; }

            private TagReplacementResult()
            {
                TagsReplaced = false;
            }
            public TagReplacementResult(string value)
            {
                TagsReplaced = true;
                NewValue = value;
            }
        }

        private class RegexReplacement
        {
            public int Number { get; set; }
            public int Index { get; set; }
            public int Length { get; set; }
            public string NewValue { get; set; }
        }
    }
}
