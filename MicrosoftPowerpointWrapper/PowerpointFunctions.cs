// Copyright 2017 door Erik de Roos
using ILiturgieDatabase;
using ISlideBuilder;
using mppt.Connect;
using mppt.LiedPresentator;
using mppt.RegelVerwerking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tools;

namespace mppt
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Presentatie slides worden hard vanaf het file systeem verwerkt! 
    /// (powerpoint heeft geen ondersteuning voor streams)
    /// </remarks>
    public class PowerpointFunctions : IBuilder, IDisposable
    {
        private IMppFactory _mppFactory { get; }
        private ILiedFormatter _liedFormatter { get; }
        private ILengteBerekenaar _lengteBerekenaar { get; }
        private Dictionary<VerwerkingType, IVerwerkFactory> _regelVerwerker { get; }

        private bool _stop;
        private CancellationTokenSource _token = null;

        private IEnumerable<ISlideOpbouw> _liturgie = new List<ISlideOpbouw>();
        private IBuilderBuildSettings _buildSettings;
        private IBuilderBuildDefaults _buildDefaults;
        private IBuilderDependendFiles _dependentFileList;
        private string _opslaanAls;

        public Action<int, int, int> Voortgang { get; set; }
        public Action<Status, string, int?> StatusWijziging { get; set; }

        public PowerpointFunctions(IMppFactory mppFactory, ILiedFormatter liedFormatter, ILengteBerekenaar lengteBerekenaar)
        {
            _mppFactory = mppFactory;
            _liedFormatter = liedFormatter;
            _lengteBerekenaar = lengteBerekenaar;
            _regelVerwerker = new Dictionary<VerwerkingType, IVerwerkFactory>();
            _regelVerwerker.Add(VerwerkingType.normaal, new VerwerkerNormaal());
            _regelVerwerker.Add(VerwerkingType.bijbeltekst, new VerwerkerBijbeltekst());
            _stop = false;
        }

        public void PreparePresentation(IEnumerable<ISlideOpbouw> liturgie, IBuilderBuildSettings buildSettings, IBuilderBuildDefaults buildDefaults, IBuilderDependendFiles dependentFileList, string opslaanAls)
        {
            _liturgie = liturgie;
            _buildSettings = buildSettings;
            _buildDefaults = buildDefaults;
            _dependentFileList = dependentFileList;
            _opslaanAls = opslaanAls;
        }

        /// <summary>
        /// Genereer een presentatie aan de hand van meegegeven Liturgie en Template voor de Liederen
        /// </summary>
        public void GeneratePresentation()
        {
            _token = new CancellationTokenSource();
            StatusWijziging?.Invoke(Status.Gestart, null, null);

            var succes = true;
            var slidesGemist = 0;
            var exception = string.Empty;
            
            // Hier pas COM calls want dit is de juiste thread
            using (var applicatie = _mppFactory.GetApplication())
            //Creeer een nieuwe lege presentatie volgens de template thema (toon scherm zodat bij fout nog iets te zien is)
            using (var presentatie = applicatie.Open(_dependentFileList.FullTemplateTheme, metWindow: true))
            {
                //Minimaliseer scherm
                applicatie.MinimizeInterface();

                try
                {
                    // Voor elke regel in de liturgie moeten sheets worden gemaakt (als dat mag)
                    // Gebruik een list zodat we de plek weten voor de progress
                    var hardeLijst = _liturgie.Where(l => l.VerwerkenAlsSlide).ToList();
                    foreach (var regel in hardeLijst)
                    {
                        var resultaat = _regelVerwerker[regel.VerwerkenAlsType]
                            .Init(applicatie, presentatie, _mppFactory, _liedFormatter, _buildSettings, _buildDefaults, _dependentFileList, _liturgie, _lengteBerekenaar)
                            .Verwerk(regel, Volgende(_liturgie, regel), _token.Token);
                        slidesGemist += resultaat.SlidesGemist;

                        Voortgang?.Invoke(0, _liturgie.Count(), hardeLijst.IndexOf(regel) + 1);
                        if (_stop)
                            break;
                    }

                    //sla de presentatie op
                    presentatie.OpslaanAls(_opslaanAls);
                }
                catch (Exception ex)
                {
                    succes = false;
                    exception = ex.ToString();
                    FoutmeldingSchrijver.Log(exception);
                }
            }
            _token = null;

            // dispose voordat we een statuswijziging hebben, anders is de applicatie niet op tijd gesloten

            if (succes)
            {
                // gereedmelding geven
                if (_stop)
                    StatusWijziging?.Invoke(Status.StopFout, "Tussentijds gestopt door de gebruiker.", null);
                else
                    StatusWijziging?.Invoke(Status.StopGoed, null, slidesGemist);
            }
            else
            {
                StatusWijziging?.Invoke(Status.StopFout, exception, null);
            }
        }

        /// <summary>
        /// Uitzoeken wat de volgende is
        /// </summary>
        private static IEnumerable<ISlideOpbouw> Volgende(IEnumerable<ISlideOpbouw> volledigeLiturgie, ISlideOpbouw huidig)
        {
            var lijst = volledigeLiturgie.ToList();
            var huidigeItemIndex = lijst.IndexOf(huidig);
            return lijst.Skip(huidigeItemIndex + 1);
        }

        public void ProbeerStop()
        {
            _stop = true;
        }

        public void ForceerStop()
        {
            ProbeerStop();
            _token?.Cancel();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_token != null)
                        _token.Dispose();
                    _token = null;
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