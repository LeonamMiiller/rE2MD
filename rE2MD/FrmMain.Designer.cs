namespace rE2MD
{
    partial class FrmMain
    {
        /// <summary>
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">verdade se for necessário descartar os recursos gerenciados; caso contrário, falso.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte do Designer - não modifique
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.Screen = new System.Windows.Forms.PictureBox();
            this.panel = new System.Windows.Forms.Panel();
            this.comboAnimGroup = new System.Windows.Forms.ComboBox();
            this.comboAnimIndex = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.Screen)).BeginInit();
            this.panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // Screen
            // 
            this.Screen.BackColor = System.Drawing.Color.Black;
            this.Screen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Screen.Location = new System.Drawing.Point(0, 24);
            this.Screen.Name = "Screen";
            this.Screen.Size = new System.Drawing.Size(784, 418);
            this.Screen.TabIndex = 0;
            this.Screen.TabStop = false;
            this.Screen.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Screen_MouseDown);
            this.Screen.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Screen_MouseMove);
            this.Screen.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Screen_MouseUp);
            this.Screen.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.Screen_MouseWheel);
            // 
            // panel
            // 
            this.panel.BackColor = System.Drawing.Color.Black;
            this.panel.Controls.Add(this.comboAnimIndex);
            this.panel.Controls.Add(this.comboAnimGroup);
            this.panel.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel.Location = new System.Drawing.Point(0, 0);
            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(784, 24);
            this.panel.TabIndex = 1;
            // 
            // comboAnimGroup
            // 
            this.comboAnimGroup.BackColor = System.Drawing.Color.Black;
            this.comboAnimGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboAnimGroup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboAnimGroup.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.comboAnimGroup.FormattingEnabled = true;
            this.comboAnimGroup.Location = new System.Drawing.Point(3, 1);
            this.comboAnimGroup.Name = "comboAnimGroup";
            this.comboAnimGroup.Size = new System.Drawing.Size(100, 21);
            this.comboAnimGroup.TabIndex = 0;
            this.comboAnimGroup.SelectedIndexChanged += new System.EventHandler(this.comboAnimGroup_SelectedIndexChanged);
            // 
            // comboAnimIndex
            // 
            this.comboAnimIndex.BackColor = System.Drawing.Color.Black;
            this.comboAnimIndex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboAnimIndex.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboAnimIndex.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.comboAnimIndex.FormattingEnabled = true;
            this.comboAnimIndex.Location = new System.Drawing.Point(109, 1);
            this.comboAnimIndex.Name = "comboAnimIndex";
            this.comboAnimIndex.Size = new System.Drawing.Size(100, 21);
            this.comboAnimIndex.TabIndex = 1;
            this.comboAnimIndex.SelectedIndexChanged += new System.EventHandler(this.comboAnimIndex_SelectedIndexChanged);
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 442);
            this.Controls.Add(this.Screen);
            this.Controls.Add(this.panel);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaximizeBox = false;
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "rE2MD";
            this.Load += new System.EventHandler(this.FrmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.Screen)).EndInit();
            this.panel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox Screen;
        private System.Windows.Forms.Panel panel;
        private System.Windows.Forms.ComboBox comboAnimGroup;
        private System.Windows.Forms.ComboBox comboAnimIndex;
    }
}

