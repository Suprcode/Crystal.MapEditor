using System;
using System.Windows.Forms;

namespace Map_Editor
{
    public partial class FrmSetAnimation : Form
    {
        private Main.DelSetAnimationProperty _delSetAnimationProperty;
        public FrmSetAnimation()
        {
            InitializeComponent();
        }

        public FrmSetAnimation(Main.DelSetAnimationProperty delSetAnimationProperty)
        {
            InitializeComponent();
            _delSetAnimationProperty = delSetAnimationProperty;
        }

        private void btnSetAnimation_Click(object sender, System.EventArgs e)
        {
            bool blend;
            byte frame;
            byte tick;
            if (txtAnimationFrame.Text.Trim() != String.Empty)
            {
                if (txtAnimationTick.Text.Trim() != String.Empty)
                {
                    blend = chkBlend.Checked;
                    frame = Convert.ToByte(txtAnimationFrame.Text.Trim());
                    tick = Convert.ToByte(txtAnimationTick.Text.Trim());
                    _delSetAnimationProperty(blend, frame, tick);
                }
            }
        }
    }
}
