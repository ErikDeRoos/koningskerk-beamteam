namespace PowerpointGenerater
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.bestandToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nieuweLiturgieToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openLiturgieToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.slaLiturgieOpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.afsluitenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bewerkenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.knippenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.kopierenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.plakkenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.optiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bekijkDatabaseToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.templatesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.invoerenMasksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.directoryEntry1 = new System.DirectoryServices.DirectoryEntry();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.contactToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.RichTextBox();
            this.textBox5 = new System.Windows.Forms.RichTextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.stopPowerpointToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox1.Location = new System.Drawing.Point(12, 45);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(178, 306);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(350, 309);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(67, 42);
            this.button1.TabIndex = 1;
            this.button1.Text = "Generate";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // bestandToolStripMenuItem
            // 
            this.bestandToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nieuweLiturgieToolStripMenuItem,
            this.openLiturgieToolStripMenuItem,
            this.slaLiturgieOpToolStripMenuItem,
            this.toolStripMenuItem2,
            this.afsluitenToolStripMenuItem});
            this.bestandToolStripMenuItem.Name = "bestandToolStripMenuItem";
            this.bestandToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.bestandToolStripMenuItem.Text = "Bestand";
            // 
            // nieuweLiturgieToolStripMenuItem
            // 
            this.nieuweLiturgieToolStripMenuItem.Name = "nieuweLiturgieToolStripMenuItem";
            this.nieuweLiturgieToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.nieuweLiturgieToolStripMenuItem.Text = "Nieuwe Liturgie  (CTRL + N)";
            this.nieuweLiturgieToolStripMenuItem.Click += new System.EventHandler(this.nieuweLiturgieToolStripMenuItem_Click);
            // 
            // openLiturgieToolStripMenuItem
            // 
            this.openLiturgieToolStripMenuItem.Name = "openLiturgieToolStripMenuItem";
            this.openLiturgieToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.openLiturgieToolStripMenuItem.Text = "Open Liturgie      (CTRL + O)";
            this.openLiturgieToolStripMenuItem.Click += new System.EventHandler(this.openLiturgieToolStripMenuItem_Click);
            // 
            // slaLiturgieOpToolStripMenuItem
            // 
            this.slaLiturgieOpToolStripMenuItem.Name = "slaLiturgieOpToolStripMenuItem";
            this.slaLiturgieOpToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.slaLiturgieOpToolStripMenuItem.Text = "Sla Liturgie op     (CTRL + S)";
            this.slaLiturgieOpToolStripMenuItem.Click += new System.EventHandler(this.slaLiturgieOpToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(223, 22);
            this.toolStripMenuItem2.Text = "Sla Liturgie op als...";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // afsluitenToolStripMenuItem
            // 
            this.afsluitenToolStripMenuItem.Name = "afsluitenToolStripMenuItem";
            this.afsluitenToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.afsluitenToolStripMenuItem.Text = "Afsluiten              (CTRL + E)";
            this.afsluitenToolStripMenuItem.Click += new System.EventHandler(this.afsluitenToolStripMenuItem_Click);
            // 
            // bewerkenToolStripMenuItem
            // 
            this.bewerkenToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem3,
            this.toolStripMenuItem4,
            this.knippenToolStripMenuItem,
            this.kopierenToolStripMenuItem,
            this.plakkenToolStripMenuItem,
            this.toolStripMenuItem1});
            this.bewerkenToolStripMenuItem.Name = "bewerkenToolStripMenuItem";
            this.bewerkenToolStripMenuItem.Size = new System.Drawing.Size(70, 20);
            this.bewerkenToolStripMenuItem.Text = "Bewerken";
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(228, 22);
            this.toolStripMenuItem3.Text = "Ongedaan maken (CTRL + Z)";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.toolStripMenuItem3_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(228, 22);
            this.toolStripMenuItem4.Text = "Opnieuw doen      (CTRL + Y)";
            this.toolStripMenuItem4.Click += new System.EventHandler(this.toolStripMenuItem4_Click);
            // 
            // knippenToolStripMenuItem
            // 
            this.knippenToolStripMenuItem.Name = "knippenToolStripMenuItem";
            this.knippenToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
            this.knippenToolStripMenuItem.Text = "Knippen           (CTRL + X)";
            this.knippenToolStripMenuItem.Click += new System.EventHandler(this.knippenToolStripMenuItem_Click);
            // 
            // kopierenToolStripMenuItem
            // 
            this.kopierenToolStripMenuItem.Name = "kopierenToolStripMenuItem";
            this.kopierenToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
            this.kopierenToolStripMenuItem.Text = "Kopieren          (CTRL + C)";
            this.kopierenToolStripMenuItem.Click += new System.EventHandler(this.kopierenToolStripMenuItem_Click);
            // 
            // plakkenToolStripMenuItem
            // 
            this.plakkenToolStripMenuItem.Name = "plakkenToolStripMenuItem";
            this.plakkenToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
            this.plakkenToolStripMenuItem.Text = "Plakken            (CTRL + V)";
            this.plakkenToolStripMenuItem.Click += new System.EventHandler(this.plakkenToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(228, 22);
            this.toolStripMenuItem1.Text = "Selecteer alles (CTRL + A)";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // optiesToolStripMenuItem
            // 
            this.optiesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bekijkDatabaseToolStripMenuItem1,
            this.stopPowerpointToolStripMenuItem,
            this.templatesToolStripMenuItem1,
            this.invoerenMasksToolStripMenuItem});
            this.optiesToolStripMenuItem.Name = "optiesToolStripMenuItem";
            this.optiesToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.optiesToolStripMenuItem.Text = "Opties";
            // 
            // bekijkDatabaseToolStripMenuItem1
            // 
            this.bekijkDatabaseToolStripMenuItem1.Name = "bekijkDatabaseToolStripMenuItem1";
            this.bekijkDatabaseToolStripMenuItem1.Size = new System.Drawing.Size(162, 22);
            this.bekijkDatabaseToolStripMenuItem1.Text = "Bekijk Database";
            this.bekijkDatabaseToolStripMenuItem1.Click += new System.EventHandler(this.bekijkDatabaseToolStripMenuItem1_Click);
            // 
            // templatesToolStripMenuItem1
            // 
            this.templatesToolStripMenuItem1.Name = "templatesToolStripMenuItem1";
            this.templatesToolStripMenuItem1.Size = new System.Drawing.Size(162, 22);
            this.templatesToolStripMenuItem1.Text = "Instellingen";
            this.templatesToolStripMenuItem1.Click += new System.EventHandler(this.templatesToolStripMenuItem1_Click);
            // 
            // invoerenMasksToolStripMenuItem
            // 
            this.invoerenMasksToolStripMenuItem.Name = "invoerenMasksToolStripMenuItem";
            this.invoerenMasksToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.invoerenMasksToolStripMenuItem.Text = "Invoeren Masks";
            this.invoerenMasksToolStripMenuItem.Click += new System.EventHandler(this.invoerenMasksToolStripMenuItem_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(196, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Predikant:";
            // 
            // textBox2
            // 
            this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox2.Location = new System.Drawing.Point(199, 45);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(218, 20);
            this.textBox2.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(196, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Collecte 1:";
            // 
            // textBox3
            // 
            this.textBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox3.Location = new System.Drawing.Point(199, 84);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(218, 20);
            this.textBox3.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(196, 107);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(57, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Collecte 2:";
            // 
            // textBox4
            // 
            this.textBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox4.Location = new System.Drawing.Point(199, 123);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(218, 20);
            this.textBox4.TabIndex = 11;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bestandToolStripMenuItem,
            this.bewerkenToolStripMenuItem,
            this.optiesToolStripMenuItem,
            this.contactToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(429, 24);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // contactToolStripMenuItem
            // 
            this.contactToolStripMenuItem.Name = "contactToolStripMenuItem";
            this.contactToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.contactToolStripMenuItem.Text = "Contact";
            this.contactToolStripMenuItem.Click += new System.EventHandler(this.contactToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            this.helpToolStripMenuItem.Click += new System.EventHandler(this.helpToolStripMenuItem_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(0, 353);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(429, 10);
            this.progressBar1.TabIndex = 13;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(196, 146);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "Lezen:";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(196, 226);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(37, 13);
            this.label5.TabIndex = 15;
            this.label5.Text = "Tekst:";
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(199, 162);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(218, 61);
            this.textBox1.TabIndex = 16;
            this.textBox1.Text = "";
            // 
            // textBox5
            // 
            this.textBox5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox5.Location = new System.Drawing.Point(199, 242);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(218, 61);
            this.textBox5.TabIndex = 17;
            this.textBox5.Text = "";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(14, 27);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(44, 13);
            this.label6.TabIndex = 18;
            this.label6.Text = "Liturgie:";
            // 
            // stopPowerpointToolStripMenuItem
            // 
            this.stopPowerpointToolStripMenuItem.Name = "stopPowerpointToolStripMenuItem";
            this.stopPowerpointToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.stopPowerpointToolStripMenuItem.Text = "Stop powerpoint";
            this.stopPowerpointToolStripMenuItem.Click += new System.EventHandler(this.stopPowerpointToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(429, 363);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBox5);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Powerpoint Generator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ToolStripMenuItem bestandToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nieuweLiturgieToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openLiturgieToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem slaLiturgieOpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem afsluitenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bewerkenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem knippenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem kopierenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem plakkenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem optiesToolStripMenuItem;
        private System.DirectoryServices.DirectoryEntry directoryEntry1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.ToolStripMenuItem templatesToolStripMenuItem1;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.MenuStrip menuStrip1;
        public System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        public System.Windows.Forms.RichTextBox textBox1;
        public System.Windows.Forms.RichTextBox textBox5;
        private System.Windows.Forms.ToolStripMenuItem contactToolStripMenuItem;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ToolStripMenuItem bekijkDatabaseToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem invoerenMasksToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopPowerpointToolStripMenuItem;
    }
}

