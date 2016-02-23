using ConnectTools;
using ISlideBuilder;
using System.ServiceModel;
using ILiturgieDatabase;
using ISettings;
using System;
using System.Collections.Generic;
using ConnectTools.Berichten;
using System.Linq;
using System.IO;
using System.Threading;
using Tools;

namespace PowerpointGenerater.Powerpoint
{
    class RemotePowerpointClient : IBuilder
    {
        private int _slideteller = 0;
        private int _slidesGemist = 0;
        private bool _stop;
        private ConnectieState _state = ConnectieState.MaakConnectie;

        private IWCFServer _proxy;
        private Token _token;
        private string _opslaanAls;
        private Instellingen _verzendInstellingen;
        private Liturgie _verzendLiturgie;
        private List<StreamTokenHolder> _streams;

        public Action<Status, string, int?> StatusWijziging { get; set; }
        public Action<int, int, int> Voortgang { get; set; }

        public RemotePowerpointClient(string endpoint)
        {
            _streams = new List<StreamTokenHolder>();
            var binding = new NetTcpBinding();
            binding.TransferMode = TransferMode.Streamed;
            binding.MaxBufferSize = 65536;  // 64kb
            binding.MaxReceivedMessageSize = 67108864; // max 64mb
            binding.OpenTimeout = new TimeSpan(0, 1, 0);
            binding.CloseTimeout = new TimeSpan(0, 1, 0);
            binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
            binding.SendTimeout = new TimeSpan(0, 10, 0);
            var address = new EndpointAddress(endpoint);
            var factory = new ChannelFactory<IWCFServer>(binding, address);
            _proxy = factory.CreateChannel();
        }

        public void PreparePresentation(IEnumerable<ILiturgieRegel> liturgie, string voorganger, string collecte1, string collecte2, string lezen, string tekst, IInstellingenBase gebruikInstellingen, string opslaanAls)
        {
            _verzendInstellingen = new Instellingen()
            {
                Regelsperslide = gebruikInstellingen.Regelsperslide,
                TemplateLiederenBestand = AddStream(gebruikInstellingen.FullTemplateliederen),
                TemplateThemeBestand = AddStream(gebruikInstellingen.FullTemplatetheme),
                StandaardTeksten = new StandaardTeksten()
                {
                    Volgende = gebruikInstellingen.StandaardTeksten.Volgende,
                    Voorganger = gebruikInstellingen.StandaardTeksten.Voorganger,
                    Collecte1 = gebruikInstellingen.StandaardTeksten.Collecte1,
                    Collecte2 = gebruikInstellingen.StandaardTeksten.Collecte2,
                    Collecte = gebruikInstellingen.StandaardTeksten.Collecte,
                    Lezen = gebruikInstellingen.StandaardTeksten.Lezen,
                    Tekst = gebruikInstellingen.StandaardTeksten.Tekst,
                    Liturgie = gebruikInstellingen.StandaardTeksten.Liturgie,
                    LiturgieLezen = gebruikInstellingen.StandaardTeksten.LiturgieLezen,
                    LiturgieTekst = gebruikInstellingen.StandaardTeksten.LiturgieTekst,
                }
            };
            _opslaanAls = opslaanAls;
            _verzendLiturgie = new Liturgie()
            {
                Voorganger = voorganger,
                Lezen = lezen,
                Tekst = tekst,
                Collecte1 = collecte1,
                Collecte2 = collecte2,
                Regels = liturgie.Select((r, i) => new LiturgieRegel()
                {
                    Index = i,
                    TonenInOverzicht = r.TonenInOverzicht,
                    TonenInVolgende = r.TonenInVolgende,
                    VerwerkenAlsSlide = r.VerwerkenAlsSlide,
                    Content = r.Content?.Select(c => new LiturgieRegelContent()
                    {
                        Nummer = c.Nummer,
                        InhoudType = c.InhoudType == ILiturgieDatabase.InhoudType.Tekst ? ConnectTools.Berichten.InhoudType.Tekst : ConnectTools.Berichten.InhoudType.PptLink,
                        InhoudTekst = c.InhoudType == ILiturgieDatabase.InhoudType.Tekst ? c.Inhoud : null,
                        InhoudBestand = c.InhoudType == ILiturgieDatabase.InhoudType.PptLink ? AddStream(c.Inhoud) : null
                    }),
                    Display = new LiturgieRegelDisplay()
                    {
                        Naam = r.Display.Naam,
                        NaamOverzicht = r.Display.NaamOverzicht,
                        SubNaam = r.Display.SubNaam,
                        VersenAfleiden = r.Display.VersenAfleiden,
                        VersenDefault = r.Display.VersenDefault
                    }
                })
            };
        }

