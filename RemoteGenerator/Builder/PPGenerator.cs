using ConnectTools.Berichten;
using IFileSystem;
using ILiturgieDatabase;
using ISettings;
using ISlideBuilder;
using RemoteGenerator.Builder.Wachtrij;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Tools;

namespace RemoteGenerator.Builder
{
    /// Powerpoint roepen we aan via een wrapper zodat we meerdere requests af kunnen
    /// handelen terwijl we maar 1 generatie gelijktijdig doen.
    class PpGenerator : IDisposable, IPpGenerator
    {
        private State _huidigeStatus;
        private IEnumerable<ILiturgieRegel> _liturgie;
        private string _voorganger;
        private string _collecte1;
        private string _collecte2;
        private string _lezen;
        private string _tekst;
        private IInstellingenBase _instellingen;
        private string _opslaanAls;
        private IFileOperations _fileManager;
        private readonly Func<IBuilder> _builderResolver;

        private Thread _startThread;
        private IBuilder _powerpoint;
        private Thread _generatorThread;
        private Thread _stopThread;

        private string _gereedMetFout;
        private int? _slidesGemist;

        private WachtrijRegel _bezigMetRegel;
        private List<WachtrijRegel> _wachtrij;
        public IEnumerable<WachtrijRegel> Wachtrij => _wachtrij;
        private List<WachtrijRegel> _verwerkt;
        public IEnumerable<WachtrijRegel> Verwerkt => _verwerkt;

        public PpGenerator(Func<IBuilder> builderResolver, IFileOperations fileManager)
        {
            _builderResolver = builderResolver;
            _fileManager = fileManager;
            _huidigeStatus = State.Onbekend;
            _wachtrij = new List<WachtrijRegel>();
            _verwerkt = new List<WachtrijRegel>();
            _startThread = new Thread(PollVoorStart);
            _startThread.Start();
        }

        public WachtrijRegel NieuweWachtrijRegel(ConnectTools.Berichten.Instellingen metInstellingen, ConnectTools.Berichten.Liturgie metLiturgie)
        {
            var regel = new WachtrijRegel()
            {
                Voortgang = new Voortgang(),
                Token = new Token() { ID = Guid.NewGuid() },
                ToegevoegdOp = DateTime.Now,
                Bestanden = new List<BestandStreamToken>()
            };
            regel.Liturgie = new Wachtrij.Liturgie(metLiturgie, (new PrepareSaveToFileFactory(regel)).MaakBestandStreamToken);
            regel.Instellingen = new Wachtrij.Instellingen(metInstellingen, (new PrepareSaveToFileFactory(regel)).MaakBestandStreamToken);
            lock (this)
            {
                regel.Index = _wachtrij.Count() > 0 ? _wachtrij.Max(w => w.Index) + 1 : 1;
                _wachtrij.Add(regel);
            }
            return regel;
        }

        public void UpdateWachtrijRegel(Token vanToken, Guid bestandToken, Stream toevoegenBestand)
        {
            WachtrijRegel regel;
            lock (this)
            {
                regel = _wachtrij.FirstOrDefault(w => w.Token.ID == vanToken.ID);
                if (regel == null || !MagUpdaten(regel, _bezigMetRegel))
                    return;
                var bestandStreamToken = regel.Bestanden.FirstOrDefault(t => t.ID == bestandToken && !t.Ontvangen);
                if (bestandStreamToken == null)
                    return;
                bestandStreamToken.LinkOpFilesysteem = SaveToTempFile(toevoegenBestand);
            }
        }
        /// <summary>
        /// Compleetheid check. Start niet echt maar checkt alleen.
        /// </summary>
        public Voortgang ProbeerTeStarten(Token vanToken)
        {
            var regel = _wachtrij.FirstOrDefault(w => w.Token.ID == vanToken.ID);
            if (regel != null && IsRegelCompleet(regel))
                return regel.Voortgang;
            return null;
        }

        /// <summary>
        /// Alleen de poll thread start een generatie.
        /// </summary>
        private void PollVoorStart()
        {
            while (_startThread != null)
            {
                Thread.Sleep(100);
                if (_bezigMetRegel == null && (new[] { State.Onbekend, State.Geinitialiseerd }.Contains(_huidigeStatus)))
                {
                    var volgendePresentatie = _wachtrij.FirstOrDefault(r => IsRegelCompleet(r));
                    if (volgendePresentatie != null)
                    {
                        if (!IsRegelCompleet(volgendePresentatie))
                            continue;
                        lock (this)
                        {
                            if (MagUpdaten(volgendePresentatie, _bezigMetRegel) && _wachtrij.Contains(volgendePresentatie))
                            {
                                _bezigMetRegel = volgendePresentatie;
                                Start(volgendePresentatie);
                            }
                        }
                    }
                }
            }
        }

        private static bool IsRegelCompleet(WachtrijRegel regel)
        {
            return regel != null
                && regel.Instellingen != null
                && regel.Liturgie != null
                && regel.Bestanden.All(b => b.Ontvangen);
        }
        private static bool MagUpdaten(WachtrijRegel regel, WachtrijRegel bezigMetRegel)
        {
            return
                regel != bezigMetRegel
                && !regel.Voortgang.Gereed;
        }

        private void Start(WachtrijRegel regel)
        {
            var opslaanAlsBestandsnaam = Path.GetTempFileName();
            Initialiseer(regel.Liturgie.LiturgieRegels, regel.Liturgie.Voorganger, regel.Liturgie.Collecte1, regel.Liturgie.Collecte2,
                regel.Liturgie.Lezen, regel.Liturgie.Tekst, regel.Instellingen, opslaanAlsBestandsnaam);
            Start();
        }

