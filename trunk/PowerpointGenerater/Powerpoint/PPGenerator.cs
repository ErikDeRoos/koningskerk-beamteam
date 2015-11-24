using PowerpointGenerater.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace PowerpointGenerater.Powerpoint {
  /// Powerpoint roepen we aan via een wrapper zodat we de resources goed
  /// kunnen beheren. Dat is namelijk een must voor een goed gebruik van
  /// interop klassen.
  class PPGenerator : IDisposable {
    private State _huidigeStatus;
    private IEnumerable<ILiturgieZoekresultaat> _liturgie;
    private string _voorganger;
    private string _collecte1;
    private string _collecte2;
    private string _lezen;
    private string _tekst;
    private Instellingen _instellingen;
    private String _opslaanAls;

    private PowerpointFunctions _powerpoint;
    private Thread _generatorThread;
    private Thread _stopThread;
    private Object _locker = new Object();
    
    public delegate void Voortgang(int lijstStart, int lijstEind, int bijItem);
    private Voortgang _setVoortgang;
    public delegate void GereedMelding(String opgeslagenAlsBestand, String foutmelding = null);
    private GereedMelding _setGereedmelding;
    private String _gereedMetFout;

    public PPGenerator(Voortgang voortgangDelegate, GereedMelding gereedmeldingDelegate) {
      _huidigeStatus = State.Onbekend;
      _setVoortgang = voortgangDelegate;
      _setGereedmelding = gereedmeldingDelegate;
    }

    public StatusMelding Initialiseer(IEnumerable<ILiturgieZoekresultaat> liturgie, string Voorganger, string Collecte1, string Collecte2, string Lezen,
      string Tekst, Instellingen instellingen, String opslaanAls) {
      lock (_locker) {
        if (_huidigeStatus != State.Onbekend && _huidigeStatus != State.Geinitialiseerd)
          return new StatusMelding(_huidigeStatus, "Kan powerpoint niet initialiseren", "Start het programma opnieuw op");
        this._liturgie = liturgie.ToList();
        this._voorganger = Voorganger;
        this._collecte1 = Collecte1;
        this._collecte2 = Collecte2;
        this._lezen = Lezen;
        this._tekst = Tekst;
        this._instellingen = instellingen;
        this._opslaanAls = opslaanAls;

        if (!File.Exists(_instellingen.FullTemplatetheme))
          return new StatusMelding(_huidigeStatus, "Het pad naar de achtergrond powerpoint presentatie kan niet worden gevonden", "Stel de achtergrond opnieuw in bij de templates");
        else if (!File.Exists(instellingen.FullTemplateliederen))
          return new StatusMelding(_huidigeStatus, "Het pad naar de liederen template powerpoint presentatie kan niet worden gevonden", "Stel de achtergrond opnieuw in bij de templates");

        this._huidigeStatus = State.Geinitialiseerd;
        return new StatusMelding(_huidigeStatus);
      }
    }
    
    public StatusMelding Start() {
      lock (_locker) {
        if (_huidigeStatus != State.Geinitialiseerd)
          return new StatusMelding(_huidigeStatus, "Kan powerpoint niet starten", "Start het programma opnieuw op");
        _powerpoint = new PowerpointFunctions(PresentatieVoortgangCallback, PresentatieStatusWijzigingCallback);
        _gereedMetFout = null;
        _powerpoint.PreparePresentation(_liturgie, _voorganger, _collecte1, _collecte2, _lezen, _tekst, _instellingen, _opslaanAls);
        _generatorThread = new Thread(new ThreadStart(StartThread));
        _generatorThread.SetApartmentState(ApartmentState.STA);
        _generatorThread.Start();
        _huidigeStatus = State.Gestart;
        return new StatusMelding(_huidigeStatus);
      }
    }
    private void StartThread() {
      _powerpoint.GeneratePresentation();
    }

    public StatusMelding Stop() {
      lock (_locker) {
        if (_huidigeStatus != State.Gestart)
          return new StatusMelding(_huidigeStatus, "Kan powerpoint niet stoppen", "Start het programma opnieuw op");
        _stopThread = new Thread(new ThreadStart(ProbeerTeStoppen));
        _stopThread.Start();
        _huidigeStatus = State.AanHetStoppen;
        return new StatusMelding(_huidigeStatus);
      }
    }

    private void ProbeerTeStoppen() {
      lock (_locker) {
        _powerpoint.Stop();
        while (_generatorThread.IsAlive)
          Thread.Sleep(5);
        _generatorThread = null;
        _powerpoint.Dispose();
        _powerpoint = null;
        _huidigeStatus = State.Geinitialiseerd;
      }
      _setGereedmelding.Invoke(_opslaanAls, foutmelding: _gereedMetFout);
    }

    private void PresentatieVoortgangCallback(int lijstStart, int lijstEind, int bijItem) {
      _setVoortgang.Invoke(lijstStart, lijstEind, bijItem);
    }
    private void PresentatieStatusWijzigingCallback(PowerpointGenerater.Powerpoint.PowerpointFunctions.Status nieuweStatus, String foutmelding = null) {
      _gereedMetFout = foutmelding;
      if (nieuweStatus == PowerpointFunctions.Status.StopFout || nieuweStatus == PowerpointFunctions.Status.StopGoed)
        Stop();
    }


    public void Dispose() {
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

    public enum State {
      Onbekend,
      Geinitialiseerd,
      Gestart,
      AanHetStoppen,
    }

    public class StatusMelding {
      public Foutmelding Fout { get; private set; }
      public State NieuweStatus { get; private set; }
      public StatusMelding(State nieuweStatus, Foutmelding fout = null) {
        NieuweStatus = nieuweStatus;
        Fout = fout;
      }
      public StatusMelding(State nieuweStatus, String foutMelding, String foutOplossing) : this(nieuweStatus, new Foutmelding(foutMelding, foutOplossing)) {
      }
    }
    public class Foutmelding {
      public String Melding { get; private set; }
      public String Oplossing { get; private set; }
      public Foutmelding(String melding, String oplossing) {
        Melding = melding;
        Oplossing = oplossing;
      }
    }
  }
}
