using ILiturgieDatabase;
using ISettings;
using ISlideBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteGeneratorConnect
{
    public class RemotePowerpointFunctions : IBuilder
    {
        private int _slideteller = 0;
        private int _slidesGemist = 0;
        private bool _stop;

        private IEnumerable<ILiturgieRegel> _liturgie = new List<ILiturgieRegel>();
        private string _voorganger;
        private string _collecte1;
        private string _collecte2;
        private string _lezen;
        private string _tekst;
        private IInstellingen _instellingen;
        private string _opslaanAls;
        private ConnectieState _state;

        public Action<int, int, int> Voortgang { get; set; }
        public Action<Status, string, int?> StatusWijziging { get; set; }

        public void PreparePresentation(IEnumerable<ILiturgieRegel> liturgie, string voorganger, string collecte1, string collecte2, string lezen, string tekst, IInstellingen gebruikInstellingen, string opslaanAls)
        {
            _liturgie = liturgie;
            _voorganger = voorganger;
            _collecte1 = collecte1;
            _collecte2 = collecte2;
            _lezen = lezen;
            _tekst = tekst;
            _instellingen = gebruikInstellingen;
            _opslaanAls = opslaanAls;
            _slideteller = 0;
            _state = ConnectieState.Leeg;
        }

        /// <summary>
        /// Genereer een presentatie aan de hand van meegegeven Liturgie en Template voor de Liederen
        /// </summary>
        public void GeneratePresentation()
        {
            StatusWijziging?.Invoke(Status.Gestart, null, null);
            var liturgieGrootte = _liturgie.Count();
            var aantalStappen = liturgieGrootte + 4;

            try
            {
                while (_state != ConnectieState.PresentatieOntvangen)
                {
                    switch(_state)
                    {
                        case ConnectieState.Leeg:
                            _state = BundelVerzoek();
                            break;
                        case ConnectieState.VerzoekGebundeld:
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
                }
            }
            catch (Exception ex)
            {
                FoutmeldingSchrijver.Log(ex.ToString());
                StatusWijziging?.Invoke(Status.StopFout, ex.ToString(), null);
                SluitAlles();
            }
        }

        private ConnectieState BundelVerzoek()
        {
            throw new NotImplementedException();
        }

        private ConnectieState MaakConnectie()
        {
            throw new NotImplementedException();
        }

        private ConnectieState VerzendVerzoek()
        {
            throw new NotImplementedException();
        }

        private ConnectieState CheckStatus()
        {
            throw new NotImplementedException();
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
            Leeg = 0,
            VerzoekGebundeld = 1,
            ConnectieBeschikbaar = 2,
            VerzoekVerzonden = 3,
            PresentatieOntvangen = 4,
        }
    }
}
