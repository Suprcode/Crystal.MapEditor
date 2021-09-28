using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Map_Editor
{
    public partial class FrmJump : Form
    {
        private Main.DelJump _delJump;
        public FrmJump()
        {
            InitializeComponent();
        }

        public FrmJump(Main.DelJump delJump)
        {
            InitializeComponent();
            _delJump = delJump;
        }

        private void btnJump_Click(object sender, EventArgs e)
        {
            if (txtX.Text.Trim() != String.Empty)
            {
                if (txtY.Text.Trim() != String.Empty)
                {
                    int x = Convert.ToInt32(txtX.Text.Trim());
                    int y = Convert.ToInt32(txtY.Text.Trim());
                    _delJump(x, y);
                    Dispose();
                    //this.Close();
                }
            }
        }

        private void txtX_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    KeyPressEventArgs arg = new KeyPressEventArgs(Convert.ToChar(Keys.Enter));
                    btnJump_Click(sender, arg);
                    break;
            }
        }

        private void txtY_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    KeyPressEventArgs arg = new KeyPressEventArgs(Convert.ToChar(Keys.Enter));
                    btnJump_Click(sender, arg);
                    break;
            }
        }
    }
}
