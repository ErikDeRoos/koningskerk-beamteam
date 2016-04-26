// Copyright 2016 door Erik de Roos
using ILiturgieDatabase;
using ISlideBuilder;
using mppt.Connect;
using mppt.LiedPresentator;
using mppt.RegelVerwerking;
using System;
using System.Collections.Generic;
using System.Linq;
using Tools;

namespace mppt
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>Zit hard op het file systeem! (powerpoint heeft geen ondersteuning voor streams)</remarks>
    public class PowerpointFunctions : IBuilder
    {
        private IMppFactory _mppFactory { get; }
        private ILiedFormatter _liedFormatter { get; }
        private Dictionary<VerwerkingType, IVerwerkFactory> _regelVerwerker { get; }

        private bool _stop;

        private IEnumerable<ILiturgieRegel> _liturgie = new List<ILiturgieRegel>();
        private IBuilderBuildSettings _buildSettings;
        private IBuilderBuildDefaults _buildDefaults;
        private IBuilderDependendFiles _dependentFileList;
        private string _opslaanAls;

        public Action<int, int, int> Voortgang { get; set; }
        public Action<Status, string, int?> StatusWijziging { get; set; }

        public PowerpointFunctions(IMppFactory mppFactory, ILiedFormatter liedFormatter)
        {
            _mppFactory = mppFactory;
            _liedFormatter = liedFormatter;
            _regelVerwerker = new Dictionary<VerwerkingType, IVerwerkFactory>();
            _regelVerwerker.Add(VerwerkingType.normaal, new VerwerkerNormaal());
            _regelVerwerker.Add(VerwerkingType.bijbeltekst, new VerwerkerBijbeltekst());
        }

        public void PreparePresentation(IEnumerable<ILiturgieRegel> liturgie, IBuilderBuildSettings buildSettings, IBuilderBuildDefaults buildDefaults, IBuilderDependendFiles dependentFileList, string opslaanAls)
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
            StatusWijziging?.Invoke(Status.Gestart, null, null);

            // Hier pas COM calls want dit is de juiste thread
            var applicatie = _mppFactory.GetApplication();
            //Creeer een nieuwe lege presentatie volgens de template thema (toon scherm zodat bij fout nog iets te zien is)
            var presentatie = applicatie.Open(_dependentFileList.FullTemplateTheme, metWindow: true);
            //Minimaliseer scherm
            applicatie.MinimizeInterface();

            try
            {
                var slidesGemist = 0;
                // Voor elke regel in de liturgie moeten sheets worden gemaakt (als dat mag)
                // Gebruik een list zodat we de plek weten voor de progress
                var hardeLijst = _liturgie.Where(l => l.VerwerkenAlsSlide).ToList();
                foreach (var regel in hardeLijst)
                {
                    var resultaat = _regelVerwerker[regel.VerwerkenAlsType]
                        .Init(applicatie, presentatie, _mppFactory, _liedFormatter, _buildSettings, _buildDefaults, _dependentFileList, _liturgie)
                        .Verwerk(regel, Volgende(_liturgie, regel));
                    slidesGemist += resultaat.SlidesGemist;

                    Voortgang?.Invoke(0, _liturgie.Count(), hardeLijst.IndexOf(regel) + 1);
                    if (_stop)
                        break;
                }

                //sla de presentatie op
                presentatie.OpslaanAls(_opslaanAls);
                // Eerst disposen zodat applicatie gesloten is voordat gereedmelding komt
                presentatie?.Dispose();
                applicatie?.Dispose();
                // gereedmelding geven
                if (_stop)
                    StatusWijziging?.Invoke(Status.StopFout, "Tussentijds gestopt door de gebruiker.", null);
                else
                    StatusWijziging?.Invoke(Status.StopGoed, null, slidesGemist);
            }
            catch (Exception ex)
            {
                FoutmeldingSchrijver.Log(ex.ToString());
                StatusWijziging?.Invoke(Status.StopFout, ex.ToString(), null);
                presentatie?.Dispose();
                applicatie?.Dispose();
            }
        }

        /// <summary>
        /// Uitzoeken wat de volgende is
        /// </summary>
        private static ILiturgieRegel Volgende(IEnumerable<ILiturgieRegel> volledigeLiturgie, ILiturgieRegel huidig)
        {
            var lijst = volledigeLiturgie.ToList();
            var huidigeItemIndex = lijst.IndexOf(huidig);
            return lijst.Skip(huidigeItemIndex + 1).FirstOrDefault();
        }


        public void Stop()
        {
            _stop = true;
        }

        public void Dispose()
        {
        }
    }
}