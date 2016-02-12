using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.ComponentModel;
using ISettings;

namespace PowerpointGenerater {
  public partial class MaskInvoer : Form {
    private readonly BindingList<IMapmask> _maskLijst;
    public IEnumerable<IMapmask> Masks => _maskLijst.ToList();
      private Mapmask _huidigeMask;

    public MaskInvoer(IEnumerable<IMapmask> masks) {
      InitializeComponent();
      _maskLijst = new BindingList<IMapmask>(masks.ToList());
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
      TxtBoxRealName.Text = string.Empty;
      TxtBoxVirtualName.Text = string.Empty;
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