        /// <summary>
        /// Genereer een presentatie aan de hand van meegegeven Liturgie en Template voor de Liederen
        /// </summary>
        /// <remarks>Deze functie draait altijd in een eigen thread</remarks>
        public void GeneratePresentation()
        {
            StatusWijziging?.Invoke(Status.Gestart, null, null);

            try
            {
                while (_state != ConnectieState.GereedPresentatieOntvangen)
                {
                    switch (_state)
                    {
                        case ConnectieState.MaakConnectie:
                            _state = MaakConnectie();
                            break;
                        case ConnectieState.VerzendBestanden:
                            _state = VerzendBestand();
                            break;
                        case ConnectieState.StartGenereren:
                            _state = StartGenereren();
                            break;
                        case ConnectieState.WachtOpGereed:
                            _state = CheckStatus();
                            break;
                    }
                    Voortgang?.Invoke(0, VoortgangTotaal(), VoortgangBij());
                    if (_stop)
                        break;
                    if (_state == ConnectieState.GereedPresentatieOntvangen) {
                        StatusWijziging?.Invoke(Status.StopGoed, null, _slidesGemist);
                        break;
                    }
                    else if (_state == ConnectieState.GereedPresentatieMislukt) {
                        StatusWijziging?.Invoke(Status.StopFout, "Genereren is mislukt", null);
                        break;
                    }
                    Thread.Sleep(200);
                }
            }
            catch (Exception ex)
            {
                FoutmeldingSchrijver.Log(ex.ToString());
                StatusWijziging?.Invoke(Status.StopFout, ex.ToString(), null);
                SluitAlles();
            }
        }

        private int VoortgangTotaal()
        {
            return _verzendLiturgie.Regels.Count() + 3 + _streams.Count;
        }
        private int VoortgangBij()
        {
            return _slideteller + (int)_state + _streams.Count(s => s.Verzonden);
        }

        private ConnectieState MaakConnectie()
        {
            _token = _proxy.StartConnectie(_verzendInstellingen, _verzendLiturgie);
            if (_token == null)
            {
                FoutmeldingSchrijver.Log("Kon connectie niet maken met remote server");
                return ConnectieState.GereedPresentatieMislukt;
            }
            return ConnectieState.VerzendBestanden;
        }

        private ConnectieState VerzendBestand()
        {
            var verzendStream = _streams.FirstOrDefault(s => !s.Verzonden);
            if (verzendStream == null)
                return ConnectieState.StartGenereren;
            _proxy.ToevoegenBestand(new SendFile() { Token = _token, FileToken = new StreamToken() { ID = verzendStream.ID }, FileByteStream = verzendStream.Stream });
            verzendStream.SetVerzonden();
            return _streams.All(s => s.Verzonden) ? ConnectieState.StartGenereren : ConnectieState.VerzendBestanden;
        }

        private ConnectieState StartGenereren()
        {
            var voortgang = _proxy.StartGenereren(_token);
            DisposeStreams();
            if (voortgang == null)
            {
                FoutmeldingSchrijver.Log("Kon genereren niet starten op remote server");
                return ConnectieState.GereedPresentatieMislukt;
            }
            return ConnectieState.WachtOpGereed;
        }

        private ConnectieState CheckStatus()
        {
            var voortgang = _proxy.CheckVoortgang(_token);
            if (voortgang == null)
            {
                FoutmeldingSchrijver.Log("Kon geen voortgang ontvangen van remote server");
                return ConnectieState.GereedPresentatieMislukt;
            }
            else if (voortgang.VolledigMislukt)
            {
                FoutmeldingSchrijver.Log("Generatie mislukt op remote server");
                return ConnectieState.GereedPresentatieMislukt;
            }
            _slideteller = voortgang.BijIndex;
            _slidesGemist = voortgang.MislukteSlides;
            if (voortgang.Gereed)
            {
                using (var copyTo = new FileStream(_opslaanAls, FileMode.Create))
                {
                    var resultaatStream = _proxy.DownloadResultaat(_token);
                    if (resultaatStream == null)
                    {
                        FoutmeldingSchrijver.Log("Kon geen presentatie vinden op remote server");
                        return ConnectieState.GereedPresentatieMislukt;
                    }
                    resultaatStream.CopyTo(copyTo);
                    copyTo.Close();
                }
                return ConnectieState.GereedPresentatieOntvangen;
            }
            return ConnectieState.WachtOpGereed;
        }

        private StreamToken AddStream(string file)
        {
            var token = new StreamTokenHolder(file);
            _streams.Add(token);
            return new StreamToken() { ID = token.ID, };
        }

        private void DisposeStreams()
        {
            foreach(var stream in _streams)
            {
                stream.DisposeStream();
            }
        }

        public void Stop()
        {
            _stop = true;
        }

        private void SluitAlles()
        {
            DisposeStreams();
        }
        public void Dispose()
        {
            SluitAlles();
        }

        public enum ConnectieState
        {
            MaakConnectie,
            StartGenereren,
            VerzendBestanden,
            WachtOpGereed,
            GereedPresentatieOntvangen,
            GereedPresentatieMislukt
        }

        class StreamTokenHolder
        {
            public Stream Stream { get; private set; }
            public Guid ID { get; private set; }
            public bool Verzonden { get; private set; }

            public StreamTokenHolder(string url)
            {
                Stream = new FileStream(url, FileMode.Open, FileAccess.Read);
                ID = Guid.NewGuid();
            }
            public void SetVerzonden()
            {
                Verzonden = true;
            }
            public void DisposeStream()
            {
                if (Stream == null)
                    return;
                Stream.Dispose();
                Stream = null;
            }
        }
    }
}
