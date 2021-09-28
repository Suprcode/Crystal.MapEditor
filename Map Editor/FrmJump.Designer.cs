namespace Map_Editor
{
    partial class FrmJump
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.txtX = new System.Windows.Forms.TextBox();
            this.txtY = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnJump = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(36, 33);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(17, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "X";
            // 
            // txtX
            // 
            this.txtX.Location = new System.Drawing.Point(59, 29);
            this.txtX.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtX.Name = "txtX";
            this.txtX.Size = new System.Drawing.Size(63, 22);
            this.txtX.TabIndex = 1;
            this.txtX.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtX_KeyDown);
            // 
            // txtY
            // 
            this.txtY.Location = new System.Drawing.Point(59, 65);
            this.txtY.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtY.Name = "txtY";
            this.txtY.Size = new System.Drawing.Size(63, 22);
            this.txtY.TabIndex = 3;
            this.txtY.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtY_KeyDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(36, 69);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Y";
            // 
            // btnJump
            // 
            this.btnJump.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnJump.Location = new System.Drawing.Point(144, 47);
            this.btnJump.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnJump.Name = "btnJump";
            this.btnJump.Size = new System.Drawing.Size(100, 31);
            this.btnJump.TabIndex = 4;
            this.btnJump.Text = "Jump";
            this.btnJump.UseVisualStyleBackColor = true;
            this.btnJump.Click += new System.EventHandler(this.btnJump_Click);
            // 
            // FrmJump
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(281, 113);
            this.Controls.Add(this.btnJump);
            this.Controls.Add(this.txtY);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtX);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmJump";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FrmJump";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtX;
        private System.Windows.Forms.TextBox txtY;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnJump;
    }
}