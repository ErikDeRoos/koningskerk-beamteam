// Copyright 2017 door Remco Veurink en Erik de Roos
using Generator.Database;
using Generator.LiturgieInterpretator;
using Generator.Powerpoint;
using ILiturgieDatabase;
using ISettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tools;

namespace Generator
{
    public class GeneratieInterface<T> where T : class, ICompRegistration
    {
        private readonly ILiturgieLosOp _liturgieOplosser;
        private readonly ILiturgieInterpreteer _liturgieInterpreteer;
        private readonly IInstellingenFactory _instellingenFactory;
        private readonly Func<ISlideBuilder.IBuilder> _builderResolver;
        public T Registration { get; }
        public string TempLiturgiePath { get; set; }
        public string CurrentFile { get; private set; }

        //generator
        private PpGenerator _powerpoint;
        public GeneratorStatus Status { get; private set; }
        public delegate void Voortgang(int lijstStart, int lijstEind, int bijItem);
        private Voortgang _setVoortgang;
        public delegate void GereedMelding(string opgeslagenAlsBestand = null, string foutmelding = null, int? slidesGemist = null);
        private GereedMelding _setGereedmelding;


        public GeneratieInterface(ILiturgieLosOp liturgieOplosser, IInstellingenFactory instellingenOplosser, ILiturgieInterpreteer liturgieInterpreteer, Func<ISlideBuilder.IBuilder> builderResolver, ICompRegistration newCompRegistration)
        {
            _liturgieOplosser = liturgieOplosser;
            _liturgieInterpreteer = liturgieInterpreteer;
            _instellingenFactory = instellingenOplosser;
            _builderResolver = builderResolver;
            _powerpoint = new PpGenerator(_builderResolver, PresentatieVoortgangCallback, PresentatieGereedmeldingCallback);
            Registration = newCompRegistration as T;
        }

        public void Opstarten(string startBestand)
        {
            if (!string.IsNullOrEmpty(startBestand) && File.Exists(startBestand))
            {
                try
                {
                    LoadWorkingfile(OpenenOpLocatie(startBestand));
                }
                catch { }
            }
            else if (File.Exists(TempLiturgiePath))
            {
                try
                {
                    LoadWorkingfile(OpenenOpLocatie(TempLiturgiePath));
                }
                catch { }
                try
                {
                    File.Delete(TempLiturgiePath);
                }
                catch { }
            }
        }

        public void RegisterVoortgang(Voortgang callback)
        {
            _setVoortgang = callback;
        }

        public void RegisterGereedmelding(GereedMelding callback)
        {
            _setGereedmelding = callback;
        }

        private void PresentatieVoortgangCallback(int lijstStart, int lijstEind, int bijItem)
        {
            if (Status == GeneratorStatus.Gestopt)
                return;
            _setVoortgang?.Invoke(lijstStart, lijstEind, bijItem);
        }
        private void PresentatieGereedmeldingCallback(string opgeslagenAlsBestand = null, string foutmelding = null, int? slidesGemist = null)
        {
            if (Status == GeneratorStatus.Gestopt)
                return;
            Status = GeneratorStatus.Gestopt;
            if (string.IsNullOrEmpty(foutmelding))
            {
                if (string.IsNullOrEmpty(opgeslagenAlsBestand)) return;
                if (File.Exists(TempLiturgiePath))
                    File.Delete(TempLiturgiePath);
            }
            _setGereedmelding?.Invoke(opgeslagenAlsBestand, foutmelding, slidesGemist);
        }

