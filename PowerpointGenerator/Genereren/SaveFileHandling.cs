// Copyright 2017 door Remco Veurink
using Generator.Tools;
using System.Collections.Generic;
using System.Linq;

namespace PowerpointGenerator.Genereren
{
    public class SaveFileHandling
    {
        public string[] Liturgie { get; set; }
        public string Voorganger { get; set; }
        public string Collecte1e { get; set; }
        public string Collecte2e { get; set; }
        public string[] Lezen { get; set; }
        public string[] Tekst { get; set; }

        public static SaveFileHandling LoadFromWorkingFile(string input)
        {
            var output = new SaveFileHandling();
            if (input.Equals(""))
                return output;

            var inputstring = SplitRegels.Split(input);
            var liturgieLijst = new List<string>();
            var i = 0;
            for (; i < inputstring.Length; i++)
            {
                if (inputstring[i].StartsWith("<"))
                    break;
                if (!inputstring[i].Equals(""))
                    liturgieLijst.Add(inputstring[i]);
            }
            output.Liturgie = liturgieLijst.ToArray();
            for (; i < inputstring.Length; i++)
            {
                if (inputstring[i].Equals("")) continue;
                var inputstringparts = inputstring[i].Split('<', '>');
                switch (inputstringparts[1])
                {
                    case "Voorganger:":
                        output.Voorganger = inputstringparts[2];
                        break;
                    case "1e Collecte:":
                        output.Collecte1e = inputstringparts[2];
                        break;
                    case "2e Collecte:":
                        output.Collecte2e = inputstringparts[2];
                        break;
                    case "Lezen":
                        var lezenLijst = new List<string>();
                        for (var j = 2; j < inputstringparts.Length; j += 2)
                        {
                            lezenLijst.Add(inputstringparts[j]);
                        }
                        output.Lezen = lezenLijst.ToArray();
                        break;
                    case "Tekst":
                        var tekstLijst = new List<string>();
                        for (var j = 2; j < inputstringparts.Length; j += 2)
                        {
                            tekstLijst.Add(inputstringparts[j]);
                        }
                        output.Tekst = tekstLijst.ToArray();
                        break;
                }
            }

            return output;
        }
        public static string CreateWorkingFile(SaveFileHandling liturgie)
        {
            var output = string.Join("\n", liturgie.Liturgie) + "\n";
            output += "<Voorganger:>" + liturgie.Voorganger + "\n";
            output += "<1e Collecte:>" + liturgie.Collecte1e + "\n";
            output += "<2e Collecte:>" + liturgie.Collecte2e + "\n";

            output += "<Lezen>";
            var regels = liturgie.Lezen.ToList();
            for (var i = 0; i < regels.Count; i++)
            {
                if (regels[i].Equals("")) continue;
                if (i + 1 < regels.Count)
                    output += regels[i] + "<n>";
                else
                    output += regels[i];
            }
            output += "\n";

            output += "<Tekst>";
            regels = liturgie.Tekst.ToList();
            for (var i = 0; i < regels.Count; i++)
            {
                if (regels[i].Equals("")) continue;
                if (i + 1 < regels.Count)
                    output += regels[i] + "<n>";
                else
                    output += regels[i];
            }
            output += "\n";

            return output;
        }
    }
}
