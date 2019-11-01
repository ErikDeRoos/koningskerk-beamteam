// Copyright 2019 door Erik de Roos
using Generator.Database;
using Generator.Powerpoint;
using ILiturgieDatabase;
using ISettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Generator
{
    /// <summary>
    /// Tussenklasse waarmee UI logica losgehaald is van de forms componenten
    /// </summary>
    public class GeneratieInterface<T> : IDisposable where T : class, ICompRegistration
    {
        private readonly ILiturgieSlideMaker _liturgieSlideMaker;
        private readonly ILiturgieTekstNaarObject _liturgieTekstNaarObject;
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


        public GeneratieInterface(ILiturgieSlideMaker liturgieSlideMaker, IInstellingenFactory instellingenOplosser, ILiturgieTekstNaarObject liturgieTekstNaarObject, Func<ISlideBuilder.IBuilder> builderResolver, ICompRegistration newCompRegistration)
        {
            _liturgieSlideMaker = liturgieSlideMaker;
            _liturgieTekstNaarObject = liturgieTekstNaarObject;
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
                    LaadVanWerkbestand(OpenenOpLocatie(startBestand));
                }
                catch { }
            }
            else if (File.Exists(TempLiturgiePath))
            {
                try
                {
                    LaadVanWerkbestand(OpenenOpLocatie(TempLiturgiePath));
                }
                catch { }
                try
                {
                    File.Delete(TempLiturgiePath);
                }
                catch { }
            }
        }

        public void LaadVanWerkbestand(string bestandsInhoud)
        {
            var liturgie = SaveFileHandling.LoadFromWorkingFile(bestandsInhoud);
            if (liturgie == null)
                return;

            Registration.Liturgie = liturgie.Liturgie;
            Registration.Voorganger = liturgie.Voorganger;
            Registration.Collecte1e = liturgie.Collecte1e;
            Registration.Collecte2e = liturgie.Collecte2e;
            Registration.Lezen = liturgie.Lezen;
            Registration.Tekst = liturgie.Tekst;
        }

        public string MaakWerkbestand()
        {
            return SaveFileHandling.CreateWorkingFile(new SaveFileHandling
            {
                Liturgie = Registration.Liturgie,
                Voorganger = Registration.Voorganger,
                Collecte1e = Registration.Collecte1e,
                Collecte2e = Registration.Collecte2e,
                Lezen = Registration.Lezen,
                Tekst = Registration.Tekst
            });
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
            _setGereedmelding?.Invoke(opgeslagenAlsBestand, foutmelding, slidesGemist);
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

        public IEnumerable<ITekstNaarSlideConversieResultaat> LiturgieOplossingen()
        {
            var instellingen = _instellingenFactory.LoadFromFile();

            // Liturgie uit tekstbox omzetten in leesbare items
            var liturgieObjecten = _liturgieTekstNaarObject.VanTekstregels(Registration.Liturgie);

            // Zoek op het bestandssysteem zo veel mogelijk al op (behalve ppt, die gaan via COM element)
            var masks = MapMasksToLiturgie.Map(instellingen.Masks);
            var settings = MapInstellingenToSettings.Map(instellingen);

            return liturgieObjecten.Select(i => _liturgieSlideMaker.ConverteerNaarSlide(i, settings, masks)).ToList();
        }

        public PpGenerator.StatusMelding StartGenereren(IEnumerable<ISlideOpbouw> ingeladenLiturgie, string opslaanAlsBestandsnaam)
        {
            Status = GeneratorStatus.AanHetGenereren;
            var lezenText = string.Join("\n", Registration.Lezen);
            var tekstText = string.Join("\n", Registration.Tekst);
            var status = _powerpoint.Initialiseer(ingeladenLiturgie.ToList(), Registration.Voorganger, Registration.Collecte1e, Registration.Collecte2e, lezenText, tekstText, _instellingenFactory.LoadFromFile(), opslaanAlsBestandsnaam);
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                    if (_powerpoint != null)
                        _powerpoint.Dispose();
                    _powerpoint = null;
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
