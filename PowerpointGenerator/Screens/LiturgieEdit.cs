using ILiturgieDatabase;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace PowerpointGenerator.Screens
{
    public partial class LiturgieEdit : UserControl
    {
        private static readonly Keys[] TextDirectionKeys = new[] { Keys.Left, Keys.Right, Keys.Up, Keys.Down };
        public ILiturgieLosOp _liturgieOplosser { get; set; }
        new private readonly bool DesignMode;

        public TextBox TextBoxLiturgie { get { return textBoxLiturgie; } }

        // huidige zoekresultaat voor autocomplete
        private IVrijZoekresultaat _huidigZoekresultaat;
        private object _dropdownLocker = new object();
        private ILiturgieOptiesGebruiker _huidigeOptiesBijZoeken;

        // plek waar de gebruiker bezig is
        private int? _regelInBewerking = null;

        public LiturgieEdit()
        {
            DesignMode = (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime);
            InitializeComponent();
            textBoxZoek.AutoCompleteCustomSource = new AutoCompleteStringCollection();
        }

        private void LiturgieEdit_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
                TriggerZoeklijstVeranderd();
        }

        #region events
        private void timerUpdateSelection_Tick(object sender, EventArgs e)
        {
            UpdateSelection();
        }

        private void textBoxLiturgie_MouseClick(object sender, MouseEventArgs e)
        {
            ScheduleUpdateSelection();
        }

        private void textBoxLiturgie_KeyPress(object sender, KeyPressEventArgs e)
        {
            ScheduleUpdateSelection();
        }

        private void textBoxLiturgie_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (TextDirectionKeys.Contains(e.KeyCode))
                ScheduleUpdateSelection();
        }

        private void toolStripMenuItemKnippen_Click(object sender, EventArgs e)
        {
            textBoxLiturgie.Cut();
        }

        private void toolStripMenuItemKopieren_Click(object sender, EventArgs e)
        {
            textBoxLiturgie.Cut();
        }

        private void toolStripMenuItemPlakken_Click(object sender, EventArgs e)
        {
            textBoxLiturgie.Cut();
        }

        private void textBoxZoek_TextChanged(object sender, EventArgs e)
        {
            TriggerZoeklijstVeranderd();
        }

        private void textBoxZoek_KeyUp(object sender, KeyEventArgs e)
        {
            //if (e.KeyData == Keys.Enter)
            //    HuidigeTekstInvoegenEnInvoerLegen();
        }

        private void buttonWijzigOpties_Click(object sender, EventArgs e)
        {
            TriggerAanpassenOptiesBijZoeken();
        }

        private void buttonVervangen_Click(object sender, EventArgs e)
        {
            HuidigeTekstAanpassen();
        }

        private void buttonInvoegen_Click(object sender, EventArgs e)
        {
            HuidigeTekstInvoegen();
        }

        private void LiturgieEdit_Leave(object sender, EventArgs e)
        {
            buttonVervangen.Enabled = false;
            _regelInBewerking = null;
        }
        #endregion events

        // TODO de werkwijze van het aanpassen van de autocomplete source veroorzaakt soms een access violation. Bijv. snel typen na opstarten.
        // TODO soms triggert de autoselect en wordt je tekst vanzelf geselecteerd, dat is irritant.
        // TODO er lijkt een memoryleak te zijn. Geheugengebruik loopt op als je snel wisselt tussen bijv. 'psalmen ' en 'psalmen 1'. Vermoedelijk de database.
        private void TriggerZoeklijstVeranderd()
        {
            _huidigZoekresultaat = _liturgieOplosser.VrijZoeken(textBoxZoek.Text, _huidigZoekresultaat);
            if (_huidigZoekresultaat.ZoeklijstAanpassing == VrijZoekresultaatAanpassingType.Geen)
                return;

            // We gaan kijken wat de verandering is.
            // Dit moet wat slimmer dan gewoon verwijderen/toevoegen omdat deze lijst zich instabiel gedraagt
            textBoxZoek.SuspendLayout();
            lock (_dropdownLocker)  // Lock om te voorkomen dat werk nog niet af is als we er nog een x in komen (lijkt namelijk te gebeuren)
            {
                if (_huidigZoekresultaat == null || _huidigZoekresultaat.ZoeklijstAanpassing == VrijZoekresultaatAanpassingType.Alles || _huidigZoekresultaat.DeltaMogelijkhedenVerwijderd.Count() > 50)
                {
                    textBoxZoek.AutoCompleteCustomSource.Clear();
                    textBoxZoek.AutoCompleteCustomSource.AddRange(_huidigZoekresultaat.AlleMogelijkheden.Select(m => m.Weergave).ToArray());
                }
                else if (_huidigZoekresultaat.ZoeklijstAanpassing == VrijZoekresultaatAanpassingType.Deel)
                {
                    textBoxZoek.AutoCompleteCustomSource.AddRange(_huidigZoekresultaat.DeltaMogelijkhedenToegevoegd.Select(m => m.Weergave).ToArray());
                    foreach (var item in _huidigZoekresultaat.DeltaMogelijkhedenVerwijderd)
                    {
                        textBoxZoek.AutoCompleteCustomSource.Remove(item.Weergave);
                    }
                }
            }
            textBoxZoek.ResumeLayout();
        }

        private void HuidigeTekstInvoegen()
        {
            var geinterpreteerdeOpties = KrijgOptiesBijZoeken();
            var toeTeVoegenTekst = _liturgieOplosser.MaakTotTekst(textBoxZoek.Text, geinterpreteerdeOpties);
            var liturgie = textBoxLiturgie.Lines.ToList();
            if (_regelInBewerking.HasValue)
                liturgie.Insert(_regelInBewerking.Value, toeTeVoegenTekst);
            else
                liturgie.Add(toeTeVoegenTekst);
            textBoxLiturgie.Lines = liturgie.ToArray();
        }

        private void HuidigeTekstAanpassen()
        {
            if (!_regelInBewerking.HasValue)
                return;
            var geinterpreteerdeOpties = KrijgOptiesBijZoeken();
            var toeTeVoegenTekst = _liturgieOplosser.MaakTotTekst(textBoxZoek.Text, geinterpreteerdeOpties);
            var liturgie = textBoxLiturgie.Lines.ToList();
            liturgie[_regelInBewerking.Value] = toeTeVoegenTekst;
            textBoxLiturgie.Lines = liturgie.ToArray();
        }

        private void TriggerAanpassenOptiesBijZoeken()
        {
            var nieuweOpties = ToonAanpassenOptiesBijZoeken();
            if (nieuweOpties != null)
            {
                ZetHuidigeOpties(nieuweOpties);
            }
        }

        private void ZetHuidigeOpties(ILiturgieOptiesGebruiker opties)
        {
            if (opties != null)
            {
                _huidigeOptiesBijZoeken = opties;
                textBoxOpties.Text = _liturgieOplosser.MaakTotTekst(KrijgOptiesBijZoeken());
            }
            else
            {
                textBoxOpties.Text = null;
                _huidigeOptiesBijZoeken = null;
            }
        }

        private ILiturgieOptiesGebruiker KrijgOptiesBijZoeken()
        {
            if (_huidigeOptiesBijZoeken == null)
                _huidigeOptiesBijZoeken = _liturgieOplosser.ZoekStandaardOptiesUitZoekresultaat(textBoxZoek.Text, _huidigZoekresultaat);
            return _huidigeOptiesBijZoeken;
        }

        private ILiturgieOptiesGebruiker ToonAanpassenOptiesBijZoeken()
        {
            var optiesFormulier = new WijzigOpties();
            optiesFormulier.Initialise(KrijgOptiesBijZoeken());
            if (optiesFormulier.ShowDialog() != DialogResult.OK)
                return null;
            return optiesFormulier.GetOpties();
        }

        private RegelStatus GetHuidigeRegel()
        {
            var postion = textBoxLiturgie.SelectionStart;
            var rowStartPosition = 0;
            var atLineNumber = 0;
            var rowEndPosition = 0;
            var lines = textBoxLiturgie.Lines.ToList();
            foreach (var row in lines)
            {
                rowEndPosition = rowStartPosition + row.Length;
                if (postion <= rowEndPosition)
                    break;
                atLineNumber++;
                rowStartPosition = rowEndPosition + 2;
            }
            return new RegelStatus()
            {
                Text = lines.Count > 0 ? lines[atLineNumber] : string.Empty,
                LineNumber = atLineNumber,
                TextStartingAtTextboxPosition = rowStartPosition,
            };
        }

        private void ScheduleUpdateSelection()
        {
            timerUpdateSelection.Enabled = true;
            timerUpdateSelection.Start();
        }

        private void UpdateSelection()
        {
            timerUpdateSelection.Stop();
            var position = GetHuidigeRegel();
            _regelInBewerking = position.LineNumber;
            buttonVervangen.Enabled = true;
            ZetZoekTekst(position.Text);
        }

        private void ZetZoekTekst(string tekst)
        {
            var opsplitsing = _liturgieOplosser.SplitsVoorOpties(tekst);
            ILiturgieOptiesGebruiker opties = null;
            var liturgieRegel = string.Empty;
            if (opsplitsing.Length == 1)  // Alleen opties
                opties = _liturgieOplosser.ToonOpties(opsplitsing[0]);
            else if (opsplitsing.Length == 2)  // Liturgie en opties
            {
                liturgieRegel = opsplitsing[0];
                opties = _liturgieOplosser.ToonOpties(opsplitsing[1]);
            }
            else  // length = 0, alleen liturgie
            {
                liturgieRegel = tekst;
                opties = _liturgieOplosser.ZoekStandaardOptiesUitZoekresultaat(tekst, null);
            }
            _huidigZoekresultaat = null;
            textBoxZoek.Text = liturgieRegel;
            ZetHuidigeOpties(opties);
        }

        private class RegelStatus
        {
            public string Text { get; set; }
            public int LineNumber { get; set; }
            public int TextStartingAtTextboxPosition { get; set; }
        }
    }
}
