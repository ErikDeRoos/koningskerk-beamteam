﻿using ILiturgieDatabase;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PowerpointGenerater
{
    public partial class LiturgieNotFoundFormulier : Form
    {
        public LiturgieNotFoundFormulier(IEnumerable<ILiturgieOplossing> fouten)
        {
            InitializeComponent();
            textBox1.Lines = fouten
                .Select(l => $"{l.VanInterpretatie.Benaming} {l.VanInterpretatie.Deel}: {l.Resultaat}")
                .ToArray();
        }
    }
}
