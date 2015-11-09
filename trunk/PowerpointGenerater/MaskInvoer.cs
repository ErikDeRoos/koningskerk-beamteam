using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.ComponentModel;

namespace PowerpointGenerater {
  public partial class MaskInvoer : Form {
    private BindingList<Mapmask> _maskLijst;
    public IEnumerable<Mapmask> Masks { get { return _maskLijst.ToList(); } }
    private Mapmask _huidigeMask;

    public MaskInvoer(IEnumerable<Mapmask> masks) {
      InitializeComponent();
      _maskLijst = new BindingList<Mapmask>(masks.ToList());
      listBox1.DataSource = _maskLijst;
    }

    private void buttonWijzig_Click(object sender, EventArgs e) {
      if (_huidigeMask == null)
        return;
      _huidigeMask.RealName = TxtBoxRealName.Text;
      _huidigeMask.Name = TxtBoxVirtualName.Text;
      _maskLijst.ResetItem(_maskLijst.IndexOf(_huidigeMask));
    }

    private void buttonVerwijder_Click(object sender, EventArgs e) {
      if (_huidigeMask == null)
        return;
      _maskLijst.Remove(_huidigeMask);
      _huidigeMask = null;
      TxtBoxRealName.Text = String.Empty;
      TxtBoxVirtualName.Text = String.Empty;
    }

    private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
      var mask = listBox1.SelectedItem as Mapmask;
      if (mask != null) {
        _huidigeMask = mask;
        TxtBoxRealName.Text = mask.RealName;
        TxtBoxVirtualName.Text = mask.Name;
      }
    }

    private void toevoegenBtn_Click(object sender, EventArgs e) {
      _huidigeMask = new Mapmask("", "");
      _maskLijst.Add(_huidigeMask);
    }
  }
}
