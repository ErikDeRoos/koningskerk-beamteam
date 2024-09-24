namespace PowerpointGenerator.Screens
{
    partial class LiturgieEdit
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxAlsBijbeltekst = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonInvoegen = new System.Windows.Forms.Button();
            this.textBoxZoek = new System.Windows.Forms.TextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.textBoxLiturgie = new System.Windows.Forms.TextBox();
            this.contextMenuStripRightMouse = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemKnippen = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemKopieren = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemPlakken = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.contextMenuStripRightMouse.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBoxAlsBijbeltekst);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.buttonInvoegen);
            this.groupBox1.Controls.Add(this.textBoxZoek);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(418, 52);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            // 
            // checkBoxAlsBijbeltekst
            // 
            this.checkBoxAlsBijbeltekst.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxAlsBijbeltekst.AutoSize = true;
            this.checkBoxAlsBijbeltekst.Location = new System.Drawing.Point(264, 20);
            this.checkBoxAlsBijbeltekst.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBoxAlsBijbeltekst.Name = "checkBoxAlsBijbeltekst";
            this.checkBoxAlsBijbeltekst.Size = new System.Drawing.Size(74, 17);
            this.checkBoxAlsBijbeltekst.TabIndex = 6;
            this.checkBoxAlsBijbeltekst.Text = "Bijbeltekst";
            this.checkBoxAlsBijbeltekst.UseVisualStyleBackColor = true;
            this.checkBoxAlsBijbeltekst.CheckedChanged += new System.EventHandler(this.checkBoxAlsBijbeltekst_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 21);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Zoek";
            // 
            // buttonInvoegen
            // 
            this.buttonInvoegen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonInvoegen.Location = new System.Drawing.Point(343, 16);
            this.buttonInvoegen.Name = "buttonInvoegen";
            this.buttonInvoegen.Size = new System.Drawing.Size(68, 23);
            this.buttonInvoegen.TabIndex = 2;
            this.buttonInvoegen.Text = "Invoegen";
            this.buttonInvoegen.UseVisualStyleBackColor = true;
            this.buttonInvoegen.Click += new System.EventHandler(this.buttonInvoegen_Click);
            // 
            // textBoxZoek
            // 
            this.textBoxZoek.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxZoek.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.textBoxZoek.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.textBoxZoek.Location = new System.Drawing.Point(41, 19);
            this.textBoxZoek.Name = "textBoxZoek";
            this.textBoxZoek.Size = new System.Drawing.Size(218, 20);
            this.textBoxZoek.TabIndex = 1;
            this.textBoxZoek.TextChanged += new System.EventHandler(this.textBoxZoek_TextChanged);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.textBoxLiturgie);
            this.splitContainer1.Size = new System.Drawing.Size(418, 415);
            this.splitContainer1.SplitterDistance = 52;
            this.splitContainer1.SplitterWidth = 2;
            this.splitContainer1.TabIndex = 0;
            // 
            // textBoxLiturgie
            // 
            this.textBoxLiturgie.ContextMenuStrip = this.contextMenuStripRightMouse;
            this.textBoxLiturgie.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxLiturgie.Location = new System.Drawing.Point(0, 0);
            this.textBoxLiturgie.Multiline = true;
            this.textBoxLiturgie.Name = "textBoxLiturgie";
            this.textBoxLiturgie.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxLiturgie.Size = new System.Drawing.Size(418, 361);
            this.textBoxLiturgie.TabIndex = 1;
            // 
            // contextMenuStripRightMouse
            // 
            this.contextMenuStripRightMouse.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.contextMenuStripRightMouse.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemKnippen,
            this.toolStripMenuItemKopieren,
            this.toolStripMenuItemPlakken});
            this.contextMenuStripRightMouse.Name = "contextMenuStrip1";
            this.contextMenuStripRightMouse.Size = new System.Drawing.Size(122, 70);
            // 
            // toolStripMenuItemKnippen
            // 
            this.toolStripMenuItemKnippen.Name = "toolStripMenuItemKnippen";
            this.toolStripMenuItemKnippen.Size = new System.Drawing.Size(121, 22);
            this.toolStripMenuItemKnippen.Text = "Knippen";
            this.toolStripMenuItemKnippen.Click += new System.EventHandler(this.toolStripMenuItemKnippen_Click);
            // 
            // toolStripMenuItemKopieren
            // 
            this.toolStripMenuItemKopieren.Name = "toolStripMenuItemKopieren";
            this.toolStripMenuItemKopieren.Size = new System.Drawing.Size(121, 22);
            this.toolStripMenuItemKopieren.Text = "Kopieren";
            this.toolStripMenuItemKopieren.Click += new System.EventHandler(this.toolStripMenuItemKopieren_Click);
            // 
            // toolStripMenuItemPlakken
            // 
            this.toolStripMenuItemPlakken.Name = "toolStripMenuItemPlakken";
            this.toolStripMenuItemPlakken.Size = new System.Drawing.Size(121, 22);
            this.toolStripMenuItemPlakken.Text = "Plakken";
            this.toolStripMenuItemPlakken.Click += new System.EventHandler(this.toolStripMenuItemPlakken_Click);
            // 
            // LiturgieEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "LiturgieEdit";
            this.Size = new System.Drawing.Size(418, 415);
            this.Load += new System.EventHandler(this.LiturgieEdit_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.contextMenuStripRightMouse.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button buttonInvoegen;
        private System.Windows.Forms.TextBox textBoxZoek;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox textBoxLiturgie;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripRightMouse;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemKnippen;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemKopieren;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPlakken;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxAlsBijbeltekst;
    }
}
