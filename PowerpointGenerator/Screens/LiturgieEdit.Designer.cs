﻿namespace PowerpointGenerator.Screens
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
            this.buttonVervangen = new System.Windows.Forms.Button();
            this.buttonWijzigOpties = new System.Windows.Forms.Button();
            this.buttonInvoegen = new System.Windows.Forms.Button();
            this.textBoxZoek = new System.Windows.Forms.TextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.textBoxLiturgie = new System.Windows.Forms.TextBox();
            this.timerUpdateSelection = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStripRightMouse = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemKnippen = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemKopieren = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemPlakken = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxOpties = new System.Windows.Forms.TextBox();
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
            this.groupBox1.Controls.Add(this.textBoxOpties);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.buttonVervangen);
            this.groupBox1.Controls.Add(this.buttonWijzigOpties);
            this.groupBox1.Controls.Add(this.buttonInvoegen);
            this.groupBox1.Controls.Add(this.textBoxZoek);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(6);
            this.groupBox1.Size = new System.Drawing.Size(766, 132);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            // 
            // buttonVervangen
            // 
            this.buttonVervangen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonVervangen.Enabled = false;
            this.buttonVervangen.Location = new System.Drawing.Point(175, 76);
            this.buttonVervangen.Margin = new System.Windows.Forms.Padding(6);
            this.buttonVervangen.Name = "buttonVervangen";
            this.buttonVervangen.Size = new System.Drawing.Size(290, 42);
            this.buttonVervangen.TabIndex = 4;
            this.buttonVervangen.Text = "Vervang geselecteerde";
            this.buttonVervangen.UseVisualStyleBackColor = true;
            this.buttonVervangen.Click += new System.EventHandler(this.buttonVervangen_Click);
            // 
            // buttonWijzigOpties
            // 
            this.buttonWijzigOpties.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonWijzigOpties.Location = new System.Drawing.Point(590, 29);
            this.buttonWijzigOpties.Margin = new System.Windows.Forms.Padding(6);
            this.buttonWijzigOpties.Name = "buttonWijzigOpties";
            this.buttonWijzigOpties.Size = new System.Drawing.Size(164, 42);
            this.buttonWijzigOpties.TabIndex = 3;
            this.buttonWijzigOpties.Text = "Zet opties";
            this.buttonWijzigOpties.UseVisualStyleBackColor = true;
            this.buttonWijzigOpties.Click += new System.EventHandler(this.buttonWijzigOpties_Click);
            // 
            // buttonInvoegen
            // 
            this.buttonInvoegen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonInvoegen.Location = new System.Drawing.Point(477, 76);
            this.buttonInvoegen.Margin = new System.Windows.Forms.Padding(6);
            this.buttonInvoegen.Name = "buttonInvoegen";
            this.buttonInvoegen.Size = new System.Drawing.Size(277, 42);
            this.buttonInvoegen.TabIndex = 2;
            this.buttonInvoegen.Text = "Invoegen voor geselecteerde";
            this.buttonInvoegen.UseVisualStyleBackColor = true;
            this.buttonInvoegen.Click += new System.EventHandler(this.buttonInvoegen_Click);
            // 
            // textBoxZoek
            // 
            this.textBoxZoek.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxZoek.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.textBoxZoek.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.textBoxZoek.Location = new System.Drawing.Point(153, 35);
            this.textBoxZoek.Margin = new System.Windows.Forms.Padding(6);
            this.textBoxZoek.Name = "textBoxZoek";
            this.textBoxZoek.Size = new System.Drawing.Size(285, 29);
            this.textBoxZoek.TabIndex = 1;
            this.textBoxZoek.TextChanged += new System.EventHandler(this.textBoxZoek_TextChanged);
            this.textBoxZoek.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxZoek_KeyUp);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
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
            this.splitContainer1.Size = new System.Drawing.Size(766, 767);
            this.splitContainer1.SplitterDistance = 132;
            this.splitContainer1.TabIndex = 0;
            // 
            // textBoxLiturgie
            // 
            this.textBoxLiturgie.ContextMenuStrip = this.contextMenuStripRightMouse;
            this.textBoxLiturgie.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxLiturgie.Location = new System.Drawing.Point(0, 0);
            this.textBoxLiturgie.Margin = new System.Windows.Forms.Padding(6);
            this.textBoxLiturgie.Multiline = true;
            this.textBoxLiturgie.Name = "textBoxLiturgie";
            this.textBoxLiturgie.Size = new System.Drawing.Size(766, 631);
            this.textBoxLiturgie.TabIndex = 1;
            this.textBoxLiturgie.MouseClick += new System.Windows.Forms.MouseEventHandler(this.textBoxLiturgie_MouseClick);
            this.textBoxLiturgie.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxLiturgie_KeyPress);
            this.textBoxLiturgie.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.textBoxLiturgie_PreviewKeyDown);
            // 
            // timerUpdateSelection
            // 
            this.timerUpdateSelection.Interval = 20;
            this.timerUpdateSelection.Tick += new System.EventHandler(this.timerUpdateSelection_Tick);
            // 
            // contextMenuStripRightMouse
            // 
            this.contextMenuStripRightMouse.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.contextMenuStripRightMouse.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemKnippen,
            this.toolStripMenuItemKopieren,
            this.toolStripMenuItemPlakken});
            this.contextMenuStripRightMouse.Name = "contextMenuStrip1";
            this.contextMenuStripRightMouse.Size = new System.Drawing.Size(168, 106);
            // 
            // toolStripMenuItemKnippen
            // 
            this.toolStripMenuItemKnippen.Name = "toolStripMenuItemKnippen";
            this.toolStripMenuItemKnippen.Size = new System.Drawing.Size(205, 34);
            this.toolStripMenuItemKnippen.Text = "Knippen";
            this.toolStripMenuItemKnippen.Click += new System.EventHandler(this.toolStripMenuItemKnippen_Click);
            // 
            // toolStripMenuItemKopieren
            // 
            this.toolStripMenuItemKopieren.Name = "toolStripMenuItemKopieren";
            this.toolStripMenuItemKopieren.Size = new System.Drawing.Size(205, 34);
            this.toolStripMenuItemKopieren.Text = "Kopieren";
            this.toolStripMenuItemKopieren.Click += new System.EventHandler(this.toolStripMenuItemKopieren_Click);
            // 
            // toolStripMenuItemPlakken
            // 
            this.toolStripMenuItemPlakken.Name = "toolStripMenuItemPlakken";
            this.toolStripMenuItemPlakken.Size = new System.Drawing.Size(205, 34);
            this.toolStripMenuItemPlakken.Text = "Plakken";
            this.toolStripMenuItemPlakken.Click += new System.EventHandler(this.toolStripMenuItemPlakken_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(135, 25);
            this.label1.TabIndex = 5;
            this.label1.Text = "Zoek en wijzig";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 85);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(133, 25);
            this.label2.TabIndex = 6;
            this.label2.Text = "Hoe invoegen";
            // 
            // textBoxOpties
            // 
            this.textBoxOpties.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOpties.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.textBoxOpties.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.textBoxOpties.Location = new System.Drawing.Point(450, 35);
            this.textBoxOpties.Margin = new System.Windows.Forms.Padding(6);
            this.textBoxOpties.Name = "textBoxOpties";
            this.textBoxOpties.ReadOnly = true;
            this.textBoxOpties.Size = new System.Drawing.Size(128, 29);
            this.textBoxOpties.TabIndex = 7;
            // 
            // LiturgieEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "LiturgieEdit";
            this.Size = new System.Drawing.Size(766, 767);
            this.Load += new System.EventHandler(this.LiturgieEdit_Load);
            this.Leave += new System.EventHandler(this.LiturgieEdit_Leave);
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
        private System.Windows.Forms.Button buttonWijzigOpties;
        private System.Windows.Forms.Button buttonInvoegen;
        private System.Windows.Forms.TextBox textBoxZoek;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox textBoxLiturgie;
        private System.Windows.Forms.Timer timerUpdateSelection;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripRightMouse;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemKnippen;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemKopieren;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPlakken;
        private System.Windows.Forms.Button buttonVervangen;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxOpties;
    }
}
