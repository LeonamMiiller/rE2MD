using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Microsoft.DirectX;

namespace rE2MD
{
    public partial class FrmMain : Form
    {
        EMDRenderer renderer = new EMDRenderer();

        Vector2 initialRotation, initialMovement;
        Vector2 finalRotation, finalMovement;

        private bool firstClick = true;

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
                renderer.initialize(Screen.Handle, Screen.Width, Screen.Height);
                renderer.load(openDlg.FileName);

                for (int i = 0; i < renderer.animations.Length; i++)
                {
                    comboAnimGroup.Items.Add("Grupo #" + i.ToString());
                }

                Application.DoEvents();
                firstClick = false;
                renderer.render();
            }
            else
            {
                this.Close();
            }
        }

        private void comboAnimGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboAnimGroup.SelectedIndex < 0) return;
            comboAnimIndex.SelectedIndex = -1;
            comboAnimIndex.Items.Clear();
            for (int i = 0; i < renderer.animations[comboAnimGroup.SelectedIndex].Length; i++)
            {
                comboAnimIndex.Items.Add("Anim. #" + i.ToString());
            }
        }

        private void comboAnimIndex_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboAnimIndex.SelectedIndex < 0) return;
            renderer.stopAnimation();
            renderer.animationGroup = comboAnimGroup.SelectedIndex;
            renderer.animationIndex = comboAnimIndex.SelectedIndex;
        }

        private void Screen_MouseDown(object sender, MouseEventArgs e)
        {
            if (firstClick) return;
            switch (e.Button)
            {
                case MouseButtons.Left: initialRotation = new Vector2(MousePosition.X, MousePosition.Y); break;
                case MouseButtons.Right: initialMovement = new Vector2(MousePosition.X, MousePosition.Y); break;
            }
        }

        private void Screen_MouseUp(object sender, MouseEventArgs e)
        {
            if (firstClick) return;
            switch (e.Button)
            {
                case MouseButtons.Left: finalRotation = new Vector2(finalRotation.X + (initialRotation.X - MousePosition.X), finalRotation.Y + (initialRotation.Y - MousePosition.Y)); break;
                case MouseButtons.Right: finalMovement = new Vector2(finalMovement.X + (initialMovement.X - MousePosition.X), finalMovement.Y + (initialMovement.Y - MousePosition.Y)); break;
            }
        }

        private void Screen_MouseMove(object sender, MouseEventArgs e)
        {
            if (firstClick) return;
            if (!Screen.Focused) Screen.Select();
            if (renderer != null)
            {
                switch (e.Button)
                {
                    case MouseButtons.Left:
                        renderer.rotation.X = (initialRotation.X - MousePosition.X) + finalRotation.X;
                        renderer.rotation.Y = (initialRotation.Y - MousePosition.Y) + finalRotation.Y;
                        break;
                    case MouseButtons.Right:
                        renderer.translation.X = (initialMovement.X - MousePosition.X) + finalMovement.X;
                        renderer.translation.Y = (initialMovement.Y - MousePosition.Y) + finalMovement.Y;
                        break;
                }
            }
        }

        private void Screen_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0) renderer.zoom += 1.0f; else renderer.zoom -= 1.0f;
        }
    }
}
