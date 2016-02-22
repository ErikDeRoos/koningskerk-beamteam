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
using System.Text;
using System.Threading;
using Tools;

namespace PowerpointGenerater.Powerpoint
{
    class RemotePowerpointClient : IBuilder
    {
        private int _slideteller = 0;
        private int _slidesGemist = 0;
        private bool _stop;
        private ConnectieState _state = ConnectieState.Leeg;

        private IWCFServer _proxy;
        private Token _token;
        private string _opslaanAls;
        private Instellingen _verzendInstellingen;
        private Liturgie _verzendLiturgie;

        public Action<Status, string, int?> StatusWijziging { get; set; }
        public Action<int, int, int> Voortgang { get; set; }

        public RemotePowerpointClient(string endpoint)
        {
            var binding = new NetTcpBinding();
            var address = new EndpointAddress(endpoint);
            var factory = new ChannelFactory<IWCFServer>(binding, address);
            _proxy = factory.CreateChannel();
        }

        public void PreparePresentation(IEnumerable<ILiturgieRegel> liturgie, string voorganger, string collecte1, string collecte2, string lezen, string tekst, IInstellingen gebruikInstellingen, string opslaanAls)
        {
            _verzendInstellingen = new Instellingen()
            {
                Regelsperslide = gebruikInstellingen.Regelsperslide,
                TemplateLiederen = File.ReadAllBytes(gebruikInstellingen.FullTemplateliederen),
                TemplateTheme = File.ReadAllBytes(gebruikInstellingen.FullTemplatetheme),
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
                        Inhoud = c.InhoudType == ILiturgieDatabase.InhoudType.Tekst ? Encoding.Unicode.GetBytes(c.Inhoud) : File.ReadAllBytes(c.Inhoud)
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
            var liturgieGrootte = _verzendLiturgie.Regels.Count();
            var aantalStappen = liturgieGrootte + 3;

            try
            {
                while (_state != ConnectieState.PresentatieOntvangen)
                {
                    switch (_state)
                    {
                        case ConnectieState.Leeg:
                            _state = MaakConnectie();
                            break;
                        case ConnectieState.ConnectieBeschikbaar:
                            _state = VerzendVerzoek();
                            break;
                        case ConnectieState.VerzoekVerzonden:
                            _state = CheckStatus();
                            break;
                    }
                    Voortgang?.Invoke(0, aantalStappen, (int)_state + _slideteller);
                    if (_stop)
                        break;
                    if (_state == ConnectieState.PresentatieOntvangen) {
                        StatusWijziging?.Invoke(Status.StopGoed, null, _slidesGemist);
                        break;
                    }
                    else if (_state == ConnectieState.PresentatieMislukt) {
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
        
        private ConnectieState MaakConnectie()
        {
            _token = _proxy.StartConnectie(_verzendInstellingen);
            if (_token == null)
                return ConnectieState.PresentatieMislukt;
            return ConnectieState.ConnectieBeschikbaar;
        }

        private ConnectieState VerzendVerzoek()
        {
            _proxy.StartGenereren(_token, _verzendLiturgie);
            return ConnectieState.VerzoekVerzonden;
        }

        private ConnectieState CheckStatus()
        {
            var voortgang = _proxy.CheckVoortgang(_token);
            if (voortgang.VolledigMislukt)
                return ConnectieState.PresentatieMislukt;
            _slideteller = voortgang.BijIndex;
            _slidesGemist = voortgang.MislukteSlides;
            if (voortgang.Gereed)
            {
                File.WriteAllBytes(_opslaanAls, _proxy.DownloadResultaat(_token));
                return ConnectieState.PresentatieOntvangen;
            }
            return ConnectieState.VerzoekVerzonden;
        }

        public void Stop()
        {
            _stop = true;
        }

        private void SluitAlles()
        {
        }
        public void Dispose()
        {
            SluitAlles();
        }

        public enum ConnectieState
        {
            Leeg,
            ConnectieBeschikbaar,
            VerzoekVerzonden,
            PresentatieOntvangen,
            PresentatieMislukt
        }
    }
}
