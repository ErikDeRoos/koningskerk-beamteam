﻿namespace PowerpointGenerater
{
    partial class MaskInvoer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MaskInvoer));
            this.TxtBoxRealName = new System.Windows.Forms.TextBox();
            this.TxtBoxVirtualName = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.maskEditPnl = new System.Windows.Forms.Panel();
            this.omschrijvingLabel = new System.Windows.Forms.Label();
            this.toevoegenBtn = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.maskEditPnl.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // TxtBoxRealName
            // 
            this.TxtBoxRealName.Location = new System.Drawing.Point(13, 18);
            this.TxtBoxRealName.Name = "TxtBoxRealName";
            this.TxtBoxRealName.Size = new System.Drawing.Size(239, 20);
            this.TxtBoxRealName.TabIndex = 0;
            // 
            // TxtBoxVirtualName
            // 
            this.TxtBoxVirtualName.Location = new System.Drawing.Point(13, 63);
            this.TxtBoxVirtualName.Name = "TxtBoxVirtualName";
            this.TxtBoxVirtualName.Size = new System.Drawing.Size(239, 20);
            this.TxtBoxVirtualName.TabIndex = 1;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(13, 96);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(64, 26);
            this.button1.TabIndex = 2;
            this.button1.Text = "Wijzigen";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Echte naam";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Virtuele naam";
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(12, 30);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(303, 108);
            this.listBox1.TabIndex = 5;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(358, 12);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(91, 30);
            this.button2.TabIndex = 6;
            this.button2.Text = "Annuleren";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button3.Location = new System.Drawing.Point(455, 12);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(137, 30);
            this.button3.TabIndex = 7;
            this.button3.Text = "Wijzigingen opslaan";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            this.button5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button5.Location = new System.Drawing.Point(12, 144);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(64, 30);
            this.button5.TabIndex = 9;
            this.button5.Text = "Verwijder";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // maskEditPnl
            // 
            this.maskEditPnl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.maskEditPnl.Controls.Add(this.TxtBoxRealName);
            this.maskEditPnl.Controls.Add(this.TxtBoxVirtualName);
            this.maskEditPnl.Controls.Add(this.button1);
            this.maskEditPnl.Controls.Add(this.label1);
            this.maskEditPnl.Controls.Add(this.label2);
            this.maskEditPnl.Location = new System.Drawing.Point(321, 12);
            this.maskEditPnl.Name = "maskEditPnl";
            this.maskEditPnl.Size = new System.Drawing.Size(268, 128);
            this.maskEditPnl.TabIndex = 10;
            // 
            // omschrijvingLabel
            // 
            this.omschrijvingLabel.AutoSize = true;
            this.omschrijvingLabel.Location = new System.Drawing.Point(13, 11);
            this.omschrijvingLabel.Name = "omschrijvingLabel";
            this.omschrijvingLabel.Size = new System.Drawing.Size(89, 13);
            this.omschrijvingLabel.TabIndex = 11;
            this.omschrijvingLabel.Text = "Gebruikte masks:";
            // 
            // toevoegenBtn
            // 
            this.toevoegenBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.toevoegenBtn.Location = new System.Drawing.Point(82, 144);
            this.toevoegenBtn.Name = "toevoegenBtn";
            this.toevoegenBtn.Size = new System.Drawing.Size(89, 30);
            this.toevoegenBtn.TabIndex = 12;
            this.toevoegenBtn.Text = "Toevoegen";
            this.toevoegenBtn.UseVisualStyleBackColor = true;
            this.toevoegenBtn.Click += new System.EventHandler(this.toevoegenBtn_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.panel1.Controls.Add(this.button3);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Location = new System.Drawing.Point(-3, 181);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(604, 48);
            this.panel1.TabIndex = 13;
            // 
            // MaskInvoer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(601, 228);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.toevoegenBtn);
            this.Controls.Add(this.omschrijvingLabel);
            this.Controls.Add(this.maskEditPnl);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.listBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MaskInvoer";
            this.Text = "Tekstvervangingen (masks)";
            this.maskEditPnl.ResumeLayout(false);
            this.maskEditPnl.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TxtBoxRealName;
        private System.Windows.Forms.TextBox TxtBoxVirtualName;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button5;
        public System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Panel maskEditPnl;
        private System.Windows.Forms.Label omschrijvingLabel;
        private System.Windows.Forms.Button toevoegenBtn;
        private System.Windows.Forms.Panel panel1;
    }
}