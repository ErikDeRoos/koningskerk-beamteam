using Generator;
using System.Windows.Forms;

namespace PowerpointGenerator.Screens
{

    class CompRegistration : ICompRegistration
    {
        public RichTextBox LiturgieLijstRichTextBox { get; set; }
        public TextBox VoorgangerTextTextBox { get; set; }
        public TextBox Collecte1eTextTextBox { get; set; }
        public TextBox Collecte2eTextTextBox { get; set; }
        public RichTextBox LezenLijstRichTextBox { get; set; }
        public RichTextBox TekstLijstRichTextBox { get; set; }

        public string LiturgieLijst { get { return LiturgieLijstRichTextBox?.Text; } set { if (LiturgieLijstRichTextBox != null) LiturgieLijstRichTextBox.Text = value; }  }
        public string VoorgangerText { get { return VoorgangerTextTextBox?.Text; } set { if (VoorgangerTextTextBox != null) VoorgangerTextTextBox.Text = value; } }
        public string Collecte1eText { get { return Collecte1eTextTextBox?.Text; } set { if (Collecte1eTextTextBox != null) Collecte1eTextTextBox.Text = value; } }
        public string Collecte2eText { get { return Collecte2eTextTextBox?.Text; } set { if (Collecte2eTextTextBox != null) Collecte2eTextTextBox.Text = value; } }
        public string LezenLijst { get { return LezenLijstRichTextBox?.Text; } set { if (LezenLijstRichTextBox != null) LezenLijstRichTextBox.Text = value; } }
        public string TekstLijst { get { return TekstLijstRichTextBox?.Text; } set { if (TekstLijstRichTextBox != null) TekstLijstRichTextBox.Text = value; } }
    }
}
