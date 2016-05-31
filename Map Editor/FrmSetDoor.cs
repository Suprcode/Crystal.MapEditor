using System;
using System.Windows.Forms;

namespace Map_Editor
{
    public partial class FrmSetDoor : Form
    {
        private Main.DelSetDoorProperty _delSetDoorProperty;
        public FrmSetDoor()
        {
            InitializeComponent();
        }

        public FrmSetDoor(Main.DelSetDoorProperty delSetDoorProperty)
        {
            InitializeComponent();
            _delSetDoorProperty = delSetDoorProperty;
        }

        private void btnSetDoor_Click(object sender, EventArgs e)
        {
            bool blCoreDoor;
            byte index;
            byte offSet;
            if (txtDoorIndex.Text.Trim() != String.Empty)
            {
                if (txtDoorOffSet.Text.Trim()!=String.Empty)
                {
                    blCoreDoor = chkCoreDoor.Checked;
                    index = Convert.ToByte(txtDoorIndex.Text.Trim());
                    offSet = Convert.ToByte(txtDoorOffSet.Text.Trim());
                    _delSetDoorProperty(blCoreDoor, index, offSet);
                }
            }
        }
    }
}
