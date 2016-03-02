using ILiturgieDatabase;
using ISettings;
using ISlideBuilder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace PowerpointGenerator.Powerpoint
{
    /// Powerpoint roepen we aan via een wrapper zodat we de resources goed
    /// kunnen beheren. Dat is namelijk een must voor een goed gebruik van
    /// interop klassen.
    internal class PpGenerator : IDisposable
    {
        private State _huidigeStatus;
        private IEnumerable<ILiturgieRegel> _liturgie;
        private string _voorganger;
        private string _collecte1;
        private string _collecte2;
        private string _lezen;
        private string _tekst;
        private IInstellingen _instellingen;
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

        public StatusMelding Initialiseer(IEnumerable<ILiturgieRegel> liturgie, string voorganger, string collecte1, string collecte2, string lezen,
          string tekst, IInstellingen instellingen, string opslaanAls)
        {
            lock (_locker)
            {
                if (_huidigeStatus != State.Onbekend && _huidigeStatus != State.Geinitialiseerd)
                    return new StatusMelding(_huidigeStatus, "Kan powerpoint niet initialiseren", "Start het programma opnieuw op");
                _liturgie = liturgie.ToList();
                _voorganger = voorganger;
                _collecte1 = collecte1;
                _collecte2 = collecte2;
                _lezen = lezen;
                _tekst = tekst;
                _instellingen = instellingen;
                _opslaanAls = opslaanAls;

                if (!File.Exists(_instellingen.FullTemplatetheme))
                    return new StatusMelding(_huidigeStatus, "Het pad naar de achtergrond powerpoint presentatie kan niet worden gevonden", "Stel de achtergrond opnieuw in bij de templates");
                if (!File.Exists(instellingen.FullTemplateliederen))
                    return new StatusMelding(_huidigeStatus, "Het pad naar de liederen template powerpoint presentatie kan niet worden gevonden", "Stel de achtergrond opnieuw in bij de templates");

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
                _powerpoint.PreparePresentation(_liturgie, _voorganger, _collecte1, _collecte2, _lezen, _tekst, _instellingen, _opslaanAls);
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
                _powerpoint.Stop();
                for (int teller = 0; teller < 1000 && _generatorThread.IsAlive; teller++)
                    Thread.Sleep(5);
                _generatorThread = null;
                _powerpoint?.Dispose();
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


        public void Dispose()
        {
            var gelocked = Monitor.TryEnter(_locker, 100);  // probeer eerst lief maar als dat niet lukt, forceer exit
            if (_stopThread != null && _stopThread.IsAlive)
                _stopThread.Abort();
            _stopThread = null;
            if (_generatorThread != null && _generatorThread.IsAlive)
                _generatorThread.Abort();
            _generatorThread = null;
            _powerpoint.Dispose();
            _powerpoint = null;
            if (gelocked)
                Monitor.Exit(_locker);
        }

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
    }
}
