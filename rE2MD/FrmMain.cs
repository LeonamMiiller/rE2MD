using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace rE2MD
{
    public partial class FrmMain : Form
    {
        EMDRenderer renderer;
        public FrmMain()
        {
            InitializeComponent();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            this.Show();
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.Filter = "Modelos do Resident Evil 2|*.emd";
            if (openDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                renderer = new EMDRenderer();
                renderer.initialize(Screen.Handle, Screen.Width, Screen.Height);
                renderer.load(openDlg.FileName);
                renderer.render();
            }
            else
            {
                this.Close();
            }
        }
    }
}
