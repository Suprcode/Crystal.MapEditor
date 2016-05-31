using System;
using System.Windows.Forms;

namespace Map_Editor
{
    public partial class NewFileFrm : Form
    {
        private readonly Main.DelSetMapSize _delSetMapSize;
        public NewFileFrm()
        {
            InitializeComponent();
        }

        public NewFileFrm(Main.DelSetMapSize delSetMapSize)
        {
            InitializeComponent();
            _delSetMapSize = delSetMapSize;
        }
        
        private void btnOk_Click(object sender, EventArgs e)
        {
            int w = Convert.ToInt32(txtWidth.Text.Trim());
            int h = Convert.ToInt32(txtHeight.Text.Trim());
            if (w<=0||h<=0||w>=1000||h>=1000)
            {
                MessageBox.Show("地图限制1000*1000");
            }
            else
            {
                _delSetMapSize(w, h);
                Close();
            }
        }
    }
}
