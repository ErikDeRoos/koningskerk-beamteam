// Copyright 2024 door Erik de Roos
using Generator.LiturgieInterpretator;
using Generator.LiturgieInterpretator.Models;
using System;
using System.Linq;
using System.Windows.Forms;

namespace PowerpointGenerator.Screens
{
    public partial class LiturgieEdit : UserControl
    {
        public ILiturgieZoeken _liturgieZoeker { get; set; }
        new private readonly bool DesignMode;

        public TextBox TextBoxLiturgie { get { return textBoxLiturgie; } }

        // huidige zoekresultaat voor autocomplete
        private IVrijZoekresultaat _huidigZoekresultaat;
        private object _dropdownLocker = new object();

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
        private void toolStripMenuItemKnippen_Click(object sender, EventArgs e)
        {
            textBoxLiturgie.Cut();
        }

        private void toolStripMenuItemKopieren_Click(object sender, EventArgs e)
        {
            textBoxLiturgie.Copy();
        }

        private void toolStripMenuItemPlakken_Click(object sender, EventArgs e)
        {
            textBoxLiturgie.Paste();
        }

        private void textBoxZoek_TextChanged(object sender, EventArgs e)
        {
            TriggerZoeklijstVeranderd();
        }

        private void checkBoxAlsBijbeltekst_CheckedChanged(object sender, EventArgs e)
        {
            TriggerZoeklijstVeranderd();
        }

        private void buttonInvoegen_Click(object sender, EventArgs e)
        {
            HuidigeTekstInvoegen();

            textBoxZoek.Text = string.Empty;
            checkBoxAlsBijbeltekst.Checked = false;
        }
        #endregion events

        // TODO de werkwijze van het aanpassen van de autocomplete source veroorzaakt soms een access violation. Bijv. snel typen na opstarten.
        // TODO soms triggert de autoselect en wordt je tekst vanzelf geselecteerd, dat is irritant.
        // TODO er lijkt een memoryleak te zijn. Geheugengebruik loopt op als je snel wisselt tussen bijv. 'psalmen ' en 'psalmen 1'. Vermoedelijk de database.
        private void TriggerZoeklijstVeranderd()
        {
            if (!_liturgieZoeker.GaatVrijZoekenAnderResultaatGeven(textBoxZoek.Text, checkBoxAlsBijbeltekst.Checked, _huidigZoekresultaat))
                return;

           _huidigZoekresultaat = _liturgieZoeker.VrijZoeken(textBoxZoek.Text, checkBoxAlsBijbeltekst.Checked, _huidigZoekresultaat);
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
            var geinterpreteerdeOpties = _liturgieZoeker.ZoekStandaardOptiesUitZoekresultaat(textBoxZoek.Text, _huidigZoekresultaat);
            var toeTeVoegenTekst = _liturgieZoeker.MaakTotTekst(textBoxZoek.Text, geinterpreteerdeOpties, _huidigZoekresultaat);
            var liturgie = textBoxLiturgie.Lines.ToList();
            var huidigeRegel = GetHuidigeRegel();
            if (huidigeRegel != null)
                liturgie.Insert(huidigeRegel.LineNumber, toeTeVoegenTekst);
            else
                liturgie.Add(toeTeVoegenTekst);
            textBoxLiturgie.Lines = liturgie.ToArray();
        }

        private RegelStatus GetHuidigeRegel()
        {
            var postion = textBoxLiturgie.SelectionStart;
            var rowStartPosition = 0;
            var atLineNumber = 0;
            var lines = textBoxLiturgie.Lines.ToList();
            foreach (var row in lines)
            {
                int rowEndPosition = rowStartPosition + row.Length;
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
        private class RegelStatus
        {
            public string Text { get; set; }
            public int LineNumber { get; set; }
            public int TextStartingAtTextboxPosition { get; set; }
        }
    }
}