        private string SaveToTempFile(Stream request)
        {
            var fileName = _fileManager.GetTempFileName();
            const int bufferSize = 2048;
            byte[] buffer = new byte[bufferSize];
            using (var outputStream = _fileManager.FileWriteStream(fileName))
            {
                request.CopyTo(outputStream);
            }
            return fileName;
        }

        private StatusMelding Initialiseer(IEnumerable<ILiturgieRegel> liturgie, string voorganger, string collecte1, string collecte2, string lezen,
          string tekst, IInstellingenBase instellingen, string opslaanAls)
        {
            lock (this)
            {
                _liturgie = liturgie.ToList();
                _voorganger = voorganger;
                _collecte1 = collecte1;
                _collecte2 = collecte2;
                _lezen = lezen;
                _tekst = tekst;
                _instellingen = instellingen;
                _opslaanAls = opslaanAls;
                _huidigeStatus = State.Geinitialiseerd;
                return new StatusMelding(_huidigeStatus);
            }
        }

        private StatusMelding Start()
        {
            lock (this)
            {
                if (_huidigeStatus != State.Geinitialiseerd)
                    return new StatusMelding(_huidigeStatus, "Kan powerpoint niet starten", "Start het programma opnieuw op");
                _powerpoint = _builderResolver();
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
                lock (this)
                {
                    GereedMelding(null, "Kon powerpoint niet opstarten");
                }
            }
        }

        private StatusMelding Stop()
        {
            lock (this)
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
            lock (this)
            {
                _powerpoint.Stop();
                for (int teller = 0; teller < 100 && _generatorThread.IsAlive; teller++)
                    Thread.Sleep(5);
                _generatorThread = null;
                _powerpoint.Dispose();
                _powerpoint = null;
                _huidigeStatus = State.Geinitialiseerd;
            }
            GereedMelding(_opslaanAls, _gereedMetFout, _slidesGemist);
        }

        private void PresentatieVoortgangCallback(int lijstStart, int lijstEind, int bijItem)
        {
            _bezigMetRegel.Voortgang.BijIndex = bijItem;
        }
        private void PresentatieStatusWijzigingCallback(Status nieuweStatus, string foutmelding = null, int? slidesGemist = null)
        {
            _gereedMetFout = foutmelding;
            _slidesGemist = slidesGemist;
            if (nieuweStatus == Status.StopFout || nieuweStatus == Status.StopGoed)
                Stop();
        }

        private void GereedMelding(string opgeslagenAlsBestand = null, string foutmelding = null, int? slidesGemist = null)
        {
            if (opgeslagenAlsBestand != null && foutmelding == null)
                AfrondenGelukt(opgeslagenAlsBestand, slidesGemist ?? 0);
            else {
                if (foutmelding != null)
                    FoutmeldingSchrijver.Log(foutmelding);
                AfrondenMislukt(opgeslagenAlsBestand);
            }
            lock (this)
            {
                _verwerkt.Add(_bezigMetRegel);
                _bezigMetRegel.Voortgang.Gereed = true;
                _wachtrij.Remove(_bezigMetRegel);
                _bezigMetRegel = null;
            }
        }

        private void AfrondenMislukt(string opgeslagenAlsBestand)
        {
            if (opgeslagenAlsBestand != null)
                try
                {
                    File.Delete(opgeslagenAlsBestand);
                }
                catch { }
            _bezigMetRegel.Voortgang.VolledigMislukt = true;
        }
        private void AfrondenGelukt(string opgeslagenAlsBestand, int slidesGemist)
        {
            _bezigMetRegel.Voortgang.MislukteSlides = slidesGemist;
            _bezigMetRegel.ResultaatOpgeslagenOp = opgeslagenAlsBestand;
        }
        private void Opruimen(WachtrijRegel regel)
        {
            throw new NotImplementedException();
        }

        public Stream KrijgGegenereerdBestand(Token token)
        {
            var item = Verwerkt.FirstOrDefault(w => w.Token.ID == token.ID);
            if (item == null || item.Voortgang.VolledigMislukt)
                return null;
            return _fileManager.FileReadStream(item.ResultaatOpgeslagenOp);
        }

        private void HardeStop()
        {
            var gelocked = Monitor.TryEnter(this, 100);  // probeer eerst lief maar als dat niet lukt, forceer exit
            if (_stopThread != null && _stopThread.IsAlive)
                _stopThread.Abort();
            _stopThread = null;
            if (_generatorThread != null && _generatorThread.IsAlive)
                _generatorThread.Abort();
            _generatorThread = null;
            _powerpoint.Dispose();
            _powerpoint = null;
            _huidigeStatus = State.Onbekend;
            if (gelocked)
                Monitor.Exit(this);
        }

        public void Dispose()
        {
            _startThread.Abort();
            _startThread = null;
            HardeStop();
        }

        public enum State
        {
            Onbekend,
            Geinitialiseerd,
            Gestart,
            AanHetStoppen,
        }

        class PrepareSaveToFileFactory
        {
            private WachtrijRegel _voorRegel;
            public PrepareSaveToFileFactory(WachtrijRegel voorRegel)
            {
                _voorRegel = voorRegel;
            }
            public BestandStreamToken MaakBestandStreamToken(StreamToken voorBericht)
            {
                var token = new BestandStreamToken()
                {
                    ID = voorBericht.ID
                };
                var nieuweCollectie = new List<BestandStreamToken>(_voorRegel.Bestanden);
                nieuweCollectie.Add(token);
                _voorRegel.Bestanden = nieuweCollectie;
                return token;
            }
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
