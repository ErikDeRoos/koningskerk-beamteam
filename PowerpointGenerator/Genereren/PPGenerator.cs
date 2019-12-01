// Copyright 2019 door Remco Veurink en Erik de Roos
using Generator.Database.Models;
using ISettings;
using mppt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace PowerpointGenerator.Genereren
{
    /// Powerpoint roepen we aan via een wrapper zodat we de resources goed
    /// kunnen beheren. Dat is namelijk een must voor een goed gebruik van
    /// interop klassen.
    public class PpGenerator : IDisposable
    {
        private State _huidigeStatus;
        private IEnumerable<ISlideOpbouw> _liturgie;
        private IBuilderBuildSettings _builderSettings;
        private IBuilderBuildDefaults _builderDefaults;
        private IBuilderDependendFiles _builderDependentFileList;
        private string _opslaanAls;
        private readonly Func<IBuilder> _builderResolver;

        private IBuilder _powerpoint;
        private Thread _generatorThread;
        private Thread _stopThread;
        private readonly object _locker = new object();

        public delegate void Voortgang(int lijstStart, int lijstEind, int bijItem);
        private readonly Voortgang _setVoortgang;
        public delegate void GereedMelding(string opgeslagenAlsBestand = null, string foutmelding = null, int? slidesGemist = null);
        private readonly GereedMelding _setGereedmelding;
        private string _gereedMetFout;
        private int? _slidesGemist;

        public PpGenerator(Func<IBuilder> builderResolver, Voortgang voortgangDelegate, GereedMelding gereedmeldingDelegate)
        {
            _builderResolver = builderResolver;
            _huidigeStatus = State.Onbekend;
            _setVoortgang = voortgangDelegate;
            _setGereedmelding = gereedmeldingDelegate;
        }

        public StatusMelding Initialiseer(IEnumerable<ISlideOpbouw> liturgie, string voorganger, string collecte1, string collecte2, string lezen,
          string tekst, IInstellingen instellingen, string opslaanAls)
        {
            lock (_locker)
            {
                if (_huidigeStatus != State.Onbekend && _huidigeStatus != State.Geinitialiseerd)
                    return new StatusMelding(_huidigeStatus, "Kan powerpoint niet initialiseren", "Start het programma opnieuw op");
                _liturgie = liturgie.ToList();
                _builderSettings = new BuilderBuildSettings(voorganger, collecte1, collecte2, lezen, tekst, instellingen.Een2eCollecte);
                var defaults = new BuilderDefaults(instellingen);
                _builderDefaults = defaults;
                _builderDependentFileList = defaults;
                _opslaanAls = opslaanAls;

                if (!File.Exists(_builderDependentFileList.FullTemplateTheme))
                    return new StatusMelding(_huidigeStatus, "Het pad naar de achtergrond powerpoint presentatie kan niet worden gevonden", "Stel de achtergrond opnieuw in");
                if (!File.Exists(_builderDependentFileList.FullTemplateLied))
                    return new StatusMelding(_huidigeStatus, "Het pad naar de lied template powerpoint presentatie kan niet worden gevonden", "Stel de lied template opnieuw in");
                if (!File.Exists(_builderDependentFileList.FullTemplateBijbeltekst))
                    return new StatusMelding(_huidigeStatus, "Het pad naar de bijbeltekst template powerpoint presentatie kan niet worden gevonden", "Stel de bijbeltekst template opnieuw in");

                _huidigeStatus = State.Geinitialiseerd;
                return new StatusMelding(_huidigeStatus);
            }
        }

        public StatusMelding Start()
        {
            lock (_locker)
            {
                if (_huidigeStatus != State.Geinitialiseerd)
                    return new StatusMelding(_huidigeStatus, "Kan powerpoint niet starten", "Start het programma opnieuw op");
                try
                {
                    _powerpoint = _builderResolver();
                }
                catch
                {
                    return new StatusMelding(_huidigeStatus, "Kan powerpoint niet starten", "Powerpoint koppeling kon niet geladen worden");
                }
                _powerpoint.StatusWijziging = PresentatieStatusWijzigingCallback;
                _powerpoint.Voortgang = PresentatieVoortgangCallback;
                _gereedMetFout = null;
                _powerpoint.PreparePresentation(_liturgie, _builderSettings, _builderDefaults, _builderDependentFileList, _opslaanAls);
                _generatorThread = new Thread(StartThread);
                _generatorThread.SetApartmentState(ApartmentState.STA);
                _huidigeStatus = State.Gestart;
                _generatorThread.Start();
                return new StatusMelding(_huidigeStatus);
            }
        }
        private void StartThread()
        {
            try {
                _powerpoint.GeneratePresentation();
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                lock (_locker)
                {
                    _setGereedmelding.Invoke(null, "Kon powerpoint niet opstarten");
                }
            }
        }

        public StatusMelding Stop()
        {
            lock (_locker)
            {
                if (_huidigeStatus != State.Gestart)
                    return new StatusMelding(_huidigeStatus, "Kan powerpoint niet stoppen", "Start het programma opnieuw op");
                _stopThread = new Thread(ProbeerTeStoppen);
                _stopThread.Start();
                _huidigeStatus = State.AanHetStoppen;
                return new StatusMelding(_huidigeStatus);
            }
        }

        private void ProbeerTeStoppen()
        {
            lock (_locker)
            {
                _powerpoint.ProbeerStop();
                for (int teller = 0; teller < 1000 && _generatorThread.IsAlive; teller++)
                    Thread.Sleep(5);
                _generatorThread = null;
                _powerpoint.ForceerStop();
                _powerpoint = null;
                _huidigeStatus = State.Geinitialiseerd;
            }
            _setGereedmelding.Invoke(_opslaanAls, _gereedMetFout, _slidesGemist);
        }

        private void PresentatieVoortgangCallback(int lijstStart, int lijstEind, int bijItem)
        {
            _setVoortgang.Invoke(lijstStart, lijstEind, bijItem);
        }
        private void PresentatieStatusWijzigingCallback(Status nieuweStatus, string foutmelding = null, int? slidesGemist = null)
        {
            _gereedMetFout = foutmelding;
            _slidesGemist = slidesGemist;
            if (nieuweStatus == Status.StopFout || nieuweStatus == Status.StopGoed)
                Stop();
        }


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    bool gelocked = false;
                    // dispose managed state (managed objects).
                    try
                    {
                        Monitor.TryEnter(_locker, 100, ref gelocked);  // probeer eerst lief (door lock met timeout aan te vragen) maar ga door (forceer) als dat niet lukt
                        if (_stopThread != null && _stopThread.IsAlive)
                            _stopThread.Abort();
                        _stopThread = null;
                        if (_generatorThread != null && _generatorThread.IsAlive)
                            _generatorThread.Abort();
                        _generatorThread = null;
                        if (_powerpoint != null)
                            _powerpoint.ForceerStop();
                        _powerpoint = null;
                    }
                    finally
                    {
                        if (gelocked)
                            Monitor.Exit(_locker);
                    }
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

        public enum State
        {
            Onbekend,
            Geinitialiseerd,
            Gestart,
            AanHetStoppen,
        }

        public class StatusMelding
        {
            public Foutmelding Fout { get; private set; }
            public State NieuweStatus { get; private set; }
            public StatusMelding(State nieuweStatus, Foutmelding fout = null)
            {
                NieuweStatus = nieuweStatus;
                Fout = fout;
            }
            public StatusMelding(State nieuweStatus, string foutMelding, string foutOplossing) : this(nieuweStatus, new Foutmelding(foutMelding, foutOplossing))
            {
            }
        }
        public class Foutmelding
        {
            public string Melding { get; private set; }
            public string Oplossing { get; private set; }
            public Foutmelding(string melding, string oplossing)
            {
                Melding = melding;
                Oplossing = oplossing;
            }
        }

        private class BuilderBuildSettings : IBuilderBuildSettings
        {
            public string Voorganger { get; set; }
            public string Collecte1 { get; set; }
            public string Collecte2 { get; set; }
            public string Lezen { get; set; }
            public string Tekst { get; set; }
            public bool Een2eCollecte { get; set; }

            public BuilderBuildSettings(string voorganger, string collecte1, string collecte2, string lezen, string tekst, bool een2eCollecte)
            {
                Voorganger = voorganger;
                Collecte1 = collecte1;
                Collecte2 = collecte2;
                Lezen = lezen;
                Tekst = tekst;
                Een2eCollecte = een2eCollecte;
            }
        }
        private class BuilderDefaults : IBuilderBuildDefaults, IBuilderDependendFiles
        {
            public int RegelsPerLiedSlide { get; set; }
            public int RegelsPerBijbeltekstSlide { get; set; }
            public string FullTemplateTheme { get; set; }
            public string FullTemplateLied{ get; set; }
            public string FullTemplateBijbeltekst { get; set; }
            public string LabelVolgende { get; set; }
            public string LabelVoorganger { get; set; }
            public string LabelCollecte1 { get; set; }
            public string LabelCollecte2 { get; set; }
            public string LabelCollecte { get; set; }
            public string LabelLezen { get; set; }
            public string LabelTekst { get; set; }
            public string LabelLiturgie { get; set; }
            public string LabelLiturgieLezen { get; set; }
            public string LabelLiturgieTekst { get; set; }
            public bool VerkortVerzenBijVolledigeContent { get; set; }

            public BuilderDefaults(IInstellingen opBasisVanInstellingen)
            {
                RegelsPerLiedSlide = opBasisVanInstellingen.RegelsPerLiedSlide;
                RegelsPerBijbeltekstSlide = opBasisVanInstellingen.RegelsPerBijbeltekstSlide;
                LabelVolgende = opBasisVanInstellingen.StandaardTeksten.Volgende;
                LabelVoorganger = opBasisVanInstellingen.StandaardTeksten.Voorganger;
                LabelCollecte1 = opBasisVanInstellingen.StandaardTeksten.Collecte1;
                LabelCollecte2 = opBasisVanInstellingen.StandaardTeksten.Collecte2;
                LabelCollecte = opBasisVanInstellingen.StandaardTeksten.Collecte;
                LabelLezen = opBasisVanInstellingen.StandaardTeksten.Lezen;
                LabelTekst = opBasisVanInstellingen.StandaardTeksten.Tekst;
                LabelLiturgie = opBasisVanInstellingen.StandaardTeksten.Liturgie;
                LabelLiturgieLezen = opBasisVanInstellingen.StandaardTeksten.LiturgieLezen;
                LabelLiturgieTekst = opBasisVanInstellingen.StandaardTeksten.LiturgieTekst;
                FullTemplateTheme = opBasisVanInstellingen.FullTemplateTheme;
                FullTemplateLied = opBasisVanInstellingen.FullTemplateLied;
                FullTemplateBijbeltekst = opBasisVanInstellingen.FullTemplateBijbeltekst;
                VerkortVerzenBijVolledigeContent = opBasisVanInstellingen.ToonGeenVersenBijVolledigeContent;
            }
        }
    }
}
