// Copyright 2016 door Erik de Roos
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Generator.Tools
{
    public static class SplitRegels
    {
        public static string[] Split(string list)
        {
            if (list == null)
                return new string[0];
            return list.Replace("\r", "").Split(new[] { "\n" }, StringSplitOptions.None);
        }

        public static IEnumerable<string> KnipInWoorden(string regel)
        {
            foreach (Match match in Regex.Matches(regel, "([^ .,!?-]+)([ .,!?-]+)?", RegexOptions.Compiled))
            {
                yield return match.Value;
            }
        }
    }
}
