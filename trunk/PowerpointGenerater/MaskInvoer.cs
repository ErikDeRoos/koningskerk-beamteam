using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PowerpointGenerater
{
    public partial class MaskInvoer : Form
    {
        public MaskInvoer(List<Mapmask> masks)
        {
            InitializeComponent();
            foreach (Mapmask mask in masks)
            {
                listBox1.Items.Add(mask);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add(new Mapmask(TxtBoxVirtualName.Text, TxtBoxRealName.Text));
            TxtBoxRealName.Text = "";
            TxtBoxVirtualName.Text = "";
        }
        private void button4_Click(object sender, EventArgs e)
        {
            object obj = listBox1.SelectedItem;
            if (obj is Mapmask)
            {
                Mapmask mask = (Mapmask)obj;
                TxtBoxRealName.Text = mask.RealName;
                TxtBoxVirtualName.Text = mask.Name;

                listBox1.Items.Remove(mask);
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            listBox1.Items.Remove(listBox1.SelectedItem);
        }
    }
}
