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


        public GeneratieInterface(ILiturgieLosOp liturgieOplosser, IInstellingenFactory instellingenOplosser, Func<ISlideBuilder.IBuilder> builderResolver, ICompRegistration newCompRegistration)
        {
            _liturgieOplosser = liturgieOplosser;
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
                    LoadWorkingfile(OpenenopLocatie(startBestand));
                }
                catch { }
            }
            else if (File.Exists(TempLiturgiePath))
            {
                try
                {
                    LoadWorkingfile(OpenenopLocatie(TempLiturgiePath));
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
            Registration.LiturgieLijst = "";
            var inputstring = SplitRegels.Split(input);
            var i = 0;
            for (; i < inputstring.Length; i++)
            {
                if (inputstring[i].StartsWith("<"))
                    break;
                if (!inputstring[i].Equals(""))
                    Registration.LiturgieLijst += inputstring[i] + "\n";
            }
            for (; i < inputstring.Length; i++)
            {
                if (inputstring[i].Equals("")) continue;
                var inputstringparts = inputstring[i].Split('<', '>');
                switch (inputstringparts[1])
                {
                    case "Voorganger:":
                        Registration.VoorgangerText = inputstringparts[2];
                        break;
                    case "1e Collecte:":
                        Registration.Collecte1eText = inputstringparts[2];
                        break;
                    case "2e Collecte:":
                        Registration.Collecte2eText = inputstringparts[2];
                        break;
                    case "Lezen":
                        Registration.LezenLijst = "";
                        for (var j = 2; j < inputstringparts.Length; j += 2)
                        {
                            if (j + 2 < inputstringparts.Length)
                                Registration.LezenLijst += inputstringparts[j] + "\n";
                            else
                                Registration.LezenLijst += inputstringparts[j];
                        }
                        break;
                    case "Tekst":
                        Registration.TekstLijst = "";
                        for (var j = 2; j < inputstringparts.Length; j += 2)
                        {
                            if (j + 2 < inputstringparts.Length)
                                Registration.TekstLijst += inputstringparts[j] + "\n";
                            else
                                Registration.TekstLijst += inputstringparts[j];
                        }
                        break;
                }
            }
        }
        public string GetWorkingFile()
        {
            var output = Registration.LiturgieLijst + "\n";
            output += "<Voorganger:>" + Registration.VoorgangerText + "\n";
            output += "<1e Collecte:>" + Registration.Collecte1eText + "\n";
            output += "<2e Collecte:>" + Registration.Collecte2eText + "\n";

            output += "<Lezen>";
            var regels = (Registration.LezenLijst ?? "").Split(new[] { "\r\n" }, StringSplitOptions.None);
            for (var i = 0; i < regels.Length; i++)
            {
                if (regels[i].Equals("")) continue;
                if (i + 1 < regels.Length)
                    output += regels[i] + "<n>";
                else
                    output += regels[i];
            }
            output += "\n";

            output += "<Tekst>";
            regels = (Registration.TekstLijst ?? "").Split(new[] { "\r\n" }, StringSplitOptions.None);
            for (var i = 0; i < regels.Length; i++)
            {
                if (regels[i].Equals("")) continue;
                if (i + 1 < regels.Length)
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
        public string OpenenopLocatie(string pad)
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
            Registration.LiturgieLijst = "";
            Registration.VoorgangerText = "";
            Registration.Collecte1eText = "";
            Registration.Collecte2eText = "";
            Registration.LezenLijst = "";
            Registration.TekstLijst = "";
        }

        public IEnumerable<ILiturgieOplossing> LiturgieOplossingen()
        {
            // Liturgie uit tekstbox omzetten in leesbare items
            var ruweLiturgie = new InterpreteerLiturgieRuw().VanTekstregels(SplitRegels.Split(Registration.LiturgieLijst));
            // Zoek op het bestandssysteem zo veel mogelijk al op (behalve ppt, die gaan via COM element)
            var masks = MapMasksToLiturgie.Map(_instellingenFactory.LoadFromXmlFile().Masks);
            return _liturgieOplosser.LosOp(ruweLiturgie, masks).ToList();
        }

        public PpGenerator.StatusMelding StartGenereren(IEnumerable<ILiturgieOplossing> ingeladenLiturgie, string opslaanAlsBestandsnaam)
        {
            Status = GeneratorStatus.AanHetGenereren;
            var status = _powerpoint.Initialiseer(ingeladenLiturgie.Select(l => l.Regel).ToList(), Registration.VoorgangerText, Registration.Collecte1eText, Registration.Collecte2eText, Registration.LezenLijst, Registration.TekstLijst, _instellingenFactory.LoadFromXmlFile(), opslaanAlsBestandsnaam);
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
