// Copyright 2016 door Erik de Roos
using Generator;
using System.Windows.Forms;

namespace PowerpointGenerator.Screens
{

    class CompRegistration : ICompRegistration
    {
        public RichTextBox LiturgieRichTextBox { get; set; }
        public TextBox VoorgangerTextBox { get; set; }
        public TextBox Collecte1eTextBox { get; set; }
        public TextBox Collecte2eTextBox { get; set; }
        public RichTextBox LezenRichTextBox { get; set; }
        public RichTextBox TekstRichTextBox { get; set; }

        public string[] Liturgie { get { return LiturgieRichTextBox?.Lines; } set { if (LiturgieRichTextBox != null) LiturgieRichTextBox.Lines = value; }  }
        public string Voorganger { get { return VoorgangerTextBox?.Text; } set { if (VoorgangerTextBox != null) VoorgangerTextBox.Text = value; } }
        public string Collecte1e { get { return Collecte1eTextBox?.Text; } set { if (Collecte1eTextBox != null) Collecte1eTextBox.Text = value; } }
        public string Collecte2e { get { return Collecte2eTextBox?.Text; } set { if (Collecte2eTextBox != null) Collecte2eTextBox.Text = value; } }
        public string[] Lezen { get { return LezenRichTextBox?.Lines; } set { if (LezenRichTextBox != null) LezenRichTextBox.Lines = value; } }
        public string[] Tekst { get { return TekstRichTextBox?.Lines; } set { if (TekstRichTextBox != null) TekstRichTextBox.Lines = value; } }
    }
}