        public void LoadWorkingfile(string input)
        {
            if (input.Equals(""))
                return;
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
            Registration.Liturgie = liturgieLijst.ToArray();
            for (; i < inputstring.Length; i++)
            {
                if (inputstring[i].Equals("")) continue;
                var inputstringparts = inputstring[i].Split('<', '>');
                switch (inputstringparts[1])
                {
                    case "Voorganger:":
                        Registration.Voorganger = inputstringparts[2];
                        break;
                    case "1e Collecte:":
                        Registration.Collecte1e = inputstringparts[2];
                        break;
                    case "2e Collecte:":
                        Registration.Collecte2e = inputstringparts[2];
                        break;
                    case "Lezen":
                        var lezenLijst = new List<string>();
                        for (var j = 2; j < inputstringparts.Length; j += 2)
                        {
                            lezenLijst.Add(inputstringparts[j]);
                        }
                        Registration.Lezen = lezenLijst.ToArray();
                        break;
                    case "Tekst":
                        var tekstLijst = new List<string>();
                        for (var j = 2; j < inputstringparts.Length; j += 2)
                        {
                            tekstLijst.Add(inputstringparts[j]);
                        }
                        Registration.Tekst = tekstLijst.ToArray();
                        break;
                }
            }
        }
        public string GetWorkingFile()
        {
            var output = string.Join("\n", Registration.Liturgie) + "\n";
            output += "<Voorganger:>" + Registration.Voorganger + "\n";
            output += "<1e Collecte:>" + Registration.Collecte1e + "\n";
            output += "<2e Collecte:>" + Registration.Collecte2e + "\n";

            output += "<Lezen>";
            var regels = Registration.Lezen.ToList();
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
            regels = Registration.Tekst.ToList();
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

        /// <summary>
        /// Opslaan van het meegegeven bestand op een locatie gekozen door savefiledialog
        /// </summary>
        /// <param name="bestand">bestand als string dat opgeslagen moet worden</param>
        public void Opslaan(string bestandsnaam, string inhoud)
        {
            //open een streamwriter naar gekozen bestand
            using (var writer = new StreamWriter(bestandsnaam))
            {
                //schrijf string weg naar streamwriter
                writer.Write(inhoud);

                //sla de locatie op voor het huidige bestand
                CurrentFile = bestandsnaam;
            }
        }

        /// <summary>
        /// uitlezen van een file op meegegeven pad
        /// </summary>
        /// <param name="pad"></param>
        /// <returns></returns>
        public string OpenenOpLocatie(string pad)
        {
            //open een filestream naar het gekozen bestand
            var strm = new FileStream(pad, FileMode.Open, FileAccess.Read);

            //gebruik streamreader om te lezen van de filestream
            using (var rdr = new StreamReader(strm))
            {
                //geef een melding dat het gelukt is
                Console.WriteLine("uitgelezen");

                //sla locatie op voor het huidige geopende bestand
                CurrentFile = pad;

                //geef het resultaat van de streamreader terug als string
                return rdr.ReadToEnd();
            }
        }

        public void NieuweLiturgie()
        {
            CurrentFile = "";
            Registration.Liturgie = new string[0];
            Registration.Voorganger = "";
            Registration.Collecte1e = "";
            Registration.Collecte2e = "";
            Registration.Lezen = new string[0];
            Registration.Tekst = new string[0];
        }

        public IEnumerable<ILiturgieOplossing> LiturgieOplossingen()
        {
            // Liturgie uit tekstbox omzetten in leesbare items
            var ruweLiturgie = _liturgieInterpreteer.VanTekstregels(Registration.Liturgie);
            // Zoek op het bestandssysteem zo veel mogelijk al op (behalve ppt, die gaan via COM element)
            var masks = MapMasksToLiturgie.Map(_instellingenFactory.LoadFromFile().Masks);
            return _liturgieOplosser.LosOp(ruweLiturgie, masks).ToList();
        }

        public PpGenerator.StatusMelding StartGenereren(IEnumerable<ILiturgieOplossing> ingeladenLiturgie, string opslaanAlsBestandsnaam)
        {
            Status = GeneratorStatus.AanHetGenereren;
            var lezenText = string.Join("\n\r", Registration.Lezen);
            var tekstText = string.Join("\n\r", Registration.Tekst);
            var status = _powerpoint.Initialiseer(ingeladenLiturgie.Select(l => l.Regel).ToList(), Registration.Voorganger, Registration.Collecte1e, Registration.Collecte2e, lezenText, tekstText, _instellingenFactory.LoadFromFile(), opslaanAlsBestandsnaam);
            if (status.Fout == null)
                status = _powerpoint.Start();
            // Stop weer als er een fout is geweest
            if (status.NieuweStatus != PpGenerator.State.Gestart)
                PresentatieGereedmeldingCallback();
            return status;
        }

        public PpGenerator.StatusMelding StopGenereren()
        {
            return _powerpoint.Stop();
        }

        public FileSavePossibility CheckFileSavePossibilities(string fileName)
        {
            // Check bestandsnaam
            if (!Path.HasExtension(fileName))
                fileName += ".ppt";
            if (File.Exists(fileName))
            {
                try
                {
                    File.Delete(fileName);
                }
                catch
                {
                    return FileSavePossibility.NotDeleteable;
                }
            }
            try
            {
                // Maak het bestand empty aan en check daarmee of er op die plek te schrijven is
                var file = File.Create(fileName);
                file.Close();
            }
            catch
            {
                return FileSavePossibility.NotCreateable;
            }
            return FileSavePossibility.Possible;
        }

        public enum FileSavePossibility
        {
            Possible,
            NotDeleteable,
            NotCreateable
        }
    }
}
