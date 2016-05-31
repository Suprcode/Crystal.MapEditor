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
    public partial class FrmSetLight : Form
    {
        private readonly Main.DelSetLightProperty _delSetLightProperty;
        public FrmSetLight()
        {
            InitializeComponent();
        }

        public FrmSetLight(Main.DelSetLightProperty delSetLightProperty)
        {
            InitializeComponent();
            _delSetLightProperty = delSetLightProperty;
        }

        private void btnSetLight_Click(object sender, EventArgs e)
        {
            byte light;
            if (txtLight.Text.Trim()!=String.Empty)
            {
                light = Convert.ToByte(txtLight.Text.Trim());
                _delSetLightProperty(light);
            }
        }
    }
}
